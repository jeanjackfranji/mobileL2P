using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;
using L2PAPIClient.DataModel;
using System;
using L2PAPIClient;
using System.Net;

namespace Grp.L2PSite.MobileApp.Controllers
{
    public class MyCoursesController : Controller
    {

        public IActionResult ShowSubject()
        {
            try
            {
                String id = Context.Session.GetString("CourseId");
                ViewData["CourseInfo"] = L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(id).Result;
            }
            catch (Exception ex) {
                ViewData["error"] = ex.Message;
                RedirectToAction(nameof(HomeController.Error), "Error"); }
            return View();
        }

        public IActionResult WhatsNew(string id)
        {
            try
            {
                Context.Session.SetString("CourseId", id);
                ViewData["ChosenCourse"] = L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(id).Result;
                ViewData["CourseWhatsNew"] = L2PAPIClient.api.Calls.L2PwhatsNewSinceAsync(id,180000).Result;
                ViewData["Assignments"] = L2PAPIClient.api.Calls.L2PviewAllAssignments(id).Result;
            }
            catch (Exception ex)
            {
                ViewData["error"] = ex.Message;
                RedirectToAction(nameof(HomeController.Error), "Error");
            }
            return View();
        }

        public ActionResult Downloads(string url, string filename)
        {
            try
            {
                string callURL = Config.L2PEndPoint + "/downloadFile/"+filename+"?accessToken=" + Config.getAccessToken() + "&cid=" + Context.Session.GetString("CourseId") + "&downloadUrl=|"+ url;
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(callURL);
                myHttpWebRequest.MaximumAutomaticRedirections = 1;
                myHttpWebRequest.AllowAutoRedirect = true;
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                return File(myHttpWebResponse.GetResponseStream(), myHttpWebResponse.ContentType, filename);
            }
            catch (Exception ex)
            {
                ViewData["error"] = ex.Message;
                RedirectToAction(nameof(HomeController.Error), "Error");
            }
            return RedirectToAction(nameof(HomeController.MyCourses), "Home");
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

        public IActionResult Announcement()
        {
            return View();
        }
        public IActionResult AddAnnouncement()
        {
            return View();
        }
        public IActionResult AddLiterature()
        {
            return View();
        }
        public IActionResult AddHyperlink()
        {
            return View();
        }

    }
}
