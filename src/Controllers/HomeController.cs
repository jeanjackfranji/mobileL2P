using Microsoft.AspNet.Mvc;
using L2PAPIClient.DataModel;
using Grp.L2PSite.MobileApp.Controllers;

namespace Cik.MazSite.WebApp.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            AppVariables.cId = null;
            bool isActive = (L2PAPIClient.AuthenticationManager.getState() == L2PAPIClient.AuthenticationManager.AuthenticationState.ACTIVE);
           
            if (isActive)
            {
                L2PCourseInfoSetData result = L2PAPIClient.api.Calls.L2PviewAllCourseInfoAsync().Result;
                ViewData["Courses"] = result.dataset;

            }
            return View();
        }

        public IActionResult Calendar()
        {
            return View();
        }

        public IActionResult About()
        {
            ViewData["Message"] = "MobileL2P is a mobile version of the RWTH Aachen L2P.\n\nOur Team Member are:\n  -John Jack Franji\n  -Neetha Baliga Bantwal\n  -Kamar blbel\n  -Pooja Sompura Harisha\n  -Mahbub Hasan\n  -Atul Mohan";
            return View();
        }

        public IActionResult Error()
        {
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
