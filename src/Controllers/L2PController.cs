using Microsoft.AspNet.Mvc;
using System;
using L2PAPIClient.DataModel;
using Grp.L2PSite.MobileApp.Services;
using System.Threading.Tasks;
using Grp.L2PSite.MobileApp.Models;

namespace Grp.L2PSite.MobileApp.Controllers
{
    public class L2PController : Controller
    {
        // Get Method to add a new Hyperlink in a course
        // GET: /L2P/AddHyperlink
        [HttpGet]
        public async Task<IActionResult> AddHyperlink(string cId)
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
                        return View("~/Views/L2P/AddEditHyperlink.cshtml");
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

        // Post Method to add a new Hyperlink in a course
        // POST: /L2P/AddHyperlink?
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHyperlink(HyperLinkViewModel model, string cId)
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
                        string errorMessage = "You do not have the sufficient rights to add a hyperlink";
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
        // GET: /L2P/ShowHyperlink?
        [HttpGet]
        public async Task<IActionResult> ShowHyperlink(string cId, int hId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);

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
                        ViewData["HyperlinkModel"] = model;
                        return View();
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to view the hyperlink";
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
        // GET: /L2P/DeleteHyperlinks
        [HttpGet]
        public async Task<IActionResult> DeleteHyperlinks(string cId, string hIds)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context))
                {
                    if (String.IsNullOrEmpty(cId) || String.IsNullOrEmpty(hIds))
                    {
                        string errorMessage = "You were redirected to this page with missing parameters.<br/> Please go back to the home page and try again.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }

                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        hIds = hIds.TrimEnd('-');
                        string[] hyperlinkIds = hIds.Split('-');
                        foreach(string hId in hyperlinkIds)
                        {
                            int id = -1;
                            int.TryParse(hId, out id);
                            await L2PAPIClient.api.Calls.L2PDeleteHyperlink(cId, id);
                        }
                        return RedirectToAction(nameof(MyCoursesController.Hyperlinks), "MyCourses", new { @cId = cId , @deleted = 1 });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to delete hyperlinks";
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
