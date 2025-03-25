using System.Collections.Generic;
using System.Linq;

namespace ProductManagement.Models.ViewModels
{
    public class Cart
    {
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        public int TotalItems => Items.Sum(i => i.Quantity);

        public decimal TotalPrice => Items.Sum(i => i.TotalPrice);

        public void AddItem(Product product, int quantity)
        {
            var existingItem = Items.FirstOrDefault(i => i.ProductId == product.Id);

            if (existingItem != null)
            {
                // Nếu sản phẩm đã tồn tại, tăng số lượng
                existingItem.Quantity += quantity;
            }
            else
            {
                // Thêm mới vào giỏ hàng
                Items.Add(new CartItem
                {
                    ProductId = product.Id,
                    ProductName = product.Name,
                    Price = product.Price,
                    Quantity = quantity,
                    ImagePath = product.ImagePath
                });
            }
        }

        public void RemoveItem(int productId)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                Items.Remove(item);
            }
        }

        public void UpdateQuantity(int productId, int quantity)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item != null)
            {
                if (quantity > 0)
                {
                    item.Quantity = quantity;
                }
                else
                {
                    Items.Remove(item);
                }
            }
        }

        public void Clear()
        {
            Items.Clear();
        }
    }
}