using L2PAPIClient;
using L2PAPIClient.DataModel;
using System;
using System.Net;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;
using Grp.L2PSite.MobileApp.Services;

namespace Grp.L2PSite.MobileApp.Controllers
{
    public class MyCoursesController : Controller
    {
        [HttpGet] // Get Method to retrieve the course What's New Page
        public async Task<IActionResult> WhatsNew(String cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context))
                {
                    Context.Session.SetString("CourseId", cId);
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["CourseWhatsNew"] = await L2PAPIClient.api.Calls.L2PwhatsNewSinceAsync(cId, 180000);

                    L2PAssignmentList assnList = await L2PAPIClient.api.Calls.L2PviewAllAssignments(cId);
                    List<L2PAssignmentElement> assignments = new List<L2PAssignmentElement>();
                    if (assnList.dataSet != null)
                    {
                        assignments = assnList.dataSet;
                        // Sort by publish date desc
                        assignments.Sort((x, y) => y.assignmentPublishDate.CompareTo(x.assignmentPublishDate));
                    }
                    ViewData["Assignments"] = assignments;
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

        [HttpGet] // Get Method to show the subject info of a course.
        public IActionResult ShowSubject(String cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["CourseInfo"] = L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId).Result;
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

        [HttpGet] // Get Method to show the learning material of a course.
        public async Task<IActionResult> LearningMaterials(String cId, String ExtdDir)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["ChosenCourse"] = course;
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    L2PLearningMaterialList lmList = await L2PAPIClient.api.Calls.L2PviewAllLearningMaterials(cId);
                    List<L2PLearningMaterialElement> learningMaterials = new List<L2PLearningMaterialElement>();
                    if (lmList.dataSet != null)
                    {
                        string sourceDirectory = "/" + course.semester + "/" + course.uniqueid + "/Lists/StructuredMaterials";
                        if(ExtdDir != null)
                        {
                            var element = from elts in lmList.dataSet
                                          where elts.isDirectory == true && elts.name.Equals(ExtdDir)
                                          select elts;
                            if (element.Any())
                            {
                                sourceDirectory = element.First().selfUrl;
                            }
                        }
                        var materials = from elts in lmList.dataSet
                                        where elts.sourceDirectory.Equals(sourceDirectory)
                                        orderby elts.isDirectory descending
                                        select elts;
                        learningMaterials = materials.ToList();
                    }
                    ViewData["CourseLearningMaterials"] = learningMaterials;
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

        [HttpGet] // Get Method to show the shared documents material of a course.
        public async Task<IActionResult> SharedDocuments(String cId, String ExtdDir)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["ChosenCourse"] = course;
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    L2PLearningMaterialList sdList = await L2PAPIClient.api.Calls.L2PviewAllSharedDocuments(cId);
                    List<L2PLearningMaterialElement> sharedDocuments = new List<L2PLearningMaterialElement>();
                    if (sdList.dataSet != null)
                    {
                        string sourceDirectory = "/" + course.semester + "/" + course.uniqueid + "/collaboration/Lists/SharedDocuments";
                        sourceDirectory += ExtdDir;
                        if (ExtdDir != null)
                        {
                            var element = from elts in sdList.dataSet
                                          where elts.isDirectory == true && elts.name.Equals(ExtdDir)
                                          select elts;
                            if (element.Any())
                            {
                                sourceDirectory = element.First().selfUrl;
                            }
                        }
                        var materials = from elts in sdList.dataSet
                                        where elts.sourceDirectory.Equals(sourceDirectory)
                                        orderby elts.isDirectory descending
                                        select elts;
                        sharedDocuments = materials.ToList();
                    }
                    ViewData["CourseSharedDocuments"] = sharedDocuments;
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

        [HttpGet] // Get Method to show all the hyperlinks of a course
        public async Task<IActionResult> Hyperlinks(string cId, string msg)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    L2PHyperlinkList hpList = await L2PAPIClient.api.Calls.L2PviewAllHyperlinks(cId);
                    List<L2PHyperlinkElement> hyperlinks = new List<L2PHyperlinkElement>();
                    if (hpList.dataSet != null)
                    {
                        hyperlinks = hpList.dataSet;
                    }

                    ViewData["Message"] = msg;
                    ViewData["CourseHyperlinks"] = hyperlinks;
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

        public IActionResult Literature()
        {
            return View();
        }

        [HttpGet] // Get Method to show the Media Library of a course.
        public async Task<IActionResult> MediaLibrary(String cId, String ExtdDir)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
                {
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["ChosenCourse"] = course;
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    L2PMediaLibraryList mdList = await L2PAPIClient.api.Calls.L2PviewAllMediaLibraries(cId);
                    List<L2PMediaLibraryElement> mediaLibrary = new List<L2PMediaLibraryElement>();
                    if (mdList.dataSet != null)
                    {
                        string sourceDirectory = "/" + course.semester + "/" + course.uniqueid + "/Lists/MediaLibrary";
                        if (ExtdDir != null)
                        {
                            var element = from elts in mdList.dataSet
                                          where elts.isDirectory == true && elts.name.Equals(ExtdDir)
                                          select elts;
                            if (element.Any())
                            {
                                sourceDirectory = element.First().selfUrl;
                            }
                        }
                        var materials = from elts in mdList.dataSet
                                        where elts.sourceDirectory.Equals(sourceDirectory)
                                        orderby elts.isDirectory descending
                                        select elts;
                        mediaLibrary = materials.ToList();
                    }
                    ViewData["CourseMediaLibrary"] = mediaLibrary;
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

        // Function used to download files from the L2P Client API
        public ActionResult Downloads(string url, string filename)
        {
            try
            {

                string callURL = Config.L2PEndPoint + "/downloadFile/" + filename + "?accessToken=" + Config.getAccessToken() + "&cid=" + Context.Session.GetString("CourseId") + "&downloadUrl=|" + url;
                HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(callURL);
                myHttpWebRequest.MaximumAutomaticRedirections = 1;
                myHttpWebRequest.AllowAutoRedirect = true;
                HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                return File(myHttpWebResponse.GetResponseStream(), myHttpWebResponse.ContentType, filename);
            }
            catch (Exception ex)
            {
                ViewData["error"] = ex.Message;
                return RedirectToAction(nameof(HomeController.Error), "Error");
            }
        }

    }
}
