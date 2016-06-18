using Microsoft.AspNet.Mvc;
using L2PAPIClient.DataModel;
using Grp.L2PSite.MobileApp.Helpers;
using L2PAPIClient;
using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;

namespace Grp.L2PSite.MobileApp.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> MyCourses(String semId)
        {
            try
            {
                int isLoggedIn = -1;
                Tools.checkIfTokenCookieExists(Request.Cookies, Context);
                if (Context.Session.GetInt32("LoggedIn").HasValue)
                    isLoggedIn = Context.Session.GetInt32("LoggedIn").Value;

                if (isLoggedIn == 1)
                {
                    //remove previously save course id
                    Context.Session.Remove("CourseId");
                    if (Tools.hasCookieToken)
                        await AuthenticationManager.CheckAccessTokenAsync();

                    L2PCourseInfoSetData result = L2PAPIClient.api.Calls.L2PviewAllCourseInfoAsync().Result;
                    L2PCourseInfoSetData resultBySemester = null;
                    if (semId != null)
                    {
                        ViewData["chosenSemesterCode"] = semId;
                        resultBySemester = L2PAPIClient.api.Calls.L2PviewAllCourseIfoBySemesterAsync(semId).Result;
                    }
                    else
                    {
                        resultBySemester = L2PAPIClient.api.Calls.L2PviewAllCourseInfoByCurrentSemester().Result;
                    }
                    ViewData["semestersList"] = result.dataset;
                    ViewData["currentSemesterCourses"] = resultBySemester.dataset;
                    return View();
                }
                else
                {
                    if (Tools.hasCookieToken && !String.IsNullOrEmpty(Config.getRefreshToken()))
                    {
                        await AuthenticationManager.CheckAccessTokenAsync();
                    }
                }
            }
            catch
            {
                Response.Cookies.Delete("CRTID");
                Response.Cookies.Delete("CRAID");
                return View();
            }
            return RedirectToAction(nameof(AccountController.Login), "Account");
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
