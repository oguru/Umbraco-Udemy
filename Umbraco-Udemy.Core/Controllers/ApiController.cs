using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Umbraco.Web.Mvc;

namespace Umbraco_Udemy.Core.Controllers
{
    public class ApiController : SurfaceController
    {
        public ActionResult GetUserData(string apiLink)
        {
            string PARTIAL_VIEW_FOLDER = "~Views/Partials";

            return PartialView(PARTIAL_VIEW_FOLDER + "/User Data.cshtml", apiLink);
        }
    }
}
