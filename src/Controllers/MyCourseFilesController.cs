using Microsoft.AspNet.Mvc;

namespace Grp.L2PSite.MobileApp.Controllers
{
    public class MyCourseFilesController : Controller
    {
 
        public IActionResult Hyperlinks()
        {
            ViewData["Message"] = "Your application description page.";
            return View();
        }
        public IActionResult SharedDocuments()
        {
            ViewData["Message"] = "Your application description page.";
            return View();
        }
        


    }
}
