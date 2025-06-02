using DTOLayer.WebUIDTO.ProductDTO;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using X.PagedList;
using X.PagedList.Extensions;

namespace WebUI.Controllers
{
    public class ProductController : Controller
    {
        private readonly HttpClient _httpClient;

        public ProductController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient("ApiClient");
        }

        public async Task<IActionResult> Index()
        {
            var response = await _httpClient.GetAsync("api/Products");
            if (response.IsSuccessStatusCode)
            {
                var jsonData = await response.Content.ReadAsStringAsync();
                var allProducts = JsonConvert.DeserializeObject<List<ResultProductDTO>>(jsonData);
                return View(allProducts);  // tüm listeyi gönderiyoruz
            }
            return View(new List<ResultProductDTO>());
        }

    }
}
