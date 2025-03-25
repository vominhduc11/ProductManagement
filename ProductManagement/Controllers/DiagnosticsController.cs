using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Data;
using System.Threading.Tasks;

namespace ProductManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DiagnosticsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<DiagnosticsController> _logger;
        private readonly IConfiguration _configuration;

        public DiagnosticsController(
            ApplicationDbContext context,
            ILogger<DiagnosticsController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<IActionResult> TestDatabase()
        {
            var viewModel = new DatabaseTestViewModel
            {
                ConnectionString = MaskConnectionString(_configuration.GetConnectionString("DefaultConnection")),
                DatabaseProvider = _context.Database.ProviderName,
                ServerVersion = await GetDatabaseVersionAsync()
            };

            try
            {
                // Kiểm tra kết nối
                bool canConnect = await _context.Database.CanConnectAsync();
                viewModel.CanConnect = canConnect;

                if (canConnect)
                {
                    // Kiểm tra DbSets
                    viewModel.CategoryCount = await _context.Categories.CountAsync();
                    viewModel.ProductCount = await _context.Products.CountAsync();
                    viewModel.UserCount = await _context.Users.CountAsync();

                    // Kiểm tra quyền thêm dữ liệu
                    bool canWrite = await TestDatabaseWritePermissionAsync();
                    viewModel.CanWrite = canWrite;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during database diagnostics");
                viewModel.Error = ex.Message;
                if (ex.InnerException != null)
                {
                    viewModel.InnerError = ex.InnerException.Message;
                }
            }

            return View(viewModel);
        }

        private async Task<string> GetDatabaseVersionAsync()
        {
            try
            {
                // SQL Server version query
                var result = await _context.Database.ExecuteSqlRawAsync("SELECT @@VERSION");
                var version = await _context.Database
                    .SqlQuery<string>($"SELECT @@VERSION as Value")
                    .FirstOrDefaultAsync();
                return version ?? "Unknown";
            }
            catch
            {
                return "Could not determine database version";
            }
        }

        private async Task<bool> TestDatabaseWritePermissionAsync()
        {
            try
            {
                // Tạo một danh mục test
                var testCategory = new Models.Category
                {
                    Name = $"Test Category - {Guid.NewGuid()}",
                    Description = "Automatically created to test database write permissions"
                };

                _context.Categories.Add(testCategory);
                await _context.SaveChangesAsync();

                // Xóa danh mục test
                _context.Categories.Remove(testCategory);
                await _context.SaveChangesAsync();

                return true;
            }
            catch
            {
                return false;
            }
        }

        private string MaskConnectionString(string connectionString)
        {
            if (string.IsNullOrEmpty(connectionString))
                return "No connection string configured";

            // Mask password if present
            if (connectionString.Contains("Password=") || connectionString.Contains("pwd="))
            {
                var regex = new System.Text.RegularExpressions.Regex("(Password|pwd)=([^;]*)");
                connectionString = regex.Replace(connectionString, "$1=******");
            }

            return connectionString;
        }
    }

    public class DatabaseTestViewModel
    {
        public string ConnectionString { get; set; }
        public string DatabaseProvider { get; set; }
        public string ServerVersion { get; set; }
        public bool CanConnect { get; set; }
        public bool CanWrite { get; set; }
        public int CategoryCount { get; set; }
        public int ProductCount { get; set; }
        public int UserCount { get; set; }
        public string Error { get; set; }
        public string InnerError { get; set; }
    }
}