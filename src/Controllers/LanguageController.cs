using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using Grp.L2PSite.MobileApp.Services;
using System.Threading;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace Grp.L2PSite.MobileApp.Controllers
{
    public class LanguageController : Controller
    {
        public IAsyncResult BeginExecuteCore(AsyncCallback callback, object state)
        {
            string cultureName = null;

            // Attempt to read the culture cookie from Request
            var cultureCookie = Request.Cookies["_culture"];
            //Request.Cookies["_culture"];

            if (cultureCookie != null)
            {
                cultureName = cultureCookie;
            }
            else
            {
                //cultureName = Request.UserLanguages != null && Request.UserLanguages.Length > 0 ? Request.UserLanguages[0] : null; // obtain it from HTTP header AcceptLanguages
                cultureName = "en-US";
            }

            // Validate culture name
            cultureName = CultureHelper.GetImplementedCulture(cultureName); // This is safe


            // Modify current thread's cultures            
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(cultureName);
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;


            return BeginExecuteCore(callback, state);
        }

        // GET: /<controller>/
        public IActionResult Index()
        {
            return View();
        }
    }
}
