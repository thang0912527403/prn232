using EbayClone.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;
using System.Text.Json;

namespace EbayClone.Web.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<HomeController> _logger;
    private readonly string ApiBaseUrl;
    public HomeController(IHttpClientFactory httpClientFactory, ILogger<HomeController> logger, IConfiguration configuration)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        ApiBaseUrl = configuration["ApiBaseUrl"] ?? "http://localhost:5268/api";
    }
    private readonly IConfiguration _configuration;

    public async Task<IActionResult> Index(int? page, int? pagination, string? search, string? categoryId, string? orderBy, string? order)
    {
        if (!page.HasValue) page = 1;
        if (!pagination.HasValue) pagination = 16;
        var client = _httpClientFactory.CreateClient();
        string requestUrl = $"{ApiBaseUrl}Product?" +
                        $"page={page}&" +
                        $"pagination={pagination}&" +
                        $"search={search}&" +
                        $"categoryId={categoryId}&" +
                        $"orderBy={orderBy}&" +
                        $"order={order}";
        var response = await client.GetAsync(requestUrl);
        var json = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };
        Console.WriteLine("pro" + json);
        var products = JsonSerializer.Deserialize<ProductResponse>(json, options);
        string requestUrl2 = $"{ApiBaseUrl}Product/category";
        var response2 = await client.GetAsync(requestUrl2);
        var json2 = await response2.Content.ReadAsStringAsync();
        Console.WriteLine("cat" + json2);
        var categories = JsonSerializer.Deserialize<List<Category>>(json2, options);
        if (products.Total % pagination != 0)
        {
            ViewBag.TotalPages = products.Total / pagination + 1;
        }
        else
        {
            ViewBag.TotalPages = products.Total / pagination;
        }
        ViewBag.CurrentPage = page.Value;
        ViewBag.Pagination = pagination;
        ViewBag.Search = search;
        ViewBag.CategoryId = categoryId;
        ViewBag.OrderBy = orderBy;
        ViewBag.Order = order;
        ViewBag.Categories = categories;
        return View(products.Data);
    }

    public async Task<IActionResult> Product(string id)
    {
        var client = _httpClientFactory.CreateClient();
        string requestUrl = $"{ApiBaseUrl}Product/{id}";
        var response = await client.GetAsync(requestUrl);
        var json = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles
        };
        var product = JsonSerializer.Deserialize<ProductDto>(json, options);

        if (product == null)
        {
            return NotFound();
        }
        requestUrl = $"{ApiBaseUrl}Product/shippingregion";
        response = await client.GetAsync(requestUrl);
        json = await response.Content.ReadAsStringAsync();
        var regions = JsonSerializer.Deserialize<List<ShippingRegion>>(json, options);
        ViewBag.SellerName = product.sellerName;
        ViewBag.ShippingRegions = regions;
        return View(product.product);
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromForm] CreateOrderViewModel model)
    {

        try
        {
            var orders = new Order();
            orders.UserId = model.UserId;
            decimal total = 0;
            foreach (var item in model.Items)
            {
                item.orderId = orders.OrderId;
                total = total + item.Quantity * item.Price;
            }
            orders.Items = model.Items;
            orders.ShippingAddress = model.ShippingAddress;
            orders.Status = OrderStatus.PendingPayment;
            orders.ShippingRegion = model.ShippingRegion;
            orders.TotalAmount = total;
            orders.UserId = "35911088-3782-48AB-BC92-D9CB277A5DCB";
            var client = _httpClientFactory.CreateClient("EbayAPI");
            var json = JsonSerializer.Serialize(orders);
            Console.WriteLine(json);

            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync("orders", content);

            if (response.IsSuccessStatusCode)
            {
                var orderJson = await response.Content.ReadAsStringAsync();
                var order = JsonSerializer.Deserialize<Order>(orderJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return RedirectToAction("OrderDetails", new { id = order?.OrderId });
            }

            TempData["Error"] = "Failed to create order";
            return RedirectToAction("Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            TempData["Error"] = ex.Message;
            return RedirectToAction("Index");
        }
    }

    public async Task<IActionResult> OrderDetails(string id)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("EbayAPI");
            var response = await client.GetAsync($"orders/{id}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var order = JsonSerializer.Deserialize<Order>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return View(order);
            }

            return NotFound();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting order {id}");
            return StatusCode(500);
        }
    }

    public async Task<IActionResult> MyOrders()
    {
        try
        {
            var userId = "buyer@example.com"; // In production, get from authentication
            var client = _httpClientFactory.CreateClient("EbayAPI");
            var response = await client.GetAsync($"orders/user/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var orders = JsonSerializer.Deserialize<List<Order>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<Order>();

                return View(orders);
            }

            return View(new List<Order>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting user orders");
            return View(new List<Order>());
        }
    }

    [HttpPost]
    public async Task<IActionResult> InitiatePayment(string orderId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("EbayAPI");
            var response = await client.PostAsync($"orders/{orderId}/payment/initiate", null);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<JsonElement>(json);
                var paypalOrderId = result.GetProperty("paypalOrderId").GetString();

                // In production, redirect to PayPal approval URL
                TempData["PayPalOrderId"] = paypalOrderId;
                return RedirectToAction("PaymentApproval", new { orderId, paypalOrderId });
            }

            TempData["Error"] = "Failed to initiate payment";
            return RedirectToAction("OrderDetails", new { id = orderId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error initiating payment for order {orderId}");
            TempData["Error"] = ex.Message;
            return RedirectToAction("OrderDetails", new { id = orderId });
        }
    }

    public IActionResult PaymentApproval(string orderId, string paypalOrderId)
    {
        ViewBag.OrderId = orderId;
        ViewBag.PayPalOrderId = paypalOrderId;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CompletePayment(string orderId, string paypalOrderId)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("EbayAPI");
            var payload = new { PayPalOrderId = paypalOrderId };
            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"orders/{orderId}/payment/complete", content);

            if (response.IsSuccessStatusCode)
            {
                TempData["Success"] = "Payment completed successfully!";
                return RedirectToAction("OrderDetails", new { id = orderId });
            }

            TempData["Error"] = "Payment completion failed";
            return RedirectToAction("OrderDetails", new { id = orderId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error completing payment for order {orderId}");
            TempData["Error"] = ex.Message;
            return RedirectToAction("OrderDetails", new { id = orderId });
        }
    }

    private class ProductResponse
    {
        public int Total { get; set; }
        public List<Product> Data { get; set; } = new();
    }
    private class ProductDto
    {
        public Product product { get; set; }
        public string sellerName { get; set; }
    }
}