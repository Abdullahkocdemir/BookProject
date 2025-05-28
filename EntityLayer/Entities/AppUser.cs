using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.Entities
{
    public class AppUser : IdentityUser
    {
        [Required]
        [Display(Name = "Ad")]
        [StringLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Soyad")]
        [StringLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Display(Name = "Ad Soyad")]
        public string FullName => $"{FirstName} {LastName}";

        [Display(Name = "Aktif Mi?")]
        public bool IsActive { get; set; } = true;

        public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; } = new List<IdentityUserRole<string>>();

    }
}
