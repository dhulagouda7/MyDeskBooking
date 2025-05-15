MyDeskBooking System
===================

A comprehensive enterprise desk booking management system built with ASP.NET MVC.

Table of Contents
----------------
1. Overview
2. System Requirements
3. Project Setup
4. Features
5. Architecture
6. Configuration
7. Usage Guide
8. Development Guide

1. Overview
-----------
MyDeskBooking is a web-based application that enables organizations to efficiently manage their office workspace. It provides functionalities for desk booking, space management, and administrative controls.

2. System Requirements
---------------------
- Windows Server 2019/2022 or Windows 10/11
- .NET Framework 4.8
- SQL Server 2019 or later
- IIS 10.0 or later
- Visual Studio 2022 (for development)

3. Project Setup
---------------
1. Clone the repository
2. Open MyDeskBooking.sln in Visual Studio
3. Restore NuGet packages
4.	Update the connection string in Web.config
5.	Run application


4. Features
----------
User Features:
- Desk booking system
- View available desks
- Manage bookings
- Check-in/check-out functionality

Admin Features:
- Location management
- Building management
- Floor management
- Desk management
- User management
- System reporting

5. Architecture
--------------
The solution follows a layered architecture:

- Presentation Layer (MVC)
  - Controllers/
  - Views/
  - Models/

- Business Logic Layer
  - Services/BusinessLogic/
  - Services/DataAccess/

- Data Access Layer
  - DataAccess/
  - Models/EntityModels/

Key Components:
- Entity Framework for ORM
- Unity for Dependency Injection
- Repository pattern for data access
- Service layer for business logic

6. Configuration
---------------
Key configuration files:
- Web.config: Database connection and application settings
- App_Start/UnityConfig.cs: Dependency injection setup
- App_Start/RouteConfig.cs: URL routing
- App_Start/MyDeskBookingRoleProvider.cs: Authentication

7. Usage Guide
-------------
Regular Users:
1. Login to the system
2. View available desks using filters
3. Book a desk for a specific date/time
4. Manage bookings through dashboard
5. Check-in/out of booked desks

Administrators:
1. Access admin dashboard
2. Manage locations/buildings/floors/desks
3. View system reports
4. Manage user accounts
5. Configure system settings

8. Development Guide
-------------------
Project Structure:
- Controllers/: MVC controllers
- Models/: View models and entity models
- Views/: Razor views
- Services/: Business logic and data access
- DataAccess/: Database context and configurations

Adding New Features:
1. Create/modify entity models
2. Add migrations if needed
3. Create/update repository
4. Implement service layer logic
5. Create/update controller
6. Add/modify views

Testing:
- Unit tests for services
- Integration tests for repositories
- UI tests for critical paths

Dependencies
-----------
- EntityFramework
- Unity.Mvc
- ClosedXML
- Newtonsoft.Json
- System.Web.Mvc

Support
-------
For support and documentation:
- Check the docs/ folder
- Review inline code documentation
- Contact system administrators

License
-------
Proprietary software. All rights reserved.

Version
-------
Current Version: 1.0
Last Updated: May 2025
