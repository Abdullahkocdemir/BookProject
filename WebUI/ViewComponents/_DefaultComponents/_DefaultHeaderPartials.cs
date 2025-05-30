using Microsoft.AspNetCore.Mvc;

namespace WebUI.ViewComponents._DefaultComponents
{
    public class _DefaultHeaderPartials : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
