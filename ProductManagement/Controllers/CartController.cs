// Controllers/CartController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Data;
using ProductManagement.Models.ViewModels;
using ProductManagement.Services;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ProductManagement.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly CartService _cartService;
        private readonly ILogger<CartController> _logger;

        public CartController(
            ApplicationDbContext context,
            CartService cartService,
            ILogger<CartController> logger)
        {
            _context = context;
            _cartService = cartService;
            _logger = logger;
        }

        // Kiểm tra xem người dùng có phải là Admin không
        private bool IsAdminUser()
        {
            return User.IsInRole("Admin");
        }

        // Action filter để kiểm tra quyền trước mỗi action
        private IActionResult CheckUserRole()
        {
            if (IsAdminUser())
            {
                return RedirectToAction("Index", "Admin");
            }

            return null;
        }

        public IActionResult Index()
        {
            // Kiểm tra nếu là Admin thì chuyển hướng
            var checkResult = CheckUserRole();
            if (checkResult != null)
                return checkResult;

            var cart = _cartService.GetCart();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
                // Kiểm tra nếu là Admin thì từ chối
                if (IsAdminUser())
                {
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new { success = false, message = "Tài khoản Admin không thể mua hàng" });
                    }

                    TempData["ErrorMessage"] = "Tài khoản Admin không thể mua hàng";
                    return RedirectToAction("Index", "Admin");
                }

                var product = await _context.Products.FindAsync(productId);
                if (product == null)
                {
                    return NotFound("Sản phẩm không tồn tại");
                }

                _cartService.AddToCart(product, quantity);

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var cart = _cartService.GetCart();
                    return Json(new { success = true, cartCount = cart.TotalItems });
                }

                TempData["SuccessMessage"] = "Đã thêm sản phẩm vào giỏ hàng";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi khi thêm sản phẩm vào giỏ hàng");

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Có lỗi xảy ra khi thêm vào giỏ hàng" });
                }

                TempData["ErrorMessage"] = "Có lỗi xảy ra khi thêm vào giỏ hàng";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        public IActionResult RemoveFromCart(int productId)
        {
            // Kiểm tra nếu là Admin thì từ chối
            if (IsAdminUser())
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Tài khoản Admin không thể thực hiện chức năng này" });
                }

                return RedirectToAction("Index", "Admin");
            }

            _cartService.RemoveFromCart(productId);

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var cart = _cartService.GetCart();
                return Json(new { success = true, cartCount = cart.TotalItems, cartTotal = cart.TotalPrice });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            // Kiểm tra nếu là Admin thì từ chối
            if (IsAdminUser())
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Tài khoản Admin không thể thực hiện chức năng này" });
                }

                return RedirectToAction("Index", "Admin");
            }

            if (quantity <= 0)
            {
                _cartService.RemoveFromCart(productId);
            }
            else
            {
                _cartService.UpdateQuantity(productId, quantity);
            }

            var cart = _cartService.GetCart();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                decimal itemTotal = cartItem != null ? cartItem.TotalPrice : 0;

                return Json(new
                {
                    success = true,
                    cartCount = cart.TotalItems,
                    cartTotal = cart.TotalPrice,
                    itemTotal = itemTotal
                });
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult ClearCart()
        {
            // Kiểm tra nếu là Admin thì từ chối
            if (IsAdminUser())
            {
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    return Json(new { success = false, message = "Tài khoản Admin không thể thực hiện chức năng này" });
                }

                return RedirectToAction("Index", "Admin");
            }

            _cartService.ClearCart();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
            // Kiểm tra nếu là Admin thì từ chối
            var checkResult = CheckUserRole();
            if (checkResult != null)
                return checkResult;

            var cart = _cartService.GetCart();
            if (cart.Items.Count == 0)
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống";
                return RedirectToAction("Index");
            }

            return View(new OrderViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Checkout(OrderViewModel model)
        {
            // Kiểm tra nếu là Admin thì từ chối
            if (IsAdminUser())
            {
                TempData["ErrorMessage"] = "Tài khoản Admin không thể thực hiện chức năng này";
                return RedirectToAction("Index", "Admin");
            }

            var cart = _cartService.GetCart();
            if (cart.Items.Count == 0)
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống";
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                // Ở đây bạn có thể thêm logic lưu đơn hàng vào cơ sở dữ liệu
                // ...

                // Sau khi đặt hàng thành công, xóa giỏ hàng
                _cartService.ClearCart();

                TempData["SuccessMessage"] = "Đặt hàng thành công! Cảm ơn bạn đã mua sắm.";
                return RedirectToAction("OrderSuccess");
            }

            return View(model);
        }

        public IActionResult OrderSuccess()
        {
            // Kiểm tra nếu là Admin thì từ chối
            var checkResult = CheckUserRole();
            if (checkResult != null)
                return checkResult;

            return View();
        }
    }
}