# Car Insurance — ASP.NET MVC Entity Framework Assignment Part 3

A standalone ASP.NET Core MVC application for Fariha Ahrar, built from the course's CarInsurance/Insuree specification. Entity Framework Core stores insurees and calculated monthly quotes in SQLite. No existing Parts 1–2 project is required.

## Run

Install the .NET 8 SDK. From this repository folder:

```powershell
dotnet restore
dotnet run --project CarInsurance.csproj --urls http://localhost:5080
```

Open http://localhost:5080. Alternatively, open `CarInsurance.csproj` in Visual Studio 2022 with the ASP.NET and web development workload, then run it. The database and schema are created automatically at `App_Data/CarInsurance.db`. Stop and restart the app to retain existing quotes. No SQL Server setup is required.

## Assignment requirements

- `Controllers/InsureeController.cs` calculates the quote on the server before saving a new or edited record.
- Base premium: $50/month. Age 18 or under adds $100; ages 19–25 add $50; ages 26 and above add $25. Age uses completed years as of the quote date.
- A car before 2000 or after 2015 adds $25. The boundary years 2000 and 2015 do not add a surcharge.
- Porsche adds $25, and Porsche 911 Carrera adds another $25. Matching ignores case and surrounding whitespace.
- Each speeding ticket adds $10. DUI multiplies that subtotal by 1.25, then full coverage multiplies the result by 1.50. Decimal arithmetic is rounded to cents once at the end, with midpoint values rounded away from zero.
- `Views/Insuree/Create.cshtml` and its shared fields partial contain no Quote input. The controller's binding allowlist and the model's `BindNever` attribute also prevent browser-supplied quotes from being trusted.
- `/Insuree/Admin` lists all currently stored quotes with first name, last name, email, and monthly quote.
- Index, Create, Details, Edit and Delete views provide the standard model operations described in the course.

Example: age 18, a 2016 Porsche 911 Carrera, two tickets, DUI and full coverage gives `(50 + 100 + 25 + 25 + 25 + 20) × 1.25 × 1.50 = $459.38/month`.

## Verify

```powershell
dotnet build CarInsurance.csproj
dotnet run --project Tests/CarInsurance.Tests.csproj
```

The test runner checks age and vehicle boundaries, birthday handling, car matching, tickets, both percentage increases, cent rounding, invalid birth dates, EF SQLite persistence and controller Create/Edit/Admin/Delete behavior.

## Scope

This is a local coursework demonstration. Admin and CRUD routes do not implement account authentication or authorization; use fictional data and do not deploy it as a public insurance service. No personal records, database files, SDK files, or generated build output are committed.

## Reference

[Microsoft Learn: Get started with EF Core in an ASP.NET MVC web app](https://learn.microsoft.com/en-us/aspnet/core/data/ef-mvc/intro) — MVC/DbContext setup and automatic database creation. Quote rules come from the course assignment.
