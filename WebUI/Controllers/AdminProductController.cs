using Microsoft.AspNetCore.Mvc;

namespace WebUI.Controllers
{
    public class AdminProductController : Controller
    {
        private readonly HttpClient _httpClient;

        public ProductController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }
        public IActionResult Index()
        {
            return View();
        }
    }
}
