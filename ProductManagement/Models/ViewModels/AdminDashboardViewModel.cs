using System.Collections.Generic;

namespace ProductManagement.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int TotalCategories { get; set; }
        public int TotalUsers { get; set; }
        public List<Product> LatestProducts { get; set; }
        public List<CategoryStatViewModel> CategoryStats { get; set; }
    }

    public class CategoryStatViewModel
    {
        public string Name { get; set; }
        public int ProductCount { get; set; }
    }

    public class UserViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string FullName { get; set; }
        public List<string> Roles { get; set; }
        public DateTime Created { get; set; }
    }
}