using Microsoft.AspNet.Mvc;

namespace Grp.L2PSite.MobileApp.Controllers
{
    public class MyCourseController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Message"] = "Your application description page.";
            return View();
        }

        public IActionResult ShowSubject()
        {
            ViewData["Message"] = "Your application description page.";
            return View();
        }
        public IActionResult Hyperlinks()
        {
            ViewData["Message"] = "Your application description page.";
            return View();
        }

        public IActionResult LearningMaterials()
        {
            return View();
        }
        public IActionResult SharedDocuments()
        {
            ViewData["Message"] = "Your application description page.";
            return View();
        }
        


    }
}
