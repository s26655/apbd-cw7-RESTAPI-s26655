using Microsoft.AspNetCore.Mvc;
using MiniHelpdesk.Services;
using MiniHelpdesk.ViewModels;

namespace MiniHelpdesk.Controllers;

public class TicketsController : Controller
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var tickets = await _ticketService.GetAllAsync();

        return View(tickets);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new CreateTicketViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateTicketViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            var ticketId = await _ticketService.CreateAsync(model);

            return RedirectToAction(nameof(Details), new { id = ticketId });
        }
        catch (BusinessRuleException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);

            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var details = await _ticketService.GetDetailsAsync(id);

        if (details is null)
        {
            return NotFound();
        }

        return View(details);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(int id)
    {
        var closed = await _ticketService.CloseAsync(id);

        if (!closed)
        {
            return NotFound();
        }

        return RedirectToAction(nameof(Details), new { id });
    }
}