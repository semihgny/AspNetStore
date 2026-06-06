# AspNetStore

A comprehensive, full-featured e-commerce web application built from the ground up using **ASP.NET Core 8 MVC**. The application relies on Entity Framework Core 9 for data access and utilizes a lightweight SQLite database for ultimate portability.

## Features

- **Full E-Commerce Flow**: Browse products, manage categories, add items to a shopping cart, and place orders.
- **Order & Cart Management**: Dedicated models for Cart, CartItems, Order, and OrderDetails/OrderItems.
- **Coupon System**: Apply discount coupons during checkout.
- **Customer Support Tickets**: Built-in system for users to open and manage support tickets (`SupportTicket`).
- **Custom Authentication**: Cookie-based custom authentication system using `BCrypt` for secure password hashing, moving away from standard Identity for finer control.
- **Role-Based Authorization**: Distinct roles and policies for standard users and Administrators.
- **Auto-Seeding**: The application automatically creates the database schema and seeds a default Admin account upon first run.

## Technologies Used

- **Framework**: .NET 8.0 (ASP.NET Core MVC)
- **Language**: C# 12
- **ORM**: Entity Framework Core 9.0.5
- **Database**: SQLite (`app.db`)
- **Security**: 
  - Cookie Authentication (`CookieAuth`)
  - BCrypt Password Hashing (`BCrypt.Net-Next`)
- **State Management**: Distributed Memory Cache and Session State.

## Default Admin Credentials

Upon the first run, the system automatically creates an administrator account so you can manage the store immediately:

- **Email**: `admin@example.com`
- **Password**: `admin123`

## Requirements

- [.NET 8.0 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Visual Studio 2022 (recommended) or VS Code.

## Installation & Setup

1. **Clone the repository:**
   ```bash
   git clone https://github.com/semihgny/AspNetStore.git
   cd AspNetStore
   ```

2. **Restore dependencies:**
   ```bash
   dotnet restore
   ```

3. **Run the Application:**
   Because this project uses SQLite and automatically calls `context.Database.EnsureCreated()` and seeds data in `Program.cs`, there is no need to manually run migrations. Just start the app:
   ```bash
   dotnet run
   ```
   *Alternatively, press `F5` in Visual Studio.*

4. **Access the Store:**
   Navigate to the URL provided in the console (usually `https://localhost:5001` or `http://localhost:5000`). Log in with the default admin credentials to access the backend features.

## License

This project is open-source. See the repository or source files for usage details.
