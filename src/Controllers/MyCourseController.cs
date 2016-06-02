using Microsoft.AspNet.Mvc;

namespace Grp.L2PSite.MobileApp.Controllers
{
    public class MyCourseController : Controller
    {

        public IActionResult ShowSubject(string id)
        {
            ViewData["CourseId"] = id;
            AppVariables.cId = id;
            return View();
        }

        public IActionResult LearningMaterials()
        {
            return View();
        }
        public IActionResult SharedDocuments()
        {
            return View();
        }

        public IActionResult Hyperlinks()
        {
            return View();
        }

        public IActionResult Literature()
        {
            return View();
        }
        public IActionResult MediaLibrary()
        {
            return View();
        }

        public IActionResult Assignments()
        {
            return View();
        }

    }
}
