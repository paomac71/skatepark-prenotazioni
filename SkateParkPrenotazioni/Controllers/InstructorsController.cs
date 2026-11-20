using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkateParkPrenotazioni.Data;

namespace SkateParkPrenotazioni.Controllers;

public class InstructorsController : Controller
{
    private readonly ApplicationDbContext _db;

    public InstructorsController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index()
    {
        var instructors = await _db.Instructors
            .AsNoTracking()
            .Where(i => i.IsActive)
            .OrderBy(i => i.Name)
            .ToListAsync();

        return View(instructors);
    }
}
