using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace ProductManagement.Models
{
    public class ApplicationUser : IdentityUser
    {
        // Các thuộc tính bổ sung có thể thêm vào đây
        public string FullName { get; set; }
        public DateTime Created { get; set; } = DateTime.Now;
    }
}
