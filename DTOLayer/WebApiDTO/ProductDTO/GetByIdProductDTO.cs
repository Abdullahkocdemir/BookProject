﻿using System;

namespace DTOLayer.WebApiDTO.ProductDTO
{
    public class GetByIdProductDTO
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageURl { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool Status { get; set; }
        public bool PopulerProduct { get; set; }
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
    }

    // Kategori detaylarını dahil etmeye karar verirseniz basit bir CategoryDTO örneği
    // public class CategoryDTO
    // {
    //     // Kategori ID'si
    //     public int CategoryId { get; set; }
    //     // Kategori adı
    //     public string CategoryName { get; set; } = string.Empty;
    // }
}