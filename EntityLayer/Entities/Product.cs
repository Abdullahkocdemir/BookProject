using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.Entities
{
    public class Product
    {
        public int ProductId { get; set; }

        [Required]
        [Column(TypeName = "VarChar")]
        [StringLength(30)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "VarChar")]
        [StringLength(50)]
        public string Author { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "VarChar")]
        [StringLength(600)]
        public string Description { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "VarChar")]
        [StringLength(200)]
        public string ImageURl { get; set; } = string.Empty;
        public bool PopulerProduct { get; set; }

        // Kategori Id'si, zorunlu, foreign key olarak tanımlanır
        [Required]
        [ForeignKey("Category")]
        public int CategoryId { get; set; }

        // Kategori nesnesi
        public virtual Category? Category { get; set; }

    }
}
