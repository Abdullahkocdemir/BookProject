using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.Entities
{
    public class AppRole : IdentityRole
    {
        [Required]
        [Display(Name = "Rol Açıklaması")]
        [StringLength(256)]
        public string Description { get; set; } = string.Empty;
        public virtual ICollection<IdentityUserRole<string>> UserRoles { get; set; } = new List<IdentityUserRole<string>>();
    }
}
