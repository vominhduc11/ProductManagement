using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProductManagement.Data;
using ProductManagement.Models;
using ProductManagement.Models.ViewModels;
using System.Linq;
using System.Threading.Tasks;

namespace ProductManagement.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin
        public async Task<IActionResult> Index()
        {
            // Đếm tổng số sản phẩm
            int totalProducts = await _context.Products.CountAsync();

            // Đếm tổng số danh mục
            int totalCategories = await _context.Categories.CountAsync();

            // Đếm tổng số người dùng
            int totalUsers = await _userManager.Users.CountAsync();

            // Lấy 5 sản phẩm mới nhất
            var latestProducts = await _context.Products
                .Include(p => p.Category)
                .OrderByDescending(p => p.Id)
                .Take(5)
                .ToListAsync();

            // Thống kê theo danh mục
            var categoryStats = await _context.Categories
                .Select(c => new {
                    c.Name,
                    ProductCount = c.Products.Count
                })
                .OrderByDescending(x => x.ProductCount)
                .ToListAsync();

            var viewModel = new AdminDashboardViewModel
            {
                TotalProducts = totalProducts,
                TotalCategories = totalCategories,
                TotalUsers = totalUsers,
                LatestProducts = latestProducts,
                CategoryStats = categoryStats.Select(x => new CategoryStatViewModel
                {
                    Name = x.Name,
                    ProductCount = x.ProductCount
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: Admin/Users
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var userViewModels = new List<UserViewModel>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                userViewModels.Add(new UserViewModel
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    Email = user.Email,
                    FullName = user.FullName,
                    Roles = roles.ToList(),
                    Created = user.Created
                });
            }

            return View(userViewModels);
        }

        // POST: Admin/ToggleAdminRole
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdminRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Kiểm tra xem người dùng có vai trò Admin không
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            if (isAdmin)
            {
                // Nếu là Admin, gỡ bỏ vai trò Admin
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            }
            else
            {
                // Nếu không phải Admin, thêm vai trò Admin
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            return RedirectToAction(nameof(Users));
        }

        // POST: Admin/DeleteUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            // Kiểm tra xem có phải tài khoản đang đăng nhập không
            if (user.UserName == User.Identity.Name)
            {
                ModelState.AddModelError(string.Empty, "Không thể xóa tài khoản đang đăng nhập.");
                return RedirectToAction(nameof(Users));
            }

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                ModelState.AddModelError(string.Empty, "Có lỗi xảy ra khi xóa tài khoản.");
            }

            return RedirectToAction(nameof(Users));
        }
    }
}