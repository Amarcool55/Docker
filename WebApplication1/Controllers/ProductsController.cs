using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Serilog;
using System.Text;
using System.Text.Json;
using WebApplication1.Models;

namespace WebApplication1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IDistributedCache _cache;

        public ProductsController(AppDbContext context, IDistributedCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            Log.Information("GetProducts() called");
            const string cacheKey = "products";
            string serializedProducts;
            var products = new List<Product>();

            var cachedProducts = await _cache.GetAsync(cacheKey);

            if (cachedProducts != null)
            {
                serializedProducts = Encoding.UTF8.GetString(cachedProducts);
                products = JsonSerializer.Deserialize<List<Product>>(serializedProducts);
            }
            else
            {
                products = await _context.Products.ToListAsync();
                serializedProducts = JsonSerializer.Serialize(products);
                cachedProducts = Encoding.UTF8.GetBytes(serializedProducts);
                var options = new DistributedCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromMinutes(5))
                    .SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                await _cache.SetAsync(cacheKey, cachedProducts, options);
            }
            Log.Information("Returning products");
            return Ok(products);
        }
    }
}
