using System;
using System.Linq;
using System.Web.Security;
using BookMyDesk.DataAccess;
using BookMyDesk.Models.EntityModels;

namespace MyDeskBooking.App_Start
{public class MyDeskBookingRoleProvider : RoleProvider, IDisposable
    {
        public override string ApplicationName { get; set; }

        private ApplicationDbContext db;
        
        public MyDeskBookingRoleProvider()
        {
            db = new ApplicationDbContext();
        }
        
        public void Dispose()
        {
            if (db != null)
            {
                db.Dispose();
            }
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            var user = db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return false;

            return (user.RoleId == 1 && roleName == "Admin") || 
                   (user.RoleId == 2 && roleName == "User");
        }

        public override string[] GetRolesForUser(string username)
        {
            var user = db.Users.FirstOrDefault(u => u.Username == username);
            if (user == null)
                return new string[] { };

            return new[] { user.RoleId == 1 ? "Admin" : "User" };
        }

        #region Not Implemented Methods

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            return roleName == "Admin" || roleName == "User";
        }

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override string[] GetUsersInRole(string roleName)
        {
            var roleId = roleName == "Admin" ? 1 : 2;
            return db.Users.Where(u => u.RoleId == roleId).Select(u => u.Username).ToArray();
        }

        public override string[] GetAllRoles()
        {
            return new[] { "Admin", "User" };
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            var roleId = roleName == "Admin" ? 1 : 2;
            return db.Users
                .Where(u => u.RoleId == roleId && u.Username.Contains(usernameToMatch))
                .Select(u => u.Username)
                .ToArray();
        }

        #endregion
    }
}