using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.Entities
{
    public class Category
    {
        public int CategoryId { get; set; }

        [Required]
        [Column(TypeName = "VarChar")]
        [StringLength(15)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "VarChar")]
        [StringLength(100)]
        public string Description { get; set; } = string.Empty;
        [Required]
        [Column(TypeName = "VarChar")]
        [StringLength(20)]
        public string Icon { get; set; } = string.Empty;
    }
}
