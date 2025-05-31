using BusinessLayer.Abstract; // IProductService için eklendi
using DTOLayer.WebApiDTO.ProductDTO;
using EntityLayer.Entities; // Product varlığını doğrudan kullanıyorsanız gerekli
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization; // DTO dönüşümleri için hala gerekli

namespace WebApi.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService; // Servis katmanını inject ediyoruz
        private readonly IMapper _mapper; // DTO dönüşümleri için Mapper'ı tutuyoruz

        public ProductsController(IProductService productService, IMapper mapper)
        {
            _productService = productService;
            _mapper = mapper;
        }

        [HttpGet]
        public IActionResult GetListAll()
        {
            var values = _mapper.Map<List<ResultProductDTO>>(_productService.BGetProductWithCategory());
            return Ok(values);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await Task.FromResult(_productService.BGetById(id));

            if (product == null)
            {
                return NotFound("Ürün bulunamadı.");
            }

            // Entity'yi DTO'ya dönüştürerek tutarlı bir API yanıtı sağlıyoruz.
            var productDetailDTO = _mapper.Map<ResultProductDTO>(product); // Şimdilik ResultProductDTO'yu yeniden kullanıyoruz
            return Ok(productDetailDTO);
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDTO createProductDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = _mapper.Map<Product>(createProductDTO);

            // Oluşturulma tarihi ve durum gibi iş mantığı ideal olarak servis katmanında veya 
            // varlığın kendisinde (örneğin kurucu metotta) yönetilmelidir.
            // Şimdilik burada bırakıldı ancak gelecekteki refactoring için not alınmıştır.
            product.CreatedDate = DateTime.UtcNow;
            product.Status = true;
            product.PopulerProduct = false; // Oluşturulurken varsayılan değer

            // Servis katmanının Add metodunu çağırıyoruz. Senkron metodunu Task.Run ile sarmalıyoruz.
            await Task.Run(() => _productService.BAdd(product));

            return StatusCode(201, new { Message = "Ürün başarıyla oluşturuldu.", ProductId = product.ProductId });
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductDTO updateProductDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Ürünü servis katmanından ID'ye göre buluyoruz.
            var existingProduct = await Task.FromResult(_productService.BGetById(updateProductDTO.ProductId));
            if (existingProduct == null)
            {
                return NotFound("Ürün bulunamadı.");
            }

            // AutoMapper, DTO'daki özellikleri mevcut entity'ye eşler.
            _mapper.Map(updateProductDTO, existingProduct);

            // Güncelleme tarihi gibi iş mantığı ideal olarak servis katmanında veya 
            // varlığın kendisinde yönetilmelidir.
            existingProduct.UpdatedDate = DateTime.UtcNow;

            // Servis katmanının Update metodunu çağırıyoruz.
            await Task.Run(() => _productService.BUpdate(existingProduct));

            return Ok("Ürün başarıyla güncellendi.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var productToDelete = await Task.FromResult(_productService.BGetById(id));
            if (productToDelete == null)
            {
                return NotFound("Ürün bulunamadı.");
            }

            // Servis katmanının Delete metodunu çağırıyoruz.
            await Task.Run(() => _productService.BDelete(productToDelete));

            return Ok("Ürün başarıyla silindi.");
        }
    }
}