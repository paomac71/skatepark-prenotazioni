using Microsoft.AspNetCore.Mvc;
using SkateParkPrenotazioni.Services;

namespace SkateParkPrenotazioni.Controllers;

public class InstructorsController(IInstructorService instructorService) : Controller
{
    private readonly IInstructorService _instructorService = instructorService;

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var instructors = await _instructorService.GetActiveAsync(cancellationToken);
        return View(instructors);
    }
}
