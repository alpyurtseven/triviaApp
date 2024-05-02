using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using triviaApp.Models;
using triviaApp.Utils;

namespace triviaApp.Controllers
{
    public class CompetitionController : Controller
    {
        private readonly AppDbContext _context;

        public CompetitionController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> List()
        {
            return View(await _context.Competitions.ToListAsync());
        }

        [HttpGet("Competition/Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var competition = await _context.Competitions
             .Include(c => c.CompetitionCategories)
             .ThenInclude(cc => cc.Category)
             .Include(c => c.CompetitionQuestions)
             .ThenInclude(cq => cq.Question)
             .FirstOrDefaultAsync(c => c.Id == id);

            if (competition == null)
            {
                return RedirectToAction("List", "Competition");
            }

            var competitionQuestions =  competition.CompetitionQuestions.AsEnumerable();
            var competitionCategories = competition.CompetitionCategories.AsEnumerable();

            var viewModel = new CreateCompetitionViewModel
            {
                Competition = competition,
                AvailableCategories = _context.Categories.ToList(),
                Questions = _context.Questions.Include(q => q.Category).AsEnumerable().Select(q => new QuestionSelectionViewModel
                {
                    QuestionId = q.Id,
                    QuestionContent = q.Body,
                    CategoryId = q.CategoryId,
                    IsSelected = competitionQuestions.Where(z=>z.QuestionId == q.Id).Count() > 0
                }).ToList(),
                Categories = _context.Categories.AsEnumerable().Select(c => new CategorySelectionViewModel
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    IsSelected = competitionCategories.Where(z => z.CategoryId == c.Id).Count() > 0
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpGet("Competition/Recreate/{id}")]
        public async Task<IActionResult> Recreate(int id)
        {
            var competition = await _context.Competitions
             .Include(c => c.CompetitionCategories)
             .ThenInclude(cc => cc.Category)
             .Include(c => c.CompetitionQuestions)
             .ThenInclude(cq => cq.Question)
             .FirstOrDefaultAsync(c => c.Id == id);

            if (competition == null)
            {
                return RedirectToAction("List", "Competition");
            }

            var competitionQuestions = competition.CompetitionQuestions.AsEnumerable();
            var competitionCategories = competition.CompetitionCategories.AsEnumerable();

            var viewModel = new CreateCompetitionViewModel
            {
                Competition = competition,
                AvailableCategories = _context.Categories.ToList(),
                Questions = _context.Questions.Include(q => q.Category).AsEnumerable().Select(q => new QuestionSelectionViewModel
                {
                    QuestionId = q.Id,
                    QuestionContent = q.Body,
                    CategoryId = q.CategoryId,
                    IsSelected = competitionQuestions.Where(z => z.QuestionId == q.Id).Count() > 0
                }).ToList(),
                Categories = _context.Categories.AsEnumerable().Select(c => new CategorySelectionViewModel
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    IsSelected = competitionCategories.Where(z => z.CategoryId == c.Id).Count() > 0
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var viewModel = new CreateCompetitionViewModel
            {
                AvailableCategories = _context.Categories.ToList(),
                Questions = _context.Questions.Include(q => q.Category).Select(q => new QuestionSelectionViewModel
                {
                    QuestionId = q.Id,
                    QuestionContent = q.Body,
                    CategoryId = q.CategoryId,
                    IsSelected = false
                }).ToList(),
                Categories = _context.Categories.Select(c => new CategorySelectionViewModel
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    IsSelected = false
                }).ToList()
            };

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Create(CreateCompetitionViewModel viewModel)
        {

            var competition = viewModel.Competition;
            competition.JoinLink = "";
            competition.CompetitionCategories = viewModel.Categories
            .Where(c => c.IsSelected)
            .Select(c => new CompetitionCategory { CategoryId = c.CategoryId, Competition = competition })
            .ToList();

            competition.CompetitionQuestions = viewModel.Questions
            .Where(q => q.IsSelected)
            .Select(q => new CompetitionQuestion { QuestionId = q.QuestionId, Competition = competition })
            .ToList();

            _context.Competitions.Add(competition);
            await _context.SaveChangesAsync();

            string origin = HttpContext.Request.Headers["Origin"].ToString();
            string token = Token.GenerateCompetitionToken(competition.Name);
        
            competition.JoinLink = $"{origin}/App/Join?competitionId={competition.Id}&token={token}";

            _context.Competitions.Update(competition);

            await _context.SaveChangesAsync();

            return RedirectToAction("List", "Competition");
        }

        [HttpPost]
        public async Task<IActionResult> Edit(CreateCompetitionViewModel viewModel)
        {
            var competitionEntity = await _context.Competitions.Include(z=>z.CompetitionCategories).Include(z=>z.CompetitionQuestions).Where(z=>z.Id ==viewModel.Competition.Id).FirstOrDefaultAsync();
            var categories = competitionEntity.CompetitionCategories.ToList();
            var questions = competitionEntity.CompetitionQuestions.ToList();

            if (!string.IsNullOrEmpty(viewModel.Competition.Name) && competitionEntity != null)
            {
                competitionEntity.Name = viewModel.Competition.Name;

                foreach (var competitionCategory in viewModel.Categories)
                {
                    if (categories.All(i => i.CategoryId != competitionCategory.CategoryId))
                    {
                        if (competitionCategory.IsSelected)
                        {
                            competitionEntity.CompetitionCategories.Add(new CompetitionCategory { CategoryId = competitionCategory.CategoryId, Competition = competitionEntity });
                        }
                       
                    }
                }

                foreach (var competitionCategory in viewModel.Categories)
                {
                    if (categories.FirstOrDefault(tu => tu.CategoryId == competitionCategory.CategoryId) == null)
                    {
                    

                        competitionEntity.CompetitionCategories.Remove(new CompetitionCategory { CategoryId = competitionCategory.CategoryId, Competition = competitionEntity });
                    }
                }

                foreach (var competitionQuestion in viewModel.Questions)
                {
                    if (questions.All(i => i.QuestionId != competitionQuestion.QuestionId))
                    {
                        if (competitionQuestion.IsSelected)
                        {
                            competitionEntity.CompetitionQuestions.Add(new CompetitionQuestion { QuestionId = competitionQuestion.QuestionId, Competition = competitionEntity });
                        }

                    }
                }

                foreach (var competitionQuestion in viewModel.Questions)
                {
                    if (!competitionQuestion.IsSelected)
                    {
                        var competitionQuestionId = competitionQuestion.QuestionId;
                        var competitionQuestionEntity = competitionEntity.CompetitionQuestions.Where(z => z.QuestionId == competitionQuestion.QuestionId).FirstOrDefault();

                        competitionEntity.CompetitionQuestions.Remove(competitionQuestionEntity);
                    }
                }

                await _context.SaveChangesAsync();
            }

            return RedirectToAction("List", "Competition");
        }

        [HttpDelete]
        public async Task<IActionResult> Remove(int id)
        {
            var competitionEntity = await _context.Competitions.SingleOrDefaultAsync(z => z.Id == id);

            if (competitionEntity != null)
            {
                _context.Competitions.Remove(competitionEntity);

                await _context.SaveChangesAsync();
            }
         
            return RedirectToAction("List", "Competition");
        }
    }
}

