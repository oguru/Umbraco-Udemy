using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using Umbraco_Udemy.Core.ViewModel;
/*using Umbraco_Udemy.Core.Interfaces;*/

namespace Umbraco_Udemy.Core.Controllers
{
    public class RegisterController : SurfaceController
    {
        private const string PARTIAL_VIEW_FOLDER = "~/Views/Partials/";
/*        private IEmailService _emailService;

        public RegisterController(IEmailService emailService)
        {
            _emailService = emailService;
        }*/

        #region Register Form
        /// <summary>
        /// Render the registration form
        /// </summary>
        /// <returns></returns>
        public ActionResult RenderRegister()
        {
            var vm = new RegisterViewModel();
            return PartialView(PARTIAL_VIEW_FOLDER + "Register.cshtml", vm);
        }

        /// <summary>
        /// Handle the registration form post
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HandleRegister(RegisterViewModel vm)
        {
            //If form not valid - return
            if (!ModelState.IsValid)
            {
                return CurrentUmbracoPage();
            }

            //Check if there is already a member with that email address
            var existingMember = Services.MemberService.GetByEmail(vm.EmailAddress);

            if (existingMember != null)
            {
                ModelState.AddModelError("Account Error", "There's already a user with that email address.");
                return CurrentUmbracoPage();
            }

            //Check if their username is already in use
            existingMember = Services.MemberService.GetByUsername(vm.Username);

            if (existingMember != null)
            {
                ModelState.AddModelError("Account Error", "There's already a user with that username. Please choose a different one.");
                return CurrentUmbracoPage();
            }

            //Create "member" in Umbraco with the details
            var newMember = Services.MemberService
                                .CreateMember(vm.Username, vm.EmailAddress, $"{vm.FirstName} {vm.LastName}", "Member");
            newMember.PasswordQuestion = "";
            newMember.RawPasswordAnswerValue = "";
            //Need to save the member before you can set the password
            Services.MemberService.Save(newMember);
            Services.MemberService.SavePassword(newMember, vm.Password);
            //Assign a role - i.e. Normal User
            Services.MemberService.AssignRole(newMember.Id, "Normal User");

            //Create email verification token
            //Token creation
            //A GUID is guaranteed to be unique
            var token = Guid.NewGuid().ToString();
            newMember.SetValue("emailVerifyToken", token);
            Services.MemberService.Save(newMember);

            //Send email verification
            /*_emailService.SendVerifyEmailAddressNotification(newMember.Email, token);*/


            //Thank the user
            //Return confirmation message to user

            return RedirectToCurrentUmbracoPage();
        }
        #endregion



    }
}
