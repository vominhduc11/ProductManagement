﻿// Controllers/HomeController.cs
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Data;
using ProductManagement.Models;
using System.Diagnostics;

namespace ProductManagement.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ApplicationDbContext context, ILogger<HomeController> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IActionResult> Index(int? categoryId, string searchString)
        {
            // Chuyển hướng Admin đến trang quản trị
            if (User.IsInRole("Admin"))
            {
                return RedirectToAction("Index", "Admin");
            }

            var productsQuery = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            // Lọc theo danh mục
            if (categoryId.HasValue)
            {
                productsQuery = productsQuery.Where(p => p.CategoryId == categoryId.Value);
                ViewData["SelectedCategoryId"] = categoryId.Value;
            }

            // Tìm kiếm theo tên hoặc mô tả
            if (!string.IsNullOrEmpty(searchString))
            {
                productsQuery = productsQuery.Where(p => p.Name.Contains(searchString)
                                                      || p.Description.Contains(searchString));
                ViewData["SearchString"] = searchString;
            }

            var categories = await _context.Categories.ToListAsync();
            ViewData["Categories"] = categories;

            var products = await productsQuery.ToListAsync();
            return View(products);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}