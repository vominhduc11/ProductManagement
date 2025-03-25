// Controllers/CartController.cs - Cập nhật với yêu cầu đăng nhập
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
    // Thêm Authorize cho tất cả các action trong controller
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

        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            try
            {
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
            _cartService.ClearCart();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new { success = true });
            }

            return RedirectToAction("Index");
        }

        public IActionResult Checkout()
        {
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
            return View();
        }
    }
}