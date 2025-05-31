using DataAccessLayer.Context;
using EntityLayer.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Linq; // Added for .ToList()

namespace WebApi.Controllers
{
    [AllowAnonymous]
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ETicaretDb _context;

        public CategoriesController(ETicaretDb context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetListAll()
        {
            var values = _context.Categories.ToList();
            return Ok(values);
        }

        [HttpPost]
        public IActionResult CreateCategory(Category category)
        {
            if (category == null)
            {
                return BadRequest("Category data is null.");
            }

            _context.Categories.Add(category);
            _context.SaveChanges();
            return StatusCode(201, "Category created successfully."); 
        }

        [HttpPut]
        public IActionResult UpdateCategory(Category category)
        {
            if (category == null || category.CategoryId == 0)
            {
                return BadRequest("Category data is null or CategoryId is invalid.");
            }

            var existingCategory = _context.Categories.Find(category.CategoryId);
            if (existingCategory == null)
            {
                return NotFound("Category not found.");
            }

            existingCategory.Name = category.Name;
            existingCategory.Description = category.Description;
            existingCategory.Icon = category.Icon;

            _context.Categories.Update(existingCategory);
            _context.SaveChanges();
            return Ok("Category updated successfully.");
        }

        [HttpDelete("{id}")] 
        public IActionResult DeleteCategory(int id)
        {
            var categoryToDelete = _context.Categories.Find(id);
            if (categoryToDelete == null)
            {
                return NotFound("Category not found.");
            }

            _context.Categories.Remove(categoryToDelete);
            _context.SaveChanges();
            return Ok("Category deleted successfully.");
        }
    }
}