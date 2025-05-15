using System;
using System.Collections.Generic;
using System.Data.Entity;
using MyDeskBooking.DataAccess;
using MyDeskBooking.Models.EntityModels;

namespace MyDeskBooking.DataAccess
{    public class MyDeskBookingInitializer : CreateDatabaseIfNotExists<ApplicationDbContext> //CreateDatabaseIfNotExists<ApplicationDbContext>
    {
        protected override void Seed(ApplicationDbContext context)
        {
            // Seed users
            var users = new List<User>
            {
                new User { UserId = 1, Username = "admin", Email = "admin@anjusoftware.com", RoleId = 1, TeamId = 1, IsActive = true },
                new User { UserId = 2, Username = "user1", Email = "john@anjusoftware.com", RoleId = 2, TeamId = 1, IsActive = true },
                new User { UserId = 3, Username = "user2", Email = "jane@anjusoftware.com", RoleId = 2, TeamId = 2, IsActive = true }
            };
            users.ForEach(u => context.Users.Add(u));
            
            // Seed locations
            var locations = new List<Location>
            {
                new Location { LocationId = 1, LocationName = "Montreal, Canada", Address = "7405 Rte Transcanadienne Suite 100 Montreal QC H4T 1Z2 Canada" },
                new Location { LocationId = 2, LocationName = "Bangalore,India", Address = "No. 38/4, Doddanekundi, Outer Ring Road, Bengaluru, 560048, Karnataka, India" },
                new Location { LocationId = 3, LocationName = "Pune, India", Address = "Pavillion, SO#601, 6th Floor, S.B Road, Shivaji Nagar, Pune 411-016, India" }
            };
            locations.ForEach(l => context.Locations.Add(l));

            // Seed buildings
            var buildings = new List<Building>
            {
                new Building { BuildingId = 1, LocationId = 1, BuildingName = "7405 Rte Transcanadienne #100" },
                new Building { BuildingId = 2, LocationId = 2, BuildingName = "Indiqube" },
                new Building { BuildingId = 3, LocationId = 3, BuildingName = "Pavillion Mall" }
            };
            buildings.ForEach(b => context.Buildings.Add(b));            // Seed floors
            var floors = new List<Floor>
            {
                new Floor { FloorId = 1, BuildingId = 1, FloorNumber = 1 },
                new Floor { FloorId = 2, BuildingId = 1, FloorNumber = 2 },
                new Floor { FloorId = 3, BuildingId = 1, FloorNumber = 3 },
                new Floor { FloorId = 4, BuildingId = 2, FloorNumber = 5 },
                new Floor { FloorId = 5, BuildingId = 3, FloorNumber = 5 },
                new Floor { FloorId = 6 ,BuildingId = 3, FloorNumber = 7 }
            };
            floors.ForEach(f => context.Floors.Add(f));            // Seed desks
            var desks = new List<Desk>
            {
                new Desk { DeskId = 1, FloorId = 1, DeskNumber = "A1", Status = "Available", IsReserved = false },
                new Desk { DeskId = 2, FloorId = 1, DeskNumber = "A2", Status = "Available", IsReserved = false },
                new Desk { DeskId = 3, FloorId = 2, DeskNumber = "B1", Status = "Available", IsReserved = false },
                new Desk { DeskId = 4, FloorId = 2, DeskNumber = "B1", Status = "Available", IsReserved = false },
                new Desk { DeskId = 5, FloorId = 3, DeskNumber = "C1", Status = "Available", IsReserved = false },
                new Desk { DeskId = 6, FloorId = 3, DeskNumber = "C2", Status = "Available", IsReserved = false },
                new Desk { DeskId = 7, FloorId = 4, DeskNumber = "D1", Status = "Available", IsReserved = false },
                new Desk { DeskId = 8, FloorId = 4, DeskNumber = "D2", Status = "Available", IsReserved = false },
                new Desk { DeskId = 9, FloorId = 4, DeskNumber = "D3", Status = "Available", IsReserved = false },
                new Desk { DeskId = 10, FloorId = 5, DeskNumber = "E1", Status = "Available", IsReserved = false },
                new Desk { DeskId = 11, FloorId = 5, DeskNumber = "E2", Status = "Available", IsReserved = false },
                new Desk { DeskId = 12, FloorId = 6, DeskNumber = "F1", Status = "Available", IsReserved = false },
                new Desk { DeskId = 13, FloorId = 6, DeskNumber = "F2", Status = "Available", IsReserved = false },
                new Desk { DeskId = 14, FloorId = 6, DeskNumber = "F3", Status = "Available", IsReserved = false },
                new Desk { DeskId = 15, FloorId = 6, DeskNumber = "F4", Status = "Available", IsReserved = false }
            };
            desks.ForEach(d => context.Desks.Add(d));

            context.SaveChanges();

            base.Seed(context);
        }
    }    
    public static class DatabaseInitializer
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
                    context.Database.Initialize(force: false);
                    
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
