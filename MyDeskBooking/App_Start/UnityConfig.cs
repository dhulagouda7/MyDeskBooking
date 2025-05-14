using System;
using System.Web.Mvc;
using MyDeskBooking.Controllers;
using MyDeskBooking.DataAccess;
using MyDeskBooking.Models.EntityModels;
using MyDeskBooking.Services.BusinessLogic;
using MyDeskBooking.Services.DataAccess;
using Unity;
using Unity.Injection;
using Unity.Lifetime;
using Unity.Mvc5;

namespace MyDeskBooking
{
    public static class UnityConfig
    {
        public static void RegisterComponents()
        {
            var container = new UnityContainer();

            // Register DbContext with container-controlled lifetime (one instance per HTTP request)
            container.RegisterType<ApplicationDbContext>(new HierarchicalLifetimeManager());            // Register your dependencies
            container.RegisterType<IBookingRepository, BookingRepository>();
            container.RegisterType<IBookingValidationService, BookingValidationService>();
            container.RegisterType<IBookingService, BookingService>();
            
            // Register generic repositories with factory method to handle proper generic type resolution
            container.RegisterType(typeof(IRepository<>), typeof(Repository<>));

            // Set the dependency resolver for MVC
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
        }
    }
}
