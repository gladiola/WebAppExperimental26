using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using REDRFID.Services;

namespace REDRFID.Pages.Red.MapWork
{
    public class MapWidgetModel : PageModel
    {
        private ILogger<MapWidgetModel> _logger;
        private INonceCatalogService _nonceCatalogService;



        public MapWidgetModel(ILogger<MapWidgetModel> logger, INonceCatalogService nonceCatalogService)
        {
            _logger = logger;
            _nonceCatalogService = nonceCatalogService;
        }

        public void OnGet()
        {
        }
    }
}
