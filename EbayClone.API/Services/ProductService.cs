using EbayClone.API.Models;
using Microsoft.EntityFrameworkCore;
namespace EbayClone.API.Services
{
    public class ProductService
    {
        private readonly ILogger<ProductService> _logger;
        private readonly IConfiguration _configuration;
        private readonly EbayDbContext _dbContext;
        public ProductService(ILogger<ProductService> logger, IConfiguration configuration, EbayDbContext context)
        {
            _logger = logger;
            _configuration = configuration;
            _dbContext = context;
        }
        public async Task<List<Product>> GetProductsAsync(
    int? page,
    int? pagination,
    string? search,
    string? categoryId,
    string? orderBy,
    string? order)
        {
            // Bắt đầu với queryable
            IQueryable<Product> query = _dbContext.Products;

            // Lọc theo category nếu có
            if (!string.IsNullOrEmpty(categoryId))
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            // Lọc theo search (Name hoặc Description)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Description.Contains(search));
            }

            // Sắp xếp nếu có orderBy
            if (!string.IsNullOrEmpty(orderBy))
            {
                bool descending = string.Equals(order, "desc", StringComparison.OrdinalIgnoreCase);

                query = orderBy.ToLower() switch
                {
                    "name" => descending ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
                    "price" => descending ? query.OrderByDescending(p => p.Price) : query.OrderBy(p => p.Price),
                    "sellerid" => descending ? query.OrderByDescending(p => p.SellerId) : query.OrderBy(p => p.SellerId),
                    "auctionendtime" => descending ? query.OrderByDescending(p => p.AuctionEndTime) : query.OrderBy(p => p.AuctionEndTime),
                    _ => query // nếu orderBy không hợp lệ, giữ nguyên
                };
            }

            // Phân trang nếu có
            if (page.HasValue && pagination.HasValue)
            {
                int skip = (page.Value - 1) * pagination.Value;
                query = query.Skip(skip).Take(pagination.Value);
            }

            // Trả kết quả
            return await query.Include(p => p.Seller).ToListAsync();
        }


        public async Task<int> totalProduct()
        {
            return _dbContext.Products.Count();
        }
        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _dbContext.Categories.ToListAsync();
        }
        public async Task<object?> GetProductByIdAsync(string productId)
        {
            productId = productId.ToUpper();

            var product = await _dbContext.Products
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.ProductId == productId);
            if (product == null)
            {
                return null;
            }
            if (product.Seller == null)
            {
                return new
                {
                    product = product,
                    sellername = "Unknown Seller"
                };
            }
            return new
            {
                product = product,
                sellername = product.Seller.Email,
            };
        }
        public async Task<List<ShippingRegion>> GetShippingRegionsAsync()
        {
            return await _dbContext.ShippingRegions.ToListAsync();
        }
    }
}
