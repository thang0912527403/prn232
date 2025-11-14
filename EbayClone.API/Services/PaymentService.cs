using EbayClone.API.Models;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace EbayClone.API.Services;

public class PaymentService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<PaymentService> _logger;
    private string? _accessToken;
    private DateTime _tokenExpiry = DateTime.MinValue;
    private readonly string _ppclienId;
    private readonly string _ppclienSecret;
    private readonly EbayDbContext _dbContext;

    public PaymentService(HttpClient httpClient, IConfiguration configuration, ILogger<PaymentService> logger, EbayDbContext dbContext)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
        _dbContext = dbContext;

        _ppclienId = _configuration["PayPal:ClientId"] ?? "";
        _ppclienSecret = _configuration["PayPal:ClientSecret"] ?? "";
        var apiUrl = _configuration["PayPal:ApiUrl"] ?? "https://api-m.sandbox.paypal.com";
        _httpClient.BaseAddress = new Uri(apiUrl);
    }


    private async Task<string> GetAccessTokenAsync()
    {
        if (!string.IsNullOrEmpty(_accessToken) && DateTime.UtcNow < _tokenExpiry)
        {
            return _accessToken;
        }

        var clientId = _configuration["PayPal:ClientId"];
        var clientSecret = _configuration["PayPal:ClientSecret"];

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            throw new InvalidOperationException("PayPal credentials not configured");
        }

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}"));

        var request = new HttpRequestMessage(HttpMethod.Post, "/v1/oauth2/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();
        var tokenResponse = JsonSerializer.Deserialize<JsonElement>(content);

        _accessToken = tokenResponse.GetProperty("access_token").GetString();
        var expiresIn = tokenResponse.GetProperty("expires_in").GetInt32();
        _tokenExpiry = DateTime.UtcNow.AddSeconds(expiresIn - 60);

        _logger.LogInformation("PayPal access token obtained");
        return _accessToken!;
    }

    public async Task<(bool Success, string OrderId, string ErrorMessage)> CreatePayPalOrderAsync(decimal amount, string currency = "USD")
    {
        try
        {
            var token = await GetAccessTokenAsync();
            var transactionId = Guid.NewGuid().ToString();

            var orderRequest = new
            {
                intent = "CAPTURE",
                purchase_units = new[]
                {
                    new
                    {
                        reference_id = transactionId,
                        amount = new
                        {
                            currency_code = currency,
                            value = amount.ToString("F2")
                        }
                    }
                },
                application_context = new
                {
                    return_url = "https://localhost:5001/payment/success",
                    cancel_url = "https://localhost:5001/payment/cancel"
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "/v2/checkout/orders");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Content = new StringContent(
                JsonSerializer.Serialize(orderRequest),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var orderResponse = JsonSerializer.Deserialize<JsonElement>(content);
                var orderId = orderResponse.GetProperty("id").GetString();

                _logger.LogInformation($"PayPal order created: {orderId}, TransactionId: {transactionId}");
                return (true, orderId!, string.Empty);
            }

            _logger.LogError($"PayPal order creation failed: {content}");
            return (false, string.Empty, content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating PayPal order");
            return (false, string.Empty, ex.Message);
        }
    }

    public async Task<(bool Success, string CaptureId, string ErrorMessage)> CapturePayPalOrderAsync(string paypalOrderId)
    {
        try
        {
            var token = await GetAccessTokenAsync();
            var transactionId = Guid.NewGuid().ToString();

            var request = new HttpRequestMessage(HttpMethod.Post, $"/v2/checkout/orders/{paypalOrderId}/capture");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("PayPal-Request-Id", transactionId);

            var startTime = DateTime.UtcNow;
            var response = await _httpClient.SendAsync(request);
            var elapsedTime = (DateTime.UtcNow - startTime).TotalSeconds;

            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                var captureResponse = JsonSerializer.Deserialize<JsonElement>(content);
                var captureId = captureResponse
                    .GetProperty("purchase_units")[0]
                    .GetProperty("payments")
                    .GetProperty("captures")[0]
                    .GetProperty("id").GetString();

                _logger.LogInformation($"Payment captured in {elapsedTime:F2}s: {captureId}, TransactionId: {transactionId}");
                return (true, captureId!, string.Empty);
            }

            _logger.LogError($"Payment capture failed: {content}, TransactionId: {transactionId}");
            return (false, string.Empty, content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error capturing payment");
            return (false, string.Empty, ex.Message);
        }
    }

    public async Task<bool> RefundPaymentAsync(string captureId, decimal amount)
    {
        try
        {
            var token = await GetAccessTokenAsync();
            var transactionId = Guid.NewGuid().ToString();

            var refundRequest = new
            {
                amount = new
                {
                    value = amount.ToString("F2"),
                    currency_code = "USD"
                }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, $"/v2/payments/captures/{captureId}/refund");
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            request.Headers.Add("PayPal-Request-Id", transactionId);
            request.Content = new StringContent(
                JsonSerializer.Serialize(refundRequest),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.SendAsync(request);
            var content = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation($"Payment refunded: {captureId}, TransactionId: {transactionId}");
                return true;
            }

            _logger.LogError($"Refund failed: {content}, TransactionId: {transactionId}");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refunding payment");
            return false;
        }
    }

    public decimal CalculateShippingFee(string region)
    {
        var shippingRegion = _dbContext.ShippingRegions
            .FirstOrDefault(s => s.Name.ToLower() == region.ToLower());

        if (shippingRegion == null)
            throw new Exception("Region not found");

        return shippingRegion.Cost;
    }


    public decimal ApplyDiscount(decimal amount, string? couponCode)
    {
        if (string.IsNullOrEmpty(couponCode))
            return 0;

        return couponCode.ToUpper() switch
        {
            "SAVE10" => amount * 0.10m,
            "SAVE20" => amount * 0.20m,
            "FREESHIP" => 0m,
            _ => 0m
        };
    }
}