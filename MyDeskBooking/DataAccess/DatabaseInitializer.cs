using System;
using System.Collections.Generic;
using System.Data.Entity;
using BookMyDesk.DataAccess;
using BookMyDesk.Models.EntityModels;

namespace MyDeskBooking.DataAccess
{
    public class MyDeskBookingInitializer : DropCreateDatabaseAlways<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            // Seed users
            var users = new List<User>
            {
                new User { UserId = 1, Username = "admin", Email = "admin@mydesk.com", RoleId = 1, TeamId = 1, IsActive = true },
                new User { UserId = 2, Username = "user1", Email = "john@example.com", RoleId = 2, TeamId = 1, IsActive = true },
                new User { UserId = 3, Username = "user2", Email = "jane@example.com", RoleId = 2, TeamId = 2, IsActive = true }
            };
            users.ForEach(u => context.Users.Add(u));
            
            // Seed locations
            var locations = new List<Location>
            {
                new Location { LocationId = 1, LocationName = "Headquarters", Address = "123 Main St, New York, NY 10001" },
                new Location { LocationId = 2, LocationName = "Branch Office", Address = "456 Park Ave, Boston, MA 02108" }
            };
            locations.ForEach(l => context.Locations.Add(l));

            // Seed buildings
            var buildings = new List<Building>
            {
                new Building { BuildingId = 1, LocationId = 1, BuildingName = "Main Tower" },
                new Building { BuildingId = 2, LocationId = 1, BuildingName = "Annex Building" },
                new Building { BuildingId = 3, LocationId = 2, BuildingName = "Boston Office" }
            };
            buildings.ForEach(b => context.Buildings.Add(b));            // Seed floors
            var floors = new List<Floor>
            {
                new Floor { FloorId = 1, BuildingId = 1, FloorNumber = 1 },
                new Floor { FloorId = 2, BuildingId = 1, FloorNumber = 2 },
                new Floor { FloorId = 3, BuildingId = 2, FloorNumber = 0 },
                new Floor { FloorId = 4, BuildingId = 3, FloorNumber = 1 }
            };
            floors.ForEach(f => context.Floors.Add(f));            // Seed desks
            var desks = new List<Desk>
            {
                new Desk { DeskId = 1, FloorId = 1, DeskNumber = "A1", Status = "Available", IsReserved = false },
                new Desk { DeskId = 2, FloorId = 1, DeskNumber = "A2", Status = "Available", IsReserved = false },
                new Desk { DeskId = 3, FloorId = 2, DeskNumber = "B1", Status = "Available", IsReserved = false },
                new Desk { DeskId = 4, FloorId = 3, DeskNumber = "C1", Status = "Available", IsReserved = false },
                new Desk { DeskId = 5, FloorId = 4, DeskNumber = "D1", Status = "Available", IsReserved = false }
            };
            desks.ForEach(d => context.Desks.Add(d));

            context.SaveChanges();

            base.Seed(context);
        }
    }    public static class DatabaseInitializer
    {
        public static void Initialize()
        {
            try
            {
                // Set the database initializer to recreate and seed the database
                Database.SetInitializer<ApplicationDbContext>(new MyDeskBookingInitializer());
                
                // Force the initialization and creation of the database
                using (var context = new ApplicationDbContext())
                {
                    // This will create the database if it doesn't exist and seed it
                    context.Database.Initialize(force: true);
                    
                    // Ensure we can connect to the database
                    if (context.Database.Exists())
                    {
                        System.Diagnostics.Debug.WriteLine("Database created successfully!");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error creating database: {ex.Message}");
                // Log the full exception for debugging
                System.Diagnostics.Debug.WriteLine($"Exception details: {ex}");
            }
        }
    }
}
