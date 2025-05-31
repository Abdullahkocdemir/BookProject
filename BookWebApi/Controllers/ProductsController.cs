using DataAccessLayer.Context;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DTOLayer.WebApiDTO.ProductDTO;
using AutoMapper;
using System;
using System.Threading.Tasks;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ETicaretDb _context;
        private readonly IMapper _mapper;

        public ProductsController(ETicaretDb context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }


        [HttpGet]
        public async Task<IActionResult> GetListAll()
        {
            var products = await _context.Products.Include(p => p.Category).ToListAsync();
            //// İsteğe bağlı: Product entity'lerini listeleme amaçlı bir DTO'ya mapleyebilirsiniz.
            var productListDTOs = _mapper.Map<List<ResultProductDTO>>(products);
            return Ok(productListDTOs);

        }


        [HttpGet("{id}")] // ID'ye göre ürün getirme metodu eklendi
        public async Task<IActionResult> GetProductById(int id)
        {
            var product = await _context.Products.Include(p => p.Category).FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound("Ürün bulunamadı.");
            }

            // İsteğe bağlı: Tekil ürün detayları için bir DTO'ya mapleyebilirsiniz.
            // Örneğin: var productDetailDTO = _mapper.Map<ProductDetailDTO>(product);
            // return Ok(productDetailDTO);
            return Ok(product);
        }


        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductDTO createProductDTO)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = _mapper.Map<Product>(createProductDTO);

            // Oluşturulma tarihini ve durumu manuel olarak ayarlıyoruz
            product.CreatedDate = DateTime.UtcNow;
            product.Status = true;
            // PopulerProduct zaten DTO'da var ve AutoMapper tarafından mapleniyor.

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Başarılı yanıt ve oluşturulan ürünün ID'sini döndür
            return StatusCode(201, new { Message = "Ürün başarıyla oluşturuldu.", ProductId = product.ProductId });
        }


        [HttpPut]
        public async Task<IActionResult> UpdateProduct([FromBody] UpdateProductDTO updateProductDTO)
        {
            // Fluent Validation, DTO üzerindeki kuralları burada otomatik olarak kontrol eder.
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Ürünü ProductId'ye göre bul
            var existingProduct = await _context.Products.FindAsync(updateProductDTO.ProductId);
            if (existingProduct == null)
            {
                return NotFound("Ürün bulunamadı.");
            }

            // AutoMapper kullanarak DTO'dan mevcut Entity'ye mapleme
            // AutoMapper, UpdateProductDTO'daki alanları existingProduct'a eşler.
            _mapper.Map(updateProductDTO, existingProduct);

            // Güncellenme tarihini manuel olarak ayarlıyoruz
            existingProduct.UpdatedDate = DateTime.UtcNow;
            // Status'u DTO'dan almıyoruz, çünkü bu genellikle yönetim paneli tarafından ayrı olarak yönetilir.
            // existingProduct.Status = updateProductDTO.Status; // Eğer DTO'dan almak istiyorsanız

            _context.Products.Update(existingProduct);
            await _context.SaveChangesAsync();

            return Ok("Ürün başarıyla güncellendi.");
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var productToDelete = await _context.Products.FindAsync(id);
            if (productToDelete == null)
            {
                return NotFound("Ürün bulunamadı.");
            }

            _context.Products.Remove(productToDelete);
            await _context.SaveChangesAsync();

            return Ok("Ürün başarıyla silindi.");
        }
    }
}