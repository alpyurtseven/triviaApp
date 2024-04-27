using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using triviaApp.Models;

// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace triviaApp.Controllers
{
    public class QuestionController : Controller
    {
        private readonly AppDbContext _context;

        public QuestionController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/Question
        [HttpGet]
        public async Task<IActionResult> List()
        {
            return View(await _context.Questions.Include(z=>z.Category).ToListAsync());
        }

        // GET: api/Question/5
        [HttpGet("Question/Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var question = await _context.Questions.Include(z => z.Category).Where(z => z.Id == id).SingleOrDefaultAsync();
            QuestionViewModel questionViewModel = new QuestionViewModel()
            {
                Question = question,
                Categories = _context.Categories.ToList()
            };

            if (question == null)
            {
                return NotFound();
            }

            return View(questionViewModel);
        }

        // POST: api/Category
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            QuestionViewModel questionViewModel = new QuestionViewModel()
            {
                Question = new Question(),
                Categories = _context.Categories.ToList()
            };

            return View(questionViewModel);
        }


        // POST: api/Question
        [HttpPost]
        public async Task<IActionResult> Create(Question question)
        {
            _context.Questions.Add(question);
            await _context.SaveChangesAsync();

            return RedirectToAction("List","Question");
        }

        // POST: api/Category
        [HttpPost]
        public async Task<IActionResult> Edit(Question question)
        {
            _context.Questions.Update(question);

            await _context.SaveChangesAsync();

            return RedirectToAction("List", "Question");
        }

        [HttpDelete]
        public async Task<IActionResult> Remove(int id)
        {
            var questionEntity = await _context.Questions.SingleOrDefaultAsync(z => z.Id == id);

            if (questionEntity != null)
            {
                _context.Questions.Remove(questionEntity);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction("List", "Question");
        }
    }
}

