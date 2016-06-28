using Microsoft.AspNet.Mvc;
using Microsoft.AspNet.Http;
using System;
using L2PAPIClient;
using ICSharpCode.SharpZipLib;
using System.Net;
using L2PAPIClient.DataModel;
using System.Collections.Generic;
using Grp.L2PSite.MobileApp.Services;
using System.Threading.Tasks;
using System.Linq;
using System.IO;
using System.IO.Compression;
using ICSharpCode.SharpZipLib.Zip;
using Microsoft.Framework.Runtime;
using System.Threading;

namespace Grp.L2PSite.MobileApp.Controllers
{
    public class MyCoursesController : Controller {

        private readonly IApplicationEnvironment _appEnvironment;

        public MyCoursesController(IApplicationEnvironment appEnvironment)
        {
            _appEnvironment = appEnvironment;
        }
    
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
                        String sourceDirectory = "/" + course.semester + "/" + course.uniqueid + "/Lists/StructuredMaterials";
                        sourceDirectory += ExtdDir;
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

        public IActionResult SharedDocuments()
        {
            return View();
        }

        [HttpGet] // Get Method to show all the hyperlinks of a course
        public async Task<IActionResult> Literature(String cId)
        {

            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    L2PLiteratureSetDataType LList = await L2PAPIClient.api.Calls.L2PviewAllLiteratureAsync(cId);
                   // L2PLiteratureViewDataType LVList = new L2PLiteratureViewDataType();
                 
                    List<L2PLiteratureElementDataType> literatures = new List<L2PLiteratureElementDataType>();
                    if (LList != null)
                    {
                        //L2PLiteratureElementDataType
                        literatures = LList.dataSet;
                    }
                    ViewData["CourseLiterature"] = literatures;
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
        public async Task<IActionResult> Hyperlinks(String cId)
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



        [HttpGet] // Get Method to show all the hyperlinks of a course
        public async Task<IActionResult> Assignments(String cId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
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


        [HttpGet] // Get Method to show specific assignments
        public async Task<IActionResult> ViewAssignment(String cId, string aid)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    L2PAssignmentList assnList = await L2PAPIClient.api.Calls.L2PviewAssignment(cId, int.Parse(aid));
                    ViewData["ChosenAssignment"] = assnList;
                    List<L2PAssignmentElement> assignments = new List<L2PAssignmentElement>();
                    if (assnList.dataSet != null)
                    {
                        
                        assignments = assnList.dataSet;
               
                    }
                    ViewData["ViewAssignment"] = assignments;
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


        public IActionResult MediaLibrary()
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


        public ActionResult DownloadsUsingCID(string url, string filename, string cId)
        {
            try
            {
                string callURL = Config.L2PEndPoint + "/downloadFile/" + filename + "?accessToken=" + Config.getAccessToken() + "&cid=" + cId + "&downloadUrl=" + url;
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


        public async Task<ActionResult> DownloadsZip(string caid, string aid)
        {
            try
            {
                L2PAssignmentList assnList = await L2PAPIClient.api.Calls.L2PviewAssignment(caid, int.Parse(aid));
                List<L2PAssignmentElement> assignments = new List<L2PAssignmentElement>();
                if (assnList.dataSet != null)
                {
                    assignments = assnList.dataSet;
                }

                string tempFilePath = Path.GetTempPath() + "Grp.L2PSite.MobileApp" + DateTime.Now.ToString("dd.MM.yyyy.hh_mm_ss");
                string tempDownloadFilesPath = tempFilePath  + "/downloadFiles/";
                string tempZipPath = tempFilePath + "/downloadZip/";

                if (!Directory.Exists(tempDownloadFilesPath))
                {
                    Directory.CreateDirectory(tempDownloadFilesPath);
                }
                if (!Directory.Exists(tempZipPath))
                {
                    Directory.CreateDirectory(tempZipPath);
                }

                foreach (L2PAssignmentElement a in assignments)
                {
                    List<L2PAttachmentElement> filePaths = a.assignmentDocuments;
                    foreach (L2PAttachmentElement filePath in filePaths)
                    {
                        string callURL = Config.L2PEndPoint + "/downloadFile/" + filePath.fileName + "?accessToken=" + Config.getAccessToken() + "&cid=" + caid + "&downloadUrl=" + filePath.downloadUrl;
                        HttpWebRequest myHttpWebRequest = (HttpWebRequest)WebRequest.Create(callURL);
                        myHttpWebRequest.MaximumAutomaticRedirections = 1;
                        myHttpWebRequest.AllowAutoRedirect = true;
                        HttpWebResponse myHttpWebResponse = (HttpWebResponse)myHttpWebRequest.GetResponse();
                        FileStreamResult file = File(myHttpWebResponse.GetResponseStream(), myHttpWebResponse.ContentType, filePath.fileName);

                        Stream s = myHttpWebResponse.GetResponseStream();
                        FileStream s1 = new FileStream(tempDownloadFilesPath + filePath.fileName, FileMode.Create);
                        Tools.CopyStream(s, s1);
                        s1.Close();
                    }

                    FileStream fsOut = System.IO.File.Create(tempZipPath + a.title + ".zip");
                    ZipOutputStream zipStream = new ZipOutputStream(fsOut);

                    zipStream.SetLevel(3); //0-9, 9 being the highest level of compression

                    // This setting will strip the leading part of the folder path in the entries, to
                    // make the entries relative to the starting folder.
                    // To include the full path for each entry up to the drive root, assign folderOffset = 0.
                    int folderOffset = tempDownloadFilesPath.Length + 0;

                    Tools.CompressFolder(tempDownloadFilesPath, zipStream, folderOffset);

                    zipStream.IsStreamOwner = true; // Makes the Close also Close the underlying stream
                    zipStream.Close();

                    //Wait 10 seconds to delete the files in temp folder
                    Task t = Task.Run(async delegate {
                        await Task.Delay(TimeSpan.FromSeconds(10));
                        Directory.Delete(tempDownloadFilesPath, true);
                        Directory.Delete(tempZipPath, true);
                        Directory.Delete(tempFilePath, true);
                    });
                    return File(tempZipPath + a.title + ".zip", "application/zip", a.title + ".zip");

                }
                return RedirectToAction(nameof(MyCoursesController.Assignments), "MyCourses");
            }
            catch (Exception ex)
            {
                ViewData["error"] = ex.Message;
                return RedirectToAction(nameof(HomeController.Error), "Error");
            }
        }
    }
}
