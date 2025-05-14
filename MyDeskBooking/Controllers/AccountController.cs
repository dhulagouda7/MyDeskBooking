using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using BookMyDesk.Models.EntityModels;
using BookMyDesk.Services.DataAccess;

namespace BookMyDesk.Controllers
{
    public class AccountController : Controller
    {
        private readonly IRepository<User> _userRepository;

        public AccountController(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;
            return View();
        }        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(string username, string password, string returnUrl)
        {            // Get user and validate status
            var users = await _userRepository.GetAllAsync();
            var user = users.FirstOrDefault(u => u.Username == username);

            if (user != null)
            {
                if (!user.IsActive)
                {
                    ModelState.AddModelError("", "Your account has been deactivated. Please contact an administrator.");
                    return View();
                }

                // Create the authentication ticket
                string roleName = user.RoleId == 1 ? "Admin" : "User";
                
                // Create a custom FormsAuthenticationTicket with user role
                var ticket = new FormsAuthenticationTicket(
                    1,                          // ticket version
                    username,                   // username
                    DateTime.Now,               // issue time
                    DateTime.Now.AddMinutes(30),// expiration
                    false,                      // persistent
                    roleName                    // user data - storing the role
                );
                
                // Encrypt the ticket
                string encryptedTicket = FormsAuthentication.Encrypt(ticket);
                
                // Create a cookie with the encrypted ticket
                var authCookie = new System.Web.HttpCookie(FormsAuthentication.FormsCookieName, encryptedTicket);
                System.Web.HttpContext.Current.Response.Cookies.Add(authCookie);
                
                return RedirectToLocal(returnUrl);
            }

            ModelState.AddModelError("", "Invalid username or password.");
            return View();
        }

        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index", "Dashboard");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Dashboard");
        }
    }
}
