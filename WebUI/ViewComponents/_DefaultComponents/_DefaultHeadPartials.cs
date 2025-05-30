using Microsoft.AspNetCore.Mvc;

namespace WebUI.ViewComponents._DefaultComponents
{
    public class _DefaultHeadPartials : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View();
        }
    }
}
