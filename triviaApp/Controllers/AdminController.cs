using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using triviaApp.Models;

namespace triviaApp.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> IndexAsync()
        {
            DashboardViewModel dashboardViewModel = new DashboardViewModel();

            dashboardViewModel.CompetitonNames = await _context.Competitions.AsNoTracking().Where(z=>z.isOver).OrderByDescending(z => z.Id).Take(10).Select(z => z.Name).ToListAsync();
            dashboardViewModel.Leaders = await _context.Scores.AsNoTracking().Include(z => z.Participant).OrderByDescending(z=>z.Points).Take(10).Select(z => new NameAndScore() { Name = z.Participant.Username, Points = z.Points }).ToListAsync();
            dashboardViewModel.LastWinners =  _context.Scores.AsNoTracking()
                .Include(s => s.Participant)
                .AsEnumerable()
                .GroupBy(s => s.CompetitionId)
                .Take(10)
                .Select(g => g.OrderByDescending(s => s.Points).FirstOrDefault())
                .Select(s => new NameAndScore
                {
                    Name = s.Participant.Username,
                    Points = s.Points
                })
                .ToList();

            return View(dashboardViewModel);
        }
    }
}

