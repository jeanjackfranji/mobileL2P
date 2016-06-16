using Microsoft.AspNet.Mvc;
using L2PAPIClient.DataModel;
using Grp.L2PSite.MobileApp.Helpers;
using L2PAPIClient;
using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using System.Collections.Generic;
using System.Linq;

namespace Grp.L2PSite.MobileApp.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> MyCourses(String id)
        {
            try
            {
                int isLoggedIn = -1;
                Tools.checkIfTokenCookieExists(Request.Cookies, Context);

                if (Context.Session.GetInt32("LoggedIn").HasValue)
                    isLoggedIn = Context.Session.GetInt32("LoggedIn").Value;

                if (isLoggedIn == 1)
                {
                    Tools.cId = null;
                    if (Tools.hasCookieToken)
                        await AuthenticationManager.CheckAccessTokenAsync();

                    L2PCourseInfoSetData result = L2PAPIClient.api.Calls.L2PviewAllCourseInfoAsync().Result;
                    ViewData["semesters"] = result.dataset;
                    ViewData["chosenSemesterCode"] = id;
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
