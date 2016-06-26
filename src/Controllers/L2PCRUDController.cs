using Microsoft.AspNet.Mvc;
using System;
using L2PAPIClient.DataModel;
using Grp.L2PSite.MobileApp.Services;
using System.Threading.Tasks;
using Grp.L2PSite.MobileApp.Models;

namespace Grp.L2PSite.MobileApp.Controllers
{
    public class L2PCRUDController : Controller
    {
        // Get Method to add a new Hyperlink in a course
        // GET: /L2PAdd/AddHyperlink
        [HttpGet]
        public async Task<IActionResult> AddHyperlink(String cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;
                        return View();
                    }
                    else
                    {
                        String errorMessage = "You do not have the sufficient rights to add a hyperlink";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // Get Method to add a new Hyperlink in a course
        // POST: /L2PAdd/AddHyperlink?
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHyperlink(HyperLinkViewModel model, String cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;

                        if (!ModelState.IsValid) // Check if the model was filled correctly (Always add)
                        {
                            return View(model);
                        }
                        if (model.URL != null) // Custom Validation / Validate URL
                        {
                            if (model.URL.ToLower().StartsWith("www."))
                                model.URL = "http://" + model.URL;
                            if (!Tools.checkURLValidity(model.URL))
                            {
                                ModelState.AddModelError(string.Empty, "The provided URL is not valid.");
                                return View(model);
                            }
                        }

                        L2PAddHyperlinkRequest newHyperlink = new L2PAddHyperlinkRequest();
                        newHyperlink.url = model.URL;
                        newHyperlink.description = model.Title;
                        newHyperlink.notes = model.Notes;

                        await L2PAPIClient.api.Calls.L2PAddHyperlink(cId, newHyperlink);
                        return RedirectToAction(nameof(MyCoursesController.Hyperlinks),"MyCourses",new { cId = cId});
                    }
                    else
                    {
                        String errorMessage = "You do not have the sufficient rights to add a hyperlink";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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

        // View Hyperlink with Privilege Validation
        // GET: /L2PAdd/ViewHyperlink?
        [HttpGet]
        public async Task<IActionResult> ViewHyperlink(String cId, int hId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    ViewData["viewMode"] = true;

                    L2PHyperlinkList hlList = await L2PAPIClient.api.Calls.L2PviewHyperlink(cId, hId);
                    if(hlList != null)
                    {
                        HyperLinkViewModel model = new HyperLinkViewModel();
                        foreach (L2PHyperlinkElement hyperlink in hlList.dataSet)
                        {
                            model.URL = hyperlink.url;
                            model.Title = hyperlink.description;
                            model.Notes = hyperlink.notes;
                        }
                        return View("~/Views/L2PAdd/AddHyperlink.cshtml", model);
                        //return RedirectToAction(nameof(L2PCRUDController.AddHyperlink),"L2PCRUD", new { model = model, cId= cId});
                    }
                    else
                    {
                        String errorMessage = "You do not have the sufficient rights to view the hyperlink";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
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
    }
}
