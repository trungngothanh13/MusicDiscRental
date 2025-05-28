# Music Disc Rental System

A WPF desktop application for managing music disc rentals, built with C# and Oracle Database integration.

## Overview

This application provides a complete rental management system for music discs with the following features:

- **Disc Management**: Add, edit, delete, and search music discs with stock tracking
- **Member Management**: Manage customer information and membership details
- **Rental Dashboard**: Handle disc rentals and returns with real-time tracking
- **Database Integration**: Full Oracle Database support with fallback to mock data

## Technology Stack

- **Frontend**: WPF (Windows Presentation Foundation) with MVVM pattern
- **Backend**: C# .NET Framework 4.7.2
- **Database**: Oracle Database 19c with stored procedures
- **Architecture**: Repository pattern with dependency injection

## Prerequisites

- .NET Framework 4.7.2 or higher
- Oracle Database 19c (optional - app includes mock data fallback)
- Visual Studio 2019 or later
- Oracle.ManagedDataAccess.Client NuGet package

## Database Setup
1. **Create a new User in Oracle**
   - Open SQL Plus
   - `CREATE USER C##MUSICADMIN identified by music_password;`
     `grant Connect, Resource, UNLIMITED TABLESPACE to C##MUSICADMIN;`
3. **Run the SQL script** (`Disc_Rental.sql`) in your Oracle database to create:
   - Tables: `Discs`, `Members`, `Rentals`
   - Sequences for auto-incrementing IDs
   - Stored procedures: `RentDisc`, `ReturnDisc`
   - Functions: `GetActiveRentals`
   - Sample test data

4. **Connection string** in `App.config`:
   ```xml
   <connectionStrings>
     <add name="MusicDiscRentalConnection"
          connectionString="User Id=C##MUSICADMIN;Password=music_password;Data Source=localhost:1521/orcl;"
          providerName="Oracle.ManagedDataAccess.Client" />
   </connectionStrings>
   ```

## How to Use

### Running the Application

1. Clone the repository
2. Open `MusicDiscRental.sln` in Visual Studio
3. Restore NuGet packages
4. Build and run the application

The app will automatically:
- Try to connect to Oracle Database
- Fall back to mock data if connection fails
- Display connection status in the footer

## Project Structure

```
MusicDiscRental/
├── Models/                 # Data models (Disc, Member, Rental)
├── ViewModels/            # MVVM view models with business logic
├── Views/                 # WPF user controls and windows
├── Database/
│   ├── DBConnection/      # Database service interfaces and implementations
│   └── Repositories/      # Data access layer
├── Common/                # Shared utilities (RelayCommand)
├── Converters/            # WPF value converters
└── Scripts/               # SQL database setup script
```

## Troubleshooting

- **Database Connection Issues**: Check Oracle service status and connection string
- **Mock Data Mode**: Application shows "Using Mock Database" when Oracle is unavailable
- **Build Errors**: Ensure Oracle.ManagedDataAccess.Client NuGet package is installed
