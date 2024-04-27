using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using triviaApp.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace triviaApp.Controllers
{
    public class CategoryController : Controller
    {
        private readonly AppDbContext _context;

        public CategoryController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Category
        [HttpGet]
        public async Task<IActionResult> List()
        {
            return View(await _context.Categories.ToListAsync());
        }

        // GET: api/Category/5
        [HttpGet("Category/Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return RedirectToAction("List","Category");
            }

            return View(category);
        }

        // POST: api/Category
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        // POST: api/Category
        [HttpPost]
        public async Task<IActionResult> Create(Category category)
        {
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            return RedirectToAction("List", "Category");
        }

        // POST: api/Category
        [HttpPost]
        public async Task<IActionResult> Edit(Category category)
        {
            var categoryEntity = await _context.Categories.FindAsync(category.Id);

            if (!string.IsNullOrEmpty(category.Name))
            {
                categoryEntity.Name = category.Name;
            }
           
            await _context.SaveChangesAsync();

            return RedirectToAction("List", "Category");
        }

        [HttpDelete]
        public async Task<IActionResult> Remove(int id)
        {
            var categoryEntity = await _context.Categories.SingleOrDefaultAsync(z=>z.Id==id);

            if (categoryEntity != null)
            {
                _context.Categories.Remove(categoryEntity);
            }
        
            await _context.SaveChangesAsync();

            return RedirectToAction("List", "Category");
        }
    }
}

