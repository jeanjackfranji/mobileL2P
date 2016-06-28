using Microsoft.AspNet.Mvc;
using System;
using L2PAPIClient.DataModel;
using Grp.L2PSite.MobileApp.Services;
using System.Threading.Tasks;
using Grp.L2PSite.MobileApp.Models;
using Microsoft.AspNet.Http;
using static Grp.L2PSite.MobileApp.Services.Tools;
using Microsoft.Net.Http.Headers;

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
                            return View("~/Views/L2P/AddEditHyperlink.cshtml", model);
                        }
                        if (model.URL != null) // Custom Validation / Validate URL
                        {
                            if (model.URL.ToLower().StartsWith("www."))
                                model.URL = "http://" + model.URL;
                            if (!Tools.checkURLValidity(model.URL))
                            {
                                ModelState.AddModelError(string.Empty, "The provided URL is not valid.");
                                return View("~/Views/L2P/AddEditHyperlink.cshtml", model);
                            }
                        }

                        L2PAddHyperlinkRequest newHyperlink = new L2PAddHyperlinkRequest();
                        newHyperlink.url = model.URL;
                        newHyperlink.description = model.Title;
                        newHyperlink.notes = model.Notes;

                        await L2PAPIClient.api.Calls.L2PAddHyperlink(cId, newHyperlink);
                        return RedirectToAction(nameof(MyCoursesController.Hyperlinks),"MyCourses",new { cId = cId, @msg = "Hyperlink was successfully added!" });
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

        // Post Method to add a new folder in the intended view (Learning Material,...)
        // POST: /L2P/AddFolder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFolder(string cId, int ModelNb, string curDir, FolderViewModel model)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
                {
                    string currentFolder = null;
                    if (curDir != null && curDir.Contains("/"))
                        currentFolder = curDir.Substring(curDir.LastIndexOf('/') + 1);
                    else if (curDir != null && curDir.Length > 0)
                        currentFolder = curDir;

                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && ((userRole.role.Contains("manager") || userRole.role.Contains("tutors")) || ModelNb == 2))
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;

                        if (!ModelState.IsValid) // Check if the model was filled correctly (Always add)
                        {
                            return RedirectToAction(nameof(MyCoursesController.LearningMaterials), "MyCourses", new { cId = cId, ExtdDir = currentFolder, @msg = "Invalid Folder Name!" });
                        }

                        await L2PAPIClient.api.Calls.L2PCreateFolder(cId, ModelNb, model.Name, curDir);

                        //moduleNb (0 = Learning Material, 1 = Media Library, 2 = SharedDocuments
                        ModuleNumber module = (ModuleNumber)Enum.ToObject(typeof(ModuleNumber), ModelNb);
                        string actionName = nameof(MyCoursesController.SharedDocuments);

                        if (module == ModuleNumber.LearningMaterials)
                            actionName = nameof(MyCoursesController.LearningMaterials);
                        else if (module == ModuleNumber.MediaLibrary)
                            actionName = nameof(MyCoursesController.MediaLibrary);

                        return RedirectToAction(actionName, "MyCourses", new { cId = cId, ExtdDir = currentFolder, @msg = "Folder was successfully added!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to add a folder";
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

        // Post Method to add a new file in the intended view (Learning Material,...)
        // POST: /L2P/AddFile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddFile(IFormFile file, string cId, int ModelNb, string curDir)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
                {
                    string currentFolder = null;
                    if (curDir != null && curDir.Contains("/"))
                        currentFolder = curDir.Substring(curDir.LastIndexOf('/') + 1);
                    else if (curDir != null && curDir.Length > 0)
                        currentFolder = curDir;

                    //moduleNb (0 = Learning Material, 1 = Media Library, 2 = SharedDocuments
                    ModuleNumber module = (ModuleNumber)Enum.ToObject(typeof(ModuleNumber), ModelNb);
                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && ((userRole.role.Contains("manager") || userRole.role.Contains("tutors")) || ModelNb == 2))
                    {
                        ViewData["ChosenCourse"] = course;
                        ViewData["userRole"] = userRole;

                        if (file == null) // Check if the model was filled correctly (Always add)
                        {
                            if (module == ModuleNumber.LearningMaterials)
                                return RedirectToAction(nameof(MyCoursesController.LearningMaterials), "MyCourses", new { cId = cId, ExtdDir = currentFolder, @msg = "No file was selected!" });
                            else if (module == ModuleNumber.MediaLibrary)
                                return RedirectToAction(nameof(MyCoursesController.MediaLibrary), "MyCourses", new { cId = cId, ExtdDir = currentFolder, @msg = "No file was selected!" });
                            else if (module == ModuleNumber.SharedDocuments)
                                return RedirectToAction(nameof(MyCoursesController.SharedDocuments), "MyCourses", new { cId = cId, ExtdDir = currentFolder, @msg = "No file was selected!" });
                        }

                        L2PUploadRequest data = new L2PUploadRequest();
                        data.fileName = ContentDispositionHeaderValue
                            .Parse(file.ContentDisposition)
                            .FileName
                            .Trim('"');// FileName returns "fileName.ext"(with double quotes) in beta 3

                        using (System.IO.Stream stream = file.OpenReadStream())
                        {
                            byte[] buffer = new byte[stream.Length];
                            await stream.ReadAsync(buffer, 0, (int)stream.Length);
                            data.stream = Convert.ToBase64String(buffer);
                        }

                        if (module == ModuleNumber.LearningMaterials)
                        {
                            await L2PAPIClient.api.Calls.L2PuploadInLearningMaterials(cId, curDir, data);
                            return RedirectToAction(nameof(MyCoursesController.LearningMaterials), "MyCourses", new { cId = cId, ExtdDir = currentFolder, @msg = "File was successfully added!" });
                        }
                        else if (module == ModuleNumber.MediaLibrary)
                        {
                            await L2PAPIClient.api.Calls.L2PuploadInMediaLibrary(cId, curDir, data);
                            return RedirectToAction(nameof(MyCoursesController.MediaLibrary), "MyCourses", new { cId = cId, ExtdDir = currentFolder, @msg = "File was successfully added!" });
                        }
                        else if (module == ModuleNumber.SharedDocuments)
                        {
                            await L2PAPIClient.api.Calls.L2PuploadInSharedDocuments(cId, curDir, data);
                            return RedirectToAction(nameof(MyCoursesController.SharedDocuments), "MyCourses", new { cId = cId, ExtdDir = currentFolder, @msg = "File was successfully added!" });
                        }
                        return RedirectToAction(nameof(MyCoursesController.WhatsNew), "MyCourses", new { cId = cId});
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to add a folder";
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
                            model.itemId = hyperlink.itemId;
                        }
                        ViewData["HyperlinkModel"] = model;
                        return View();
                    }
                    else
                    {
                        string errorMessage = "The Hyperlink you are trying to view does not exist.";
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
        public async Task<IActionResult> EditHyperlink(string cId, int hId)
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
                        L2PHyperlinkList hlList = await L2PAPIClient.api.Calls.L2PviewHyperlink(cId, hId);
                        if (hlList != null)
                        {
                            HtmlConverter con = new HtmlConverter();
                            HyperLinkViewModel model = new HyperLinkViewModel();
                            foreach (L2PHyperlinkElement hyperlink in hlList.dataSet)
                            {
                                model.URL = hyperlink.url;
                                model.Title = hyperlink.description;
                                model.Notes = con.ConvertHtml(hyperlink.notes);
                                model.itemId = hyperlink.itemId;
                            }
                            ViewData["EditMode"] = true;
                            ViewData["ChosenCourse"] = course;
                            ViewData["userRole"] = userRole;
                            return View("~/Views/L2P/AddEditHyperlink.cshtml", model);
                        }
                        else
                        {
                            string errorMessage = "The Hyperlink you are trying to view does not exist.";
                            return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                        }
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to edit this hyperlink";
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
        public async Task<IActionResult> EditHyperlink(HyperLinkViewModel model, string cId)
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
                            return View("~/Views/L2P/AddEditHyperlink.cshtml", model);
                        }
                        if (model.URL != null) // Custom Validation / Validate URL
                        {
                            if (model.URL.ToLower().StartsWith("www."))
                                model.URL = "http://" + model.URL;
                            if (!Tools.checkURLValidity(model.URL))
                            {
                                ModelState.AddModelError(string.Empty, "The provided URL is not valid.");
                                View("~/Views/L2P/AddEditHyperlink.cshtml", model);
                            }
                        }

                        L2PAddHyperlinkRequest editHyperlink = new L2PAddHyperlinkRequest();
                        editHyperlink.url = model.URL;
                        editHyperlink.description = model.Title;
                        editHyperlink.notes = model.Notes;

                        await L2PAPIClient.api.Calls.L2PupdateHyperlink(cId, model.itemId, editHyperlink);

                        return RedirectToAction(nameof(MyCoursesController.Hyperlinks), "MyCourses", new { cId = cId, @msg = "Hyperlink was successfully edited!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to edit a hyperlink";
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
                    if (String.IsNullOrEmpty(cId))
                    {
                        string errorMessage = "You were redirected to this page with missing parameters.<br/> Please go back to the home page and try again.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
                    else if (String.IsNullOrEmpty(hIds))
                    {
                        return RedirectToAction(nameof(MyCoursesController.Hyperlinks), "MyCourses", new { @cId = cId});
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
                        return RedirectToAction(nameof(MyCoursesController.Hyperlinks), "MyCourses", new { @cId = cId , @msg = "Hyperlinks(s) successfully deleted!" });
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

        // Get Method to delete learning materials in a course
        // GET: /L2P/DeleteLearningMaterials
        [HttpGet]
        public async Task<IActionResult> DeleteLearningMaterials(string cId, string LMIds, string curDir)
        {
            try
            {
                string currentFolder = null;
                if (curDir != null && curDir.Contains("/"))
                    currentFolder = curDir.Substring(curDir.LastIndexOf('/') + 1);
                else if (curDir != null && curDir.Length > 0)
                    currentFolder = curDir;

                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context))
                {
                    if (String.IsNullOrEmpty(cId))
                    {
                        string errorMessage = "You were redirected to this page with missing parameters.<br/> Please go back to the home page and try again.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
                    else if (String.IsNullOrEmpty(LMIds))
                    {
                        return RedirectToAction(nameof(MyCoursesController.LearningMaterials), "MyCourses", new { @cId = cId });
                    }

                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        LMIds = LMIds.TrimEnd('-');
                        string[] lrnMaterialIds = LMIds.Split('-');
                        foreach (string hId in lrnMaterialIds)
                        {
                            int id = -1;
                            int.TryParse(hId, out id);
                            await L2PAPIClient.api.Calls.L2PDeleteLearningMaterial(cId, id);
                        }
                        return RedirectToAction(nameof(MyCoursesController.LearningMaterials), "MyCourses", new { @cId = cId, @ExtdDir = currentFolder, @msg = "Material(s)/Folder(s) successfully deleted!" });
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

        // Get Method to delete MediaLibrary in a course
        // GET: /L2P/DeleteMediaLibrary
        [HttpGet]
        public async Task<IActionResult> DeleteMediaLibrary(string cId, string MLIds, string curDir)
        {
            try
            {
                string currentFolder = null;
                if (curDir != null && curDir.Contains("/"))
                    currentFolder = curDir.Substring(curDir.LastIndexOf('/') + 1);
                else if (curDir != null && curDir.Length > 0)
                    currentFolder = curDir;

                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context))
                {
                    if (String.IsNullOrEmpty(cId))
                    {
                        string errorMessage = "You were redirected to this page with missing parameters.<br/> Please go back to the home page and try again.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
                    else if (String.IsNullOrEmpty(MLIds))
                    {
                        return RedirectToAction(nameof(MyCoursesController.MediaLibrary), "MyCourses", new { @cId = cId });
                    }

                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        MLIds = MLIds.TrimEnd('-');
                        string[] mediaIds = MLIds.Split('-');
                        foreach (string mId in mediaIds)
                        {
                            int id = -1;
                            int.TryParse(mId, out id);
                            await L2PAPIClient.api.Calls.L2PDeleteMediaLibrary(cId, id);
                        }
                        return RedirectToAction(nameof(MyCoursesController.MediaLibrary), "MyCourses", new { @cId = cId, @ExtdDir = currentFolder, @msg = "Media(s)/Folder(s) successfully deleted!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to delete media in media library.";
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

        // Get Method to delete Shared Documents in a course
        // GET: /L2P/DeleteSharedDocuments
        [HttpGet]
        public async Task<IActionResult> DeleteSharedDocuments(string cId, string SDIds, string curDir)
        {
            try
            {
                string currentFolder = null;
                if (curDir != null && curDir.Contains("/"))
                    currentFolder = curDir.Substring(curDir.LastIndexOf('/') + 1);
                else if (curDir != null && curDir.Length > 0)
                    currentFolder = curDir;

                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context))
                {
                    if (String.IsNullOrEmpty(cId))
                    {
                        string errorMessage = "You were redirected to this page with missing parameters.<br/> Please go back to the home page and try again.";
                        return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                    }
                    else if (String.IsNullOrEmpty(SDIds))
                    {
                        return RedirectToAction(nameof(MyCoursesController.MediaLibrary), "MyCourses", new { @cId = cId });
                    }

                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors") || userRole.role.Contains("students") || userRole.role.Contains("extra")))
                    {
                        SDIds = SDIds.TrimEnd('-');
                        string[] mediaIds = SDIds.Split('-');
                        foreach (string mId in mediaIds)
                        {
                            int id = -1;
                            int.TryParse(mId, out id);
                            await L2PAPIClient.api.Calls.L2PDeleteSharedDocument(cId, id);
                        }
                        return RedirectToAction(nameof(MyCoursesController.SharedDocuments), "MyCourses", new { @cId = cId, @ExtdDir = currentFolder, @msg = "Shared Document(s)/Folder(s) successfully deleted!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to delete shared documents";
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
// Get Method to add a new Announcement in a course
        // GET: /L2P/AddAnnouncement
        [HttpGet]
        public async Task<IActionResult> AddAnnouncement(string cId)
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
                        return View("~/Views/L2P/AddEditAnnouncement.cshtml");
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to add an announcement";
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

        // Post Method to add a new Announcement in a course
        // POST: /L2P/AddAnnouncement?
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAnnouncement(AnnouncementViewModel model, string cId, IFormFile file)
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
                            return View("~/Views/L2P/AddEditAnnouncement.cshtml", model);
                        }
                       
                        L2PAddAnnouncementRequest newAnnouncement = new L2PAddAnnouncementRequest();
                        newAnnouncement.title = model.title;
                        newAnnouncement.body = model.body;
                        
                       
                        await L2PAPIClient.api.Calls.L2PAddAnnouncement(cId, newAnnouncement);
                        
                        if (file != null)
                        {

                            L2PUploadRequest data = new L2PUploadRequest();
                            data.fileName = ContentDispositionHeaderValue
                                .Parse(file.ContentDisposition)
                                .FileName
                                .Trim('"');// FileName returns "fileName.ext"(with double quotes) in beta 3

                            using (System.IO.Stream stream = file.OpenReadStream())
                            {
                                byte[] buffer = new byte[stream.Length];
                                await stream.ReadAsync(buffer, 0, (int)stream.Length);
                                data.stream = Convert.ToBase64String(buffer);
                            }
                           L2PAnnouncementList aList = await L2PAPIClient.api.Calls.L2PviewAllAnnouncements(cId);
                            List<L2PAnnouncementElement> announcements = new List<L2PAnnouncementElement>();
                            if (aList.dataSet != null)
                            {
                                announcements = aList.dataSet;

                            }
                            int i = 0;
                            L2PAnnouncementElement lastAnnouncement = new L2PAnnouncementElement();
                            foreach(L2PAnnouncementElement a in announcements){
                                lastAnnouncement = a;
                            }
                           

                            await L2PAPIClient.api.Calls.L2PuploadInAnnouncements(cId, lastAnnouncement.attachmentDirectory, data);

                        }
                        return RedirectToAction(nameof(MyCoursesController.Announcement), "MyCourses", new { cId = cId, @msg = "Announcement was successfully added!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to add an announcement";
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

        // View Announcement with Privilege Validation
        // GET: /L2P/ShowAnnouncement?
        [HttpGet]
        public async Task<IActionResult> ShowAnnouncement(string cId, int hId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);

                    L2PAnnouncementList aList = await L2PAPIClient.api.Calls.L2PviewAnnouncement(cId, hId);
                    if (aList != null)
                    {
                        AnnouncementViewModel model = new AnnouncementViewModel();
                        foreach (L2PAnnouncementElement announcement in aList.dataSet)
                        {
                            model.title = announcement.title;
                            model.body = announcement.body;
                            model.itemId = announcement.itemId;
                            model.folderName = announcement.attachmentDirectory;
                            ViewData["attachments"] = announcement.attachments;
                        }
                        ViewData["AnnouncementModel"] = model;
                        
                        return View();
                    }
                    else
                    {
                        string errorMessage = "The Announcement you are trying to view does not exist.";
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

        // Get Method to Edit a Announcement in a course
        // GET: /L2P/EditAnnouncement
        [HttpGet]
        public async Task<IActionResult> EditAnnouncement(string cId, int hId)
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
                        L2PAnnouncementList aList = await L2PAPIClient.api.Calls.L2PviewAnnouncement(cId, hId);
                        if (aList != null)
                        {

                            HtmlConverter con = new HtmlConverter();
                            AnnouncementViewModel model = new AnnouncementViewModel();
                            foreach (L2PAnnouncementElement announcement in aList.dataSet)
                            {
                                model.title = announcement.title;
                                model.body = con.ConvertHtml(announcement.body);
                                model.itemId = announcement.itemId;
                            }
                            ViewData["EditMode"] = true;
                            ViewData["ChosenCourse"] = course;
                            ViewData["userRole"] = userRole;
                            return View("~/Views/L2P/AddEditAnnouncement.cshtml", model);
                        }
                        else
                        {
                            string errorMessage = "The Announcement you are trying to view does not exist.";
                            return RedirectToAction(nameof(HomeController.Error), "Home", new { @error = errorMessage });
                        }
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to edit this announcement";
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
        // POST: /L2P/EditAnnouncement
        [HttpPost]
        public async Task<IActionResult> EditAnnouncement(AnnouncementViewModel model, string cId, IFormFile file)
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
                            return View("~/Views/L2P/AddEditAnnouncement.cshtml", model);
                        }
                      
                        L2PAddAnnouncementRequest editAnnouncement = new L2PAddAnnouncementRequest();
                        editAnnouncement.title = model.title;
                        editAnnouncement.body = model.body;
                        

                        await L2PAPIClient.api.Calls.L2PupdateAnnouncement(cId, model.itemId, editAnnouncement);
                        if (file != null)
                        {

                            L2PUploadRequest data = new L2PUploadRequest();
                            data.fileName = ContentDispositionHeaderValue
                                .Parse(file.ContentDisposition)
                                .FileName
                                .Trim('"');// FileName returns "fileName.ext"(with double quotes) in beta 3

                            using (System.IO.Stream stream = file.OpenReadStream())
                            {
                                byte[] buffer = new byte[stream.Length];
                                await stream.ReadAsync(buffer, 0, (int)stream.Length);
                                data.stream = Convert.ToBase64String(buffer);
                            }

                            String currDir = "/" + course.semester + "/" + cId + "/" + "Lists/AnnouncementDocuments/A" + model.itemId;
                            await L2PAPIClient.api.Calls.L2PuploadInAnnouncements(cId, currDir , data);

                        }

                        return RedirectToAction(nameof(MyCoursesController.Announcement), "MyCourses", new { cId = cId, @msg = "Announcement was successfully edited!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to edit a announcement";
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

        // Get Method to add a new Announcement in a course
        // GET: /L2P/DeleteAnnouncements
        [HttpGet]
        public async Task<IActionResult> DeleteAnnouncements(string cId, string hIds)
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
                    else if (String.IsNullOrEmpty(hIds))
                    {
                        return RedirectToAction(nameof(MyCoursesController.Announcement), "MyCourses", new { @cId = cId });
                    }

                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        hIds = hIds.TrimEnd('-');
                        string[] announcementIds = hIds.Split('-');
                        foreach (string hId in announcementIds)
                        {
                            int id = -1;
                            int.TryParse(hId, out id);
                            await L2PAPIClient.api.Calls.L2PDeleteAnnouncement(cId, id);
                        }
                        return RedirectToAction(nameof(MyCoursesController.Announcement), "MyCourses", new { @cId = cId, @msg = "Hyperlinks(s) successfully deleted!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to delete announcements";
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
        // Get Method to add a new Email in a course
        // GET: /L2P/AddEmail
        [HttpGet]
        public async Task<IActionResult> AddEmail(string cId)
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
                        return View("~/Views/L2P/AddEmail.cshtml");
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to add an email";
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

        // Post Method to add a new Email in a course
        // POST: /L2P/AddEmail?
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmail(EmailViewModel model, string cId, IFormFile file)
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
                            return View("~/Views/L2P/AddEmail.cshtml", model);
                        }

                        L2PAddEmailRequest newEmail = new L2PAddEmailRequest();
                        newEmail.body = model.body;
                        if (model.cc != null)
                            newEmail.cc = model.cc;
                        newEmail.recipients = model.recipients;

                     
                        if (file != null)
                        {

                            L2PUploadRequest data = new L2PUploadRequest();
                            data.fileName = ContentDispositionHeaderValue
                                .Parse(file.ContentDisposition)
                                .FileName
                                .Trim('"');// FileName returns "fileName.ext"(with double quotes) in beta 3

                            using (System.IO.Stream stream = file.OpenReadStream())
                            {
                                byte[] buffer = new byte[stream.Length];
                                await stream.ReadAsync(buffer, 0, (int)stream.Length);
                                data.stream = Convert.ToBase64String(buffer);
                            }

                            List<L2PUploadRequest> listOfUploads = new List<L2PUploadRequest>();
                            listOfUploads.Add(data);
                            newEmail.attachmentsToUpload = listOfUploads;
                        }
                        await L2PAPIClient.api.Calls.L2PAddEmail(cId, newEmail);

                        return RedirectToAction(nameof(MyCoursesController.Email), "MyCourses", new { cId = cId, @msg = "Email was successfully added!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to add an Email";
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

        // View Email with Privilege Validation
        // GET: /L2P/ShowEmail?
        [HttpGet]
        public async Task<IActionResult> ShowEmail(string cId, int hId)
        {
            try
            {
                // This method must be used before every L2P API call
                Tools.getAndSetUserToken(Request.Cookies, Context);
                if (Tools.isUserLoggedInAndAPIActive(Context) && !String.IsNullOrEmpty(cId))
                {
                    ViewData["ChosenCourse"] = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    ViewData["userRole"] = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);

                    L2PEmailList eList = await L2PAPIClient.api.Calls.L2PviewEmail(cId, hId);
                    if (eList != null)
                    {
                        EmailViewModel model = new EmailViewModel();
                        foreach (L2PEmailElement email in eList.dataSet)
                        {
                            model.recipients = email.recipients;
                            model.body = email.body;
                            model.itemId = email.itemId;
                            model.cc = email.cc;
                            model.sender = email.from;
                            model.subject = email.subject;
                            ViewData["attachments"] = email.attachments;
                        }
                        ViewData["EmailModel"] = model;

                        return View();
                    }
                    else
                    {
                        string errorMessage = "The Email you are trying to view does not exist.";
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



        // Get Method to delete Emails in a course
        // GET: /L2P/DeleteEmails
        [HttpGet]
        public async Task<IActionResult> DeleteEmails(string cId, string hIds)
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
                    else if (String.IsNullOrEmpty(hIds))
                    {
                        return RedirectToAction(nameof(MyCoursesController.Email), "MyCourses", new { @cId = cId });
                    }

                    L2PCourseInfoData course = await L2PAPIClient.api.Calls.L2PviewCourseInfoAsync(cId);
                    L2PRole userRole = await L2PAPIClient.api.Calls.L2PviewUserRoleAsync(cId);
                    if (userRole != null && (userRole.role.Contains("manager") || userRole.role.Contains("tutors")))
                    {
                        hIds = hIds.TrimEnd('-');
                        string[] emailIds = hIds.Split('-');
                        foreach (string hId in emailIds)
                        {
                            int id = -1;
                            int.TryParse(hId, out id);
                            await L2PAPIClient.api.Calls.L2PDeleteEmail(cId, id);
                        }
                        return RedirectToAction(nameof(MyCoursesController.Email), "MyCourses", new { @cId = cId, @msg = "Hyperlinks(s) successfully deleted!" });
                    }
                    else
                    {
                        string errorMessage = "You do not have the sufficient rights to delete emails";
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
