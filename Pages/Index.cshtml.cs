using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Proyectamos.Data;
using Proyectamos.Models;
using System;

using Microsoft.EntityFrameworkCore;


namespace Proyectamos.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly ApplicationDbContext _context;

        public IndexModel(ILogger<IndexModel> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public List<Project> HighlightedProjects { get; set; } = new();

        public void OnGet()
        {
            HighlightedProjects = _context.Projects
                .Include(p => p.Category)
                .Where(p => p.Status == 1)
                .OrderByDescending(p => p.Id)
                .Take(5)
                .AsNoTracking()
                .ToList();

            _logger.LogInformation("Se cargaron {count} proyectos destacados", HighlightedProjects.Count);
        }
    }
}