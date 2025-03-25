using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ProductManagement.Models;
using ProductManagement.Models.ViewModels;
using System;

namespace ProductManagement.Services
{
    public class CartService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _cartSessionKey = "Cart";

        public CartService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Cart GetCart()
        {
            var session = _httpContextAccessor.HttpContext.Session;
            string cartJson = session.GetString(_cartSessionKey);

            if (string.IsNullOrEmpty(cartJson))
            {
                return new Cart();
            }

            try
            {
                return JsonConvert.DeserializeObject<Cart>(cartJson);
            }
            catch
            {
                // Nếu lỗi khi deserialize, trả về giỏ hàng mới
                return new Cart();
            }
        }

        public void SaveCart(Cart cart)
        {
            var session = _httpContextAccessor.HttpContext.Session;
            string cartJson = JsonConvert.SerializeObject(cart);
            session.SetString(_cartSessionKey, cartJson);
        }

        public void AddToCart(Product product, int quantity = 1)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            var cart = GetCart();
            cart.AddItem(product, quantity);
            SaveCart(cart);
        }

        public void RemoveFromCart(int productId)
        {
            var cart = GetCart();
            cart.RemoveItem(productId);
            SaveCart(cart);
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var cart = GetCart();
            cart.UpdateQuantity(productId, quantity);
            SaveCart(cart);
        }

        public void ClearCart()
        {
            var cart = GetCart();
            cart.Clear();
            SaveCart(cart);
        }
    }
}