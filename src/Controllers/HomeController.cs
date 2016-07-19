using Microsoft.AspNet.Mvc;
using L2PAPIClient.DataModel;
using L2PAPIClient;
using System;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Grp.L2PSite.MobileApp.Services;
using static Grp.L2PSite.MobileApp.Services.Tools;
using System.Collections.Generic;
using System.Threading;

namespace Grp.L2PSite.MobileApp.Controllers
{
    public class HomeController : Controller
    {
        public async Task<IActionResult> MyCourses(String semId)
        {
            try
            {
                LoginStatus lStatus = LoginStatus.Waiting;
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Context.Session.Get("LoggedIn") != null)
                    lStatus = (LoginStatus)Tools.ByteArrayToObject(Context.Session.Get("LoggedIn"));

                if (lStatus == LoginStatus.LoggedIn)
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
            catch(Exception ex)
            {
                //Let Cookie Expire
                CookieOptions cOptions = new CookieOptions();
                cOptions.Expires = DateTime.Now.AddDays(-1);
                Response.Cookies.Append("CRTID", Encryptor.Encrypt(L2PAPIClient.Config.getRefreshToken()), cOptions);
                Response.Cookies.Append("CRAID", Encryptor.Encrypt(L2PAPIClient.Config.getAccessToken()), cOptions);

                return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = ex.Message });
            }
            return RedirectToAction(nameof(AccountController.Login), "Account");
        }

        public async Task<IActionResult> Calendar()
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context))
                {
                    //remove previously save course id
                    Context.Session.Remove("CourseId");

                    List<L2PCourseEvent> courseEvents = new List<L2PCourseEvent>();
                    L2PCourseInfoSetData courses = L2PAPIClient.api.Calls.L2PviewAllCourseInfoAsync().Result;

                    if(courses.dataset != null)
                    {
                        foreach(L2PCourseInfoData course in courses.dataset)
                        {
                            L2PCourseEventList result = await L2PAPIClient.api.Calls.L2PviewCourseEvents(course.uniqueid);
                            if (result.dataSet != null)
                            {
                                courseEvents.AddRange(result.dataSet);
                            }
                        }
                    }
                    ViewData["courseEventsList"] = courseEvents;
                    return View();
                }
                else
                {
                    return RedirectToAction(nameof(AccountController.Login), "Account");
                }
            }
            catch (Exception ex)
            {
                return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = ex.Message });
            }
        }

        public IActionResult About()
        {
            return View();
        }
        
        public IActionResult Error(string error)
        {
            if(error != null && error.Contains("401 (Unauthorized)"))
            {
                error = "The page you are trying to visit does not exist. Please choose from the options below.";
            }
            ViewData["error"] = error;
            return View("~/Views/Shared/Error.cshtml");
        }
    }
}
