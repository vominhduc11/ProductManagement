using System.ComponentModel.DataAnnotations;

namespace ProductManagement.Models.ViewModels
{
    public class OrderViewModel
    {
        [Required(ErrorMessage = "Họ tên là bắt buộc")]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
        [Display(Name = "Số điện thoại")]
        [RegularExpression(@"^\d{10,11}$", ErrorMessage = "Số điện thoại không hợp lệ")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Địa chỉ là bắt buộc")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string Address { get; set; }

        [Display(Name = "Ghi chú")]
        public string Notes { get; set; }
    }
}