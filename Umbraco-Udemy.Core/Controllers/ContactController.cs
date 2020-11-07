using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Umbraco.Web;
using Umbraco.Web.Mvc;
using Umbraco_Udemy.Core.ViewModel;
using Umbraco.Core.Logging;
using System.Net.Mail;

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
            //check validity
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("Error", "Please check the form.");
                return CurrentUmbracoPage();
            }

            try
            {
                //Create a new contact form in umbraco
                //Get a handle to "Contact Forms" for the parentId in newContact
                //Querying all content in content tab, looking anything of type contactForms
                var contactForms = Umbraco.ContentAtRoot().DescendantsOrSelfOfType("contactForms").FirstOrDefault();

                if (contactForms != null)
                {
                    //Asks ContentService to create new Contact record, set values, then save and publish content on Umbraco
                    //Provide name to save as (Contact), parent - must be ID/GUID of the item in content which is the contact forms doc type (contactForms.Id), and doc type alias (contactForm)
                    var newContact = Services.ContentService.Create("Contact", contactForms.Id, "contactForm");
                    //set values for each field
                    newContact.SetValue("contactName", vm.Name);
                    newContact.SetValue("contactEmail", vm.EmailAddress);
                    newContact.SetValue("contactSubject", vm.Subject);
                    newContact.SetValue("contactComments", vm.Comment);
                    //Save and publish on Umbraco
                    Services.ContentService.SaveAndPublish(newContact);
                }

                //Send out an email to site admin
                SendContactFormReceivedEmail(vm);

                //Return confirmation message to user in a variable called status
                TempData["status"] = "OK";

                //redirect so if refresh page, form won't resubmit
                return RedirectToCurrentUmbracoPage();
            }

            catch (Exception exc)
            {
                //logging from the ContactController
                Logger.Error<ContactController>("There was an error in the contact form submission", exc.Message);
                //set model state and add error to return to user
                ModelState.AddModelError("Error", "Sorry there was a problem noting your details. Would you please try again later?");
            }

            return CurrentUmbracoPage();

        }

        /// <summary>
        /// This will send out an email to site admins saying a contact form has been submitted
        /// </summary>
        /// <param name="vm"></param>
        private void SendContactFormReceivedEmail(ContactFormViewModel vm)
        {
            //Read email FROM and TO addresses
            //Get site settings
            var siteSettings = Umbraco.ContentAtRoot().DescendantsOrSelfOfType("siteSettings").FirstOrDefault();
            if (siteSettings == null)
            {
                throw new Exception("There are no site settings");
            }

            var fromAddress = siteSettings.Value<string>("emailSettingsFromAddress");
            var toAddresses = siteSettings.Value<string>("emailSettingsAdminAccounts");

            if (string.IsNullOrEmpty(fromAddress))
            {
                throw new Exception("There needs to be a from address in site settings");
            }
            if (string.IsNullOrEmpty(toAddresses))
            {
                throw new Exception("There needs to be a to address in site settings");
            }


            //Construct the actual email
            var emailSubject = "There has been a contact form submitted";
            var emailBody = $"A new contact form has been received from {vm.Name}. Their comments were: {vm.Comment}";
            var smtpMessage = new MailMessage();
            smtpMessage.Subject = emailSubject;
            smtpMessage.Body = emailBody;
            smtpMessage.From = new MailAddress(fromAddress);

            var toList = toAddresses.Split(',');
            foreach (var item in toList)
            {
                if (!string.IsNullOrEmpty(item))
                    smtpMessage.To.Add(item);
            }


            //Send via whatever email service set in web.config
            using (var smtp = new SmtpClient())
            {
                smtp.Send(smtpMessage);
            }
        }
    }
}
