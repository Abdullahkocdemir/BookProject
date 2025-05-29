using DataAccessLayer.Context;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Added for .Include()
using System.Linq;

namespace WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly ETicaretDb _context;

        public ProductsController(ETicaretDb context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetListAll()
        {
            // Eager loading Category to include it in the response
            var values = _context.Products.Include(p => p.Category).ToList();
            return Ok(values);
        }

        [HttpPost]
        public IActionResult CreateProduct(Product product)
        {
            if (product == null)
            {
                return BadRequest("Product data is null.");
            }

            _context.Products.Add(product);
            _context.SaveChanges();
            return StatusCode(201, "Product created successfully."); // 201 Created status
        }

        [HttpPut]
        public IActionResult UpdateProduct(Product product)
        {
            if (product == null || product.ProductId == 0)
            {
                return BadRequest("Product data is null or ProductId is invalid.");
            }

            var existingProduct = _context.Products.Find(product.ProductId);
            if (existingProduct == null)
            {
                return NotFound("Product not found.");
            }

            existingProduct.Name = product.Name;
            existingProduct.Author = product.Author;
            existingProduct.Description = product.Description;
            existingProduct.ImageURl = product.ImageURl;
            existingProduct.PopulerProduct = product.PopulerProduct;
            existingProduct.CategoryId = product.CategoryId;

            _context.Products.Update(existingProduct);
            _context.SaveChanges();
            return Ok("Product updated successfully.");
        }

        [HttpDelete("{id}")] // Use route parameter for ID
        public IActionResult DeleteProduct(int id)
        {
            var productToDelete = _context.Products.Find(id);
            if (productToDelete == null)
            {
                return NotFound("Product not found.");
            }

            _context.Products.Remove(productToDelete);
            _context.SaveChanges();
            return Ok("Product deleted successfully.");
        }
    }
}