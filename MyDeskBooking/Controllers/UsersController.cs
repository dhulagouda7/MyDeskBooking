using System;
using System.Threading.Tasks;
using System.Web.Mvc;
using System.Web.Security;
using MyDeskBooking.Models.EntityModels;
using MyDeskBooking.Services.DataAccess;

namespace MyDeskBooking.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : Controller
    {
        private readonly IRepository<User> _userRepository;

        public UsersController(IRepository<User> userRepository)
        {
            _userRepository = userRepository;
        }

        // GET: Users
        public async Task<ActionResult> Index()
        {
            var users = await _userRepository.GetAllAsync();
            return View(users);
        }

        // GET: Users/Create
        public ActionResult Create()
        {
            ViewBag.RoleId = new SelectList(new[] 
            {
                new { Id = 1, Name = "Admin" },
                new { Id = 2, Name = "User" }
            }, "Id", "Name");
            return View();
        }

        // POST: Users/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create([Bind(Include = "Username,Email,RoleId,TeamId,IsActive")] User user)
        {
            if (ModelState.IsValid)
            {
                user.IsActive = true;
                await _userRepository.AddAsync(user);
                TempData["SuccessMessage"] = "User created successfully!";
                return RedirectToAction("Index");
            }

            ViewBag.RoleId = new SelectList(new[] 
            {
                new { Id = 1, Name = "Admin" },
                new { Id = 2, Name = "User" }
            }, "Id", "Name", user.RoleId);
            return View(user);
        }

        // GET: Users/Edit/5
        public async Task<ActionResult> Edit(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }

            ViewBag.RoleId = new SelectList(new[] 
            {
                new { Id = 1, Name = "Admin" },
                new { Id = 2, Name = "User" }
            }, "Id", "Name", user.RoleId);
            return View(user);
        }

        // POST: Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit([Bind(Include = "UserId,Username,Email,RoleId,TeamId,IsActive")] User user)
        {            if (ModelState.IsValid)
            {
                var existingUser = await _userRepository.GetByIdAsync(user.UserId);
                
                // If the user is being deactivated
                if (existingUser.IsActive && !user.IsActive)
                {
                    // Force logout by removing their authentication cookie if they are currently logged in
                    var currentUser = User.Identity.Name;
                    if (currentUser == existingUser.Username)
                    {
                        FormsAuthentication.SignOut();
                    }
                }

                await _userRepository.UpdateAsync(user);
                TempData["SuccessMessage"] = "User updated successfully!";
                return RedirectToAction("Index");
            }

            ViewBag.RoleId = new SelectList(new[] 
            {
                new { Id = 1, Name = "Admin" },
                new { Id = 2, Name = "User" }
            }, "Id", "Name", user.RoleId);
            return View(user);
        }

        // GET: Users/Delete/5
        public async Task<ActionResult> Delete(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return HttpNotFound();
            }
            return View(user);
        }

        // POST: Users/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteConfirmed(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user != null)
            {
                await _userRepository.DeleteAsync(id);
                TempData["SuccessMessage"] = "User deleted successfully!";
            }
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                // No need to dispose _userRepository as it's handled by dependency injection
            }
            base.Dispose(disposing);
        }
    }
}