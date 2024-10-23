using Dapper;
using MemoryCacheWebApi.Data;
using MemoryCacheWebApi.Models;
using System.Data;
using System.Data.SqlClient;

namespace MemoryCacheWebApi.Services
{
    public class ProductRepository
    {
        private readonly DapperContext _connectionString;

        public ProductRepository(DapperContext connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<IEnumerable<Product>> GetPaginatedProducts(int pageNumber, int pageSize)
        {
            using (var db = _connectionString.CreateConnection())
            {
                var sql = "SELECT * FROM Products ORDER BY Id OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;";
                return await db.QueryAsync<Product>(sql, new { Offset = (pageNumber - 1) * pageSize, PageSize = pageSize });
            }
        }

        public async Task<int> GetTotalProductsCount()
        {
            using (var db = _connectionString.CreateConnection())
            {
                var sql = "SELECT COUNT(*) FROM Records;";
                return await db.ExecuteScalarAsync<int>(sql);
            }
        }


        // Fetch all data without pagination
        public async Task<IEnumerable<Product>> GetAllData()
        {
            using (var db = _connectionString.CreateConnection())
            {
                var query = "SELECT * FROM Products";
                return await db.QueryAsync<Product>(query);
            }
        }
    }
}
