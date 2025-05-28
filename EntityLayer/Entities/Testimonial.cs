using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // For Column attribute
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntityLayer.Entities
{
    public class Testimonial
    {
        public int TestimonialId { get; set; }

        [Required]
        [StringLength(100)]
        [Column(TypeName = "VarChar")]
        public string Name { get; set; } = string.Empty;

        [Required]
        [Column(TypeName = "VarChar")]
        [StringLength(100)]
        public string Description { get; set; } = string.Empty;

        [Range(1, 5)]
        public int Rating { get; set; }
    }
}