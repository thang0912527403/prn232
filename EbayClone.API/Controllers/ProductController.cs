using EbayClone.API.Services;
using Microsoft.AspNetCore.Mvc;
using EbayClone.API.Models;
namespace EbayClone.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController : Controller
    {
        private readonly ProductService _productService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ProductService productService, ILogger<ProductController> logger)
        {
            _productService = productService;
            _logger = logger;
        }
        [HttpGet]
        public async Task<ActionResult<List<Product>>> GetProducts(
       [FromQuery] int? page,
       [FromQuery] int? pagination,
       [FromQuery] string? search,
       [FromQuery] string? categoryId, [FromQuery] string? orderBy, [FromQuery] string? order)
        {
            var totalProducts = await _productService.totalProduct();
            var products = await _productService.GetProductsAsync(page, pagination, search, categoryId, orderBy, order);
            return Ok(new { total = totalProducts, data = products });
        }
        [HttpGet("category")]
        public async Task<ActionResult<List<Product>>> GetCategory()
        {
            var categories = await _productService.GetCategoriesAsync();
            return Ok(categories);
        }
        [HttpGet("{productId}")]
        public async Task<ActionResult<Product>> GetProductById(string productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null)
            {
                return NotFound(new { error = "Product not found" });
            }
            return Ok(product);
        }
        [HttpGet("shippingregion")]
        public async Task<ActionResult<List<ShippingRegion>>> GetShippingRegions()
        {
            var regions = await _productService.GetShippingRegionsAsync();
            return Ok(regions);
        }
    }
}
