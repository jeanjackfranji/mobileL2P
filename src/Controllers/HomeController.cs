using Microsoft.AspNet.Mvc;
using L2PAPIClient.DataModel;

namespace Cik.MazSite.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            bool isActive = (L2PAPIClient.AuthenticationManager.getState() == L2PAPIClient.AuthenticationManager.AuthenticationState.ACTIVE);
            if (isActive)
            {
                L2PCourseInfoSetData result = L2PAPIClient.api.Calls.L2PviewAllCourseInfoAsync().Result;
                ViewData["Courses"] = result.dataset;

            }

            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";
            return View();
        }

        public IActionResult Contact()
        {
            ViewData["Message"] = "Your contact page.";

            return View();
        }

        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
