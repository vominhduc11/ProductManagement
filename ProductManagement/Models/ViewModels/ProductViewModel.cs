using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Models.ViewModels
{
    public class ProductViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm là bắt buộc")]
        [StringLength(200, ErrorMessage = "Tên sản phẩm không được quá 200 ký tự")]
        [Display(Name = "Tên sản phẩm")]
        public string Name { get; set; }

        [Display(Name = "Mô tả")]
        public string Description { get; set; }

        [Required(ErrorMessage = "Giá sản phẩm là bắt buộc")]
        [Range(0, double.MaxValue, ErrorMessage = "Giá phải lớn hơn hoặc bằng 0")]
        [Display(Name = "Giá")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Danh mục sản phẩm là bắt buộc")]
        [Display(Name = "Danh mục")]
        public int CategoryId { get; set; }

        // Thuộc tính cho hình ảnh
        [Display(Name = "Hình ảnh")]
        public IFormFile ImageFile { get; set; }

        // Bỏ [Required] nếu có và đặt giá trị mặc định là chuỗi rỗng
        public string ExistingImagePath { get; set; } = string.Empty;
    }
}