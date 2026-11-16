using CarInsurance.Controllers;
using CarInsurance.Data;
using CarInsurance.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

var today = new DateTime(2026, 11, 11);
int passed = 0;
void Check(bool condition, string name)
{
    if (!condition) throw new Exception("FAIL: " + name);
    Console.WriteLine("PASS: " + name);
    passed++;
}
Insuree Applicant(int age = 30) => new() { FirstName = "Test", LastName = "Applicant", EmailAddress = "test@example.com", DateOfBirth = today.AddYears(-age), CarYear = 2010, CarMake = "Toyota", CarModel = "Corolla" };
void Quote(string name, Insuree item, decimal expected) => Check(InsureeController.CalculateQuote(item, today) == expected, name);

// Check all age brackets and the birthday boundary, independently of the machine date.
foreach (var pair in new[] { (0, 150m), (18, 150m), (19, 100m), (25, 100m), (26, 75m), (80, 75m) })
    Quote("Age " + pair.Item1, Applicant(pair.Item1), pair.Item2);
var birthday = Applicant(26); birthday.DateOfBirth = birthday.DateOfBirth.AddDays(1);
Quote("26th birthday is tomorrow", birthday, 100m);
foreach (var pair in new[] { (1999, 100m), (2000, 75m), (2015, 75m), (2016, 100m) })
{ var item = Applicant(); item.CarYear = pair.Item1; Quote("Car year " + pair.Item1, item, pair.Item2); }
var porsche = Applicant(); porsche.CarMake = " pOrScHe "; porsche.CarModel = "Cayenne";
Quote("Porsche make", porsche, 100m);
porsche.CarModel = " 911   carrera "; Quote("Porsche Carrera additional surcharge", porsche, 125m);
var other = Applicant(); other.CarModel = "911 Carrera"; Quote("Carrera name without Porsche make", other, 75m);
var tickets = Applicant(); tickets.SpeedingTickets = 3; Quote("Three tickets", tickets, 105m);
var dui = Applicant(); dui.DUI = true; Quote("DUI only", dui, 93.75m);
var full = Applicant(); full.CoverageType = true; Quote("Full coverage only", full, 112.50m);
dui.CoverageType = true; Quote("Both percentages and final cent rounding", dui, 140.63m);
var combined = Applicant(18); combined.CarYear = 2016; combined.CarMake = "Porsche"; combined.CarModel = "911 Carrera"; combined.SpeedingTickets = 2; combined.DUI = true; combined.CoverageType = true;
Quote("All surcharges: 245 x 1.25 x 1.5", combined, 459.38m);
var future = Applicant(); future.DateOfBirth = today.AddDays(1);
try { InsureeController.CalculateQuote(future, today); throw new Exception("Future DOB accepted"); }
catch (ArgumentException) { Check(true, "Future birth date rejected"); }

// Use a real SQLite database held in memory to exercise EF and controller actions.
using var connection = new SqliteConnection("Data Source=:memory:");
connection.Open();
var options = new DbContextOptionsBuilder<InsuranceContext>().UseSqlite(connection).Options;
await using var db = new InsuranceContext(options);
await db.Database.EnsureCreatedAsync();
var controller = new InsureeController(db);
var saved = Applicant(); saved.DateOfBirth = DateTime.Today.AddYears(-30); saved.Quote = 1m;
Check(await controller.Create(saved) is RedirectToActionResult, "Create redirects after save");
Check(await db.Insurees.CountAsync() == 1 && saved.Quote == 75m, "Server overwrites supplied quote and persists it");
var admin = (ViewResult)await controller.Admin();
Check(((IEnumerable<Insuree>)admin.Model!).Single().EmailAddress == "test@example.com", "Admin returns quote and contact data");
var edited = Applicant(); edited.DateOfBirth = DateTime.Today.AddYears(-30); edited.SpeedingTickets = 2;
await controller.Edit(saved.Id, edited);
Check((await db.Insurees.SingleAsync()).Quote == 95m, "Edit recalculates stored quote");
controller.ModelState.AddModelError("EmailAddress", "Invalid email");
Check(await controller.Create(Applicant()) is ViewResult && await db.Insurees.CountAsync() == 1, "Invalid form is not saved");
Check(await controller.Details(9999) is NotFoundResult, "Unknown record returns 404");
controller.ModelState.Clear();
await controller.DeleteConfirmed(saved.Id);
Check(await db.Insurees.CountAsync() == 0, "Delete removes record");
Console.WriteLine($"All {passed} checks passed.");
