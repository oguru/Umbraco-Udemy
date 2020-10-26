using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using Umbraco_Udemy.Core.ViewModel;

namespace Umbraco_Udemy.Core.Controllers
{
    public class ContactController : SurfaceController
    {
        public ActionResult RenderContactForm()
        {
            var vm = new ContactFormViewModel();
            return PartialView("~/Views/Partials/Contact Form.cshtml", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleContactForm(ContactFormViewModel vm)
        {
            return null;
        }
    }
}
