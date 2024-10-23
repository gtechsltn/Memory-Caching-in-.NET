using MemoryCacheWebApi.DTOs;
using MemoryCacheWebApi.Models;
using MemoryCacheWebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Reflection;

namespace MemoryCacheWebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class ProductsController : ControllerBase
    {
        private readonly ProductRepository _productRepository;
        private readonly IMemoryCache _memoryCache;
        public ProductsController(ProductRepository productRepository, IMemoryCache mMemoryCache)
        {
            _productRepository = productRepository;
            _memoryCache = mMemoryCache;
        }

        [HttpGet]
        public async Task<ActionResult<PaginatedResponse<Record>>> GetProducts(int pageNumber = 1, int pageSize = 10)
        {
            var records = new List<Product>();
            string cacheKey = $"GetProducts_Records_Page_{pageNumber}Size{pageSize}";

            if (!_memoryCache.TryGetValue(cacheKey, out List<Product> cachedRecords))
            {
                cachedRecords = (await _productRepository.GetPaginatedProducts(pageNumber, pageSize)).ToList();

                var cacheOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
                };
                _memoryCache.Set(cacheKey, cachedRecords, cacheOptions);

                records = cachedRecords;
            }
            else
            {
                _memoryCache.TryGetValue(cacheKey, out List<Product> cachedRecord);

                records = cachedRecord;
            }

            //var products = await _productRepository.GetPaginatedProducts(pageNumber, pageSize);
            var totalCount = await _productRepository.GetTotalProductsCount();

            var response = new PaginatedResponse<Product>
            {
                Items = records,
                TotalCount = totalCount,
                PageSize = pageSize,
                PageNumber = pageNumber
            };




            return Ok(response);
        }

        [HttpGet]
        public async Task<IActionResult> GetData([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 100)
        {
            // Fetch all data from the repository
            var allData = await _productRepository.GetAllData();

            // In-memory pagination using Skip and Take
            var paginatedData = allData.Skip((pageNumber - 1) * pageSize)
                                       .Take(pageSize)
                                       .ToList();

            return Ok(paginatedData);
        }

    }
}
