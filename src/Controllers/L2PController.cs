using Microsoft.AspNet.Mvc;
using System;
using L2PAPIClient.DataModel;
using Grp.L2PSite.MobileApp.Services;
using System.Threading.Tasks;
using Grp.L2PSite.MobileApp.Models;
using Microsoft.AspNet.Http;
using Microsoft.Net.Http.Headers;

namespace Grp.L2PSite.MobileApp.Controllers
{
    public class L2PController : Controller
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
                        return RedirectToAction(nameof(MyCoursesController.Hyperlinks), "MyCourses", new { cId = cId });

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
                    if (hlList != null)
                    {
                        HyperLinkViewModel model = new HyperLinkViewModel();
                        foreach (L2PHyperlinkElement hyperlink in hlList.dataSet)
                        {
                            model.URL = hyperlink.url;
                            model.Title = hyperlink.description;
                            model.Notes = hyperlink.notes;
                        }
                        return View("~/Views/L2PAdd/AddHyperlink.cshtml", model);
                        //return RedirectToAction(nameof(L2PController.AddHyperlink),"L2P", new { model = model, cId= cId});
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

      


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddSolution(IFormFile file, SolutionViewModel model, String cId, string aId)
        //{
        //    try
        //    {
        //        // This method must be used before every L2P API call
        //        Tools.getAndSetUserToken(Request.Cookies, Context);
        //        if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
        //        {
        //            L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
        //            L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
        //            if (userRole != null && (userRole.role.Contains("student")))
        //            {
        //                ViewData["ChosenCourse"] = course;
        //                ViewData["userRole"] = userRole;

        //                if (!ModelState.IsValid) // Check if the model was filled correctly (Always add)
        //                {
        //                    return View(model);
        //                }


        //                if (model != null)
        //                {

                           
        //                    L2PAssignmentSolution sol = new L2PAssignmentSolution();
        //                    L2PUploadRequest data = new L2PUploadRequest();
        //                    data.fileName = ContentDispositionHeaderValue.Parse(file.ContentDisposition)
        //                        .FileName
        //                        .Trim('"');// FileName returns "fileName.ext"(with double quotes) in beta 3

        //                    using (System.IO.Stream stream = file.OpenReadStream())
        //                    {
        //                        byte[] buffer = new byte[stream.Length];
        //                        await stream.ReadAsync(buffer, 0, (int)stream.Length);
        //                        data.stream = Convert.ToBase64String(buffer);
        //                    }
        //                    String sourceDirectory = "/" + course.semester + "/" + course.uniqueid + "/assessment/Lists/LA_SolutionDocuments/A" + aId + "/S" + model.solName;

                   
        //                    await L2PAPIClient.api.Calls.L2PuploadInAssignments(cId, sourceDirectory, data);


        //                    sol.solutionDirectory = "/" + course.semester + "/" + course.uniqueid + "/assessment/Lists/LA_SolutionDocuments/A" + model.assignID + "/S" + model.solName;
        //                    sol.itemId = model.assignID;

        //                    L2PAvailableGroups group=  await L2PAPIClient.api.Calls.L2PviewAvailableGroupsInGroupWorkspace(cId);
        //                    L2PgwsElement GE = new L2PgwsElement();
        //                    //if(group !=null)

        //                    string groupAlias = ""; 
        //                    foreach (L2PgwsElement g in group.dataSet)
        //                    {
        //                        groupAlias = g.systemGeneratedAlias;
        //                    }

        //                    await L2PAPIClient.api.Calls.L2PprovideAssignmentSolution(cId, model.assignID, groupAlias, sol);

        //                }

        //                //await L2PAPIClient.api.Calls.L2PprovideAssignmentSolution()

        //                return RedirectToAction(nameof(L2PController.AddSolution), "L2P", new { model = model, cId = cId });
        //            }
        //            else
        //            {
        //                String errorMessage = "You do not have the sufficient rights to add a solution";
        //                return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
        //            }
        //        }
        //        else
        //        {
        //            return RedirectToAction(nameof(AccountController.Login), "Account");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = ex.Message });
        //    }
        //}


        //[HttpGet]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> AddSolution(String cId, int sid)
        //{
        //    try
        //    {
        //        // This method must be used before every L2P API call
        //        Tools.getAndSetUserToken(Request.Cookies, Context);
        //        if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
        //        {
        //            L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
        //            L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
        //            if (userRole != null && (userRole.role.Contains("student")))
        //            {
        //                ViewData["ChosenCourse"] = course;
        //                ViewData["userRole"] = userRole;

        //                SolutionViewModel model = new SolutionViewModel();
        //               L2PAssignmentList L = await L2PAPIClient.api.Calls.L2PviewAssignment(cId, sid);
        //                //await L2PAPIClient.api.Calls.L2PprovideAssignmentSolution()
        //                foreach(L2PAssignmentElement a in L.dataSet)
        //                {
        //                    L2PAssignmentSolution asS = a.solution;
        //                    model.StudentComment = asS.studentComment;
        //                    model.assignID= asS.itemId;
        //                    model.Status = asS.Status;
        //                    model.assignmentName = a.title;
                            
                           

        //                }

        //                ViewData["ChosenCourse"] = course;
        //                ViewData["userRole"] = userRole;
        //                return View("~/Views/L2P/AddSolution.cshtml", model);
        //            }
        //            else
        //            {
        //                String errorMessage = "You do not have the sufficient rights to add a solution";
        //                return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
        //            }
        //        }
        //        else
        //        {
        //            return RedirectToAction(nameof(AccountController.Login), "Account");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = ex.Message });
        //    }
        //}



        // View Hyperlink with Privilege Validation
        // GET: /L2P/ShowHyperlink?
        [HttpGet]
        public async Task<IActionResult> ViewLiterature(string cId, int lId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);

                    L2PLiteratureSetDataType LList = await L2PAPIClient.api.Calls.L2PviewLiteratureAsync(cId, lId);
                    if (LList != null)
                    {
                        LiteratureViewModel model = new LiteratureViewModel();
                        foreach (L2PLiteratureElementDataType L in LList.dataSet)
                        {

                            model.title = L.title;
                            model.address = L.address;
                            model.authors = L.authors;
                            model.availability = L.availability;
                            model.booktitle = L.booktitle;
                            model.comments = L.comments;
                            model.doi = L.doi;
                            model.edition = L.edition;
                            model.editor = L.editor;
                            model.fromPage = L.fromPage;
                            model.isxn = L.isxn;
                            model.itemID = L.itemID;
                            model.journalName = L.journalName;
                            model.number = L.number;
                            model.publisher = L.publisher;
                            model.relevance = L.relevance;
                            model.role = L.role;
                            model.series = L.series;
                            model.type = L.type;
                            model.url = L.url;
                            model.urlComment = L.urlComment;
                            model.volume = L.volume;
                            model.year = L.year;

                        }
                        ViewData["LiteratureModel"] = model;
                        return View();
                    }
                    else
                    {
                        string errorMessage = "The Literature you are trying to view does not exist.";
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
        public async Task<IActionResult> DeleteLiterature(string cId, string lIds)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context))
                {
                    if (String.IsNullOrEmpty(cId))
                    {
                        string errorMessage = "You were redirected to this page with missing parameters.<br/> Please go back to the home page and try again.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
                    else if (String.IsNullOrEmpty(lIds))
                    {
                        return RedirectToAction(nameof(MyCoursesController.Literature), "MyCourses", new { @cId = cId });
                    }

                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        lIds = lIds.TrimEnd('-');
                        string[] LiteratureIds = lIds.Split('-');
                        foreach (string lId in LiteratureIds)
                        {
                            int id = -1;
                            int.TryParse(lId, out id);
                            await L2PAPIClient.api.Calls.L2PDeleteLiterature(cId, id);
                        }
                        return RedirectToAction(nameof(MyCoursesController.Literature), "MyCourses", new { @cId = cId, @msg = "Literature(s) successfully deleted!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to delete Literature(s)";
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

        // Get Method to Edit a Hyperlink in a course
        // GET: /L2P/EditHyperlink
        [HttpGet]
        public async Task<IActionResult> EditLiterature(string cId, int lId)
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
                        L2PLiteratureSetDataType lList = await L2PAPIClient.api.Calls.L2PviewLiteratureAsync(cId, lId);
                        if (lList != null)
                        {
                           
                            LiteratureViewModel model = new LiteratureViewModel();
                            foreach (L2PLiteratureElementDataType L in lList.dataSet)
                            {
                                model.title = L.title;
                                model.address = L.address;
                                model.authors = L.authors;
                                model.availability = L.availability;
                                model.booktitle = L.booktitle;
                                model.comments = L.comments;
                                model.doi = L.doi;
                                model.edition = L.edition;
                                model.editor = L.editor;
                                model.fromPage = L.fromPage;
                                model.isxn = L.isxn;
                                model.itemID = L.itemID;
                                model.journalName = L.journalName;
                                model.number = L.number;
                                model.publisher = L.publisher;
                                model.relevance = L.relevance;
                                model.role = L.role;
                                model.series = L.series;
                                model.type = L.type;
                                model.url = L.url;
                                model.urlComment = L.urlComment;
                                model.volume = L.volume;
                                model.year = L.year;
                            }
                            ViewData["EditMode"] = true;
                            ViewData["ChosenCourse"] = course;
                            ViewData["userRole"] = userRole;
                            return View("~/Views/L2P/AddEditLiterature.cshtml", model);
                        }
                        else
                        {
                            string errorMessage = "The Literature you are trying to view does not exist.";
                            return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                        }
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to edit this Literature";
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

        // POST Method to Edit a Hyperlink in a course
        // POST: /L2P/EditHyperlink
        [HttpPost]
        public async Task<IActionResult> EditLiterature(LiteratureViewModel model, string cId)
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
                            return View("~/Views/L2P/AddEditLiterature.cshtml", model);
                        }
                        //if (model.URL != null) // Custom Validation / Validate URL
                        //{
                        //    if (model.URL.ToLower().StartsWith("www."))
                        //        model.URL = "http://" + model.URL;
                        //    if (!Tools.checkURLValidity(model.URL))
                        //    {
                        //        ModelState.AddModelError(string.Empty, "The provided URL is not valid.");
                        //        View("~/Views/L2P/AddEditHyperlink.cshtml", model);
                        //    }
                        //}

                        L2PLiteratureAddRequest editLiterature = new L2PLiteratureAddRequest();

                        editLiterature.title = model.title;
                        editLiterature.address = model.address;
                        editLiterature.authors = model.authors;
                     
                        editLiterature.booktitle = model.booktitle;
                        editLiterature.comments = model.comments;
                        editLiterature.doi = model.doi;
                        editLiterature.edition = model.edition;
                        
                        editLiterature.fromPage = model.fromPage;
                        editLiterature.isxn = model.isxn;
                       
                        editLiterature.journalName = model.journalName;
                        editLiterature.number = model.number;
                        editLiterature.publisher = model.publisher;
                        editLiterature.relevance = model.relevance;
                        editLiterature.role = model.role;
                        editLiterature.series = model.series;
                        editLiterature.type = model.type;
                        if(model.url !=null)
                        { 
                        if (model.url.ToLower().StartsWith("www."))
                            model.url = "http://" + model.url;
                        if (!Tools.checkURLValidity(model.url))
                        {
                            ModelState.AddModelError(string.Empty, "The provided URL is not valid.");
                            View("~/Views/L2P/AddEditLiterature.cshtml", model);
                        }
                        }
                        editLiterature.url = model.url;
                        editLiterature.urlComment = model.urlComment;
                        editLiterature.volume = model.volume;
                        editLiterature.year = model.year;


                        

                        await L2PAPIClient.api.Calls.L2PupdateLiterature(cId, model.itemID, editLiterature);

                        return RedirectToAction(nameof(MyCoursesController.Literature), "MyCourses", new { cId = cId, @msg = "Literature was successfully edited!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to edit a Literature";
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



        // Get Method to Edit a Hyperlink in a course
        // GET: /L2P/EditHyperlink
        [HttpGet]
        public async Task<IActionResult> AddLiterature(string cId, int lId)
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
                        L2PLiteratureSetDataType lList = await L2PAPIClient.api.Calls.L2PviewLiteratureAsync(cId, lId);
                        if (lList != null)
                        {

                            LiteratureViewModel model = new LiteratureViewModel();
                            foreach (L2PLiteratureElementDataType L in lList.dataSet)
                            {
                                model.title = L.title;
                                model.address = L.address;
                                model.authors = L.authors;
                                model.availability = L.availability;
                                model.booktitle = L.booktitle;
                                model.comments = L.comments;
                                model.doi = L.doi;
                                model.edition = L.edition;
                                model.editor = L.editor;
                                model.fromPage = L.fromPage;
                                model.isxn = L.isxn;
                                model.itemID = L.itemID;
                                model.journalName = L.journalName;
                                model.number = L.number;
                                model.publisher = L.publisher;
                                model.relevance = L.relevance;
                                model.role = L.role;
                                model.series = L.series;
                                model.type = L.type;
                                model.url = L.url;
                                model.urlComment = L.urlComment;
                                model.volume = L.volume;
                                model.year = L.year;
                            }
                            ViewData["ChosenCourse"] = course;
                            ViewData["userRole"] = userRole;
                            return View("~/Views/L2P/AddEditLiterature.cshtml", model);
                        }
                        else
                        {
                            string errorMessage = "The Literature you are trying to view does not exist.";
                            return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                        }
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to edit this Literature";
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

        // POST Method to Edit a Hyperlink in a course
        // POST: /L2P/EditHyperlink
        [HttpPost]
        public async Task<IActionResult> AddLiterature(LiteratureViewModel model, string cId)
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
                            return View("~/Views/L2P/AddEditLiterature.cshtml", model);
                        }
                        //if (model.URL != null) // Custom Validation / Validate URL
                        //{
                        //    if (model.URL.ToLower().StartsWith("www."))
                        //        model.URL = "http://" + model.URL;
                        //    if (!Tools.checkURLValidity(model.URL))
                        //    {
                        //        ModelState.AddModelError(string.Empty, "The provided URL is not valid.");
                        //        View("~/Views/L2P/AddEditHyperlink.cshtml", model);
                        //    }
                        //}

                        L2PLiteratureAddRequest AddLiterature = new L2PLiteratureAddRequest();

                        AddLiterature.title = model.title;
                        AddLiterature.address = model.address;
                        AddLiterature.authors = model.authors;
            
                        AddLiterature.booktitle = model.booktitle;
                        AddLiterature.comments = model.comments;
                        AddLiterature.doi = model.doi;
                        AddLiterature.edition = model.edition;

                        AddLiterature.fromPage = model.fromPage;
                        AddLiterature.isxn = model.isxn;

                        AddLiterature.journalName = model.journalName;
                        AddLiterature.number = model.number;
                        AddLiterature.publisher = model.publisher;
                        AddLiterature.relevance = model.relevance;
                        AddLiterature.role = model.role;
                        AddLiterature.series = model.series;
                        AddLiterature.type = model.type;
                        if (model.url != null) // Custom Validation / Validate URL
                        {
                            if (model.url.ToLower().StartsWith("www."))
                                model.url = "http://" + model.url;
                            if (!Tools.checkURLValidity(model.url))
                            {
                                ModelState.AddModelError(string.Empty, "The provided URL is not valid.");
                                View("~/Views/L2P/AddEditLiterature.cshtml", model);
                            }
                        }
                        AddLiterature.url = model.url;
                        AddLiterature.urlComment = model.urlComment;
                        AddLiterature.volume = model.volume;
                        AddLiterature.year = model.year;




                        await L2PAPIClient.api.Calls.L2PAddLiterature(cId, AddLiterature);

                        return RedirectToAction(nameof(MyCoursesController.Literature), "MyCourses", new { cId = cId, @msg = "Literature was successfully Added!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to Add a hyperlink";
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
        public async Task<IActionResult> DeleteAssignment(string cId, string aId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context))
                {
                    if (String.IsNullOrEmpty(cId))
                    {
                        string errorMessage = "You were redirected to this page with missing parameters.<br/> Please go back to the home page and try again.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
                    else if (String.IsNullOrEmpty(aId))
                    {
                        return RedirectToAction(nameof(MyCoursesController.Literature), "MyCourses", new { @cId = cId });
                    }

                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {

                        await L2PAPIClient.api.Calls.L2PDeleteAssignment(cId, Int32.Parse(aId));
                        return RedirectToAction(nameof(MyCoursesController.Assignments), "MyCourses", new { @cId = cId, @msg = "Assignment successfully deleted!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to delete Assignment";
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


