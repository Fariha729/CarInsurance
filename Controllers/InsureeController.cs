using CarInsurance.Data;
using CarInsurance.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CarInsurance.Controllers;

public class InsureeController(InsuranceContext db) : Controller
{
    // Show every stored insuree; no tracking is needed for read-only pages.
    public async Task<IActionResult> Index() => View(await db.Insurees.AsNoTracking().OrderBy(i => i.Id).ToListAsync());

    public IActionResult Create() => View(new Insuree { CarYear = DateTime.Today.Year });

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("FirstName,LastName,EmailAddress,DateOfBirth,CarYear,CarMake,CarModel,DUI,SpeedingTickets,CoverageType")] Insuree insuree)
    {
        if (!ModelState.IsValid) return View(insuree);
        // Calculate on the server before saving. Quote and Id are excluded from binding.
        insuree.Quote = CalculateQuote(insuree, DateTime.Today);
        db.Insurees.Add(insuree);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id = insuree.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var insuree = await db.Insurees.AsNoTracking().SingleOrDefaultAsync(i => i.Id == id);
        return insuree == null ? NotFound() : View(insuree);
    }

    public async Task<IActionResult> Edit(int id)
    {
        var insuree = await db.Insurees.FindAsync(id);
        return insuree == null ? NotFound() : View(insuree);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, [Bind("FirstName,LastName,EmailAddress,DateOfBirth,CarYear,CarMake,CarModel,DUI,SpeedingTickets,CoverageType")] Insuree input)
    {
        var saved = await db.Insurees.FindAsync(id);
        if (saved == null) return NotFound();
        if (!ModelState.IsValid) { input.Id = id; return View(input); }
        // Update only permitted fields and recalculate the premium after changes.
        saved.FirstName = input.FirstName; saved.LastName = input.LastName;
        saved.EmailAddress = input.EmailAddress; saved.DateOfBirth = input.DateOfBirth;
        saved.CarYear = input.CarYear; saved.CarMake = input.CarMake; saved.CarModel = input.CarModel;
        saved.DUI = input.DUI; saved.SpeedingTickets = input.SpeedingTickets; saved.CoverageType = input.CoverageType;
        saved.Quote = CalculateQuote(saved, DateTime.Today);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Details), new { id });
    }

    public async Task<IActionResult> Delete(int id)
    {
        var insuree = await db.Insurees.AsNoTracking().SingleOrDefaultAsync(i => i.Id == id);
        return insuree == null ? NotFound() : View(insuree);
    }

    [HttpPost, ActionName("Delete"), ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var insuree = await db.Insurees.FindAsync(id);
        if (insuree == null) return NotFound();
        db.Insurees.Remove(insuree);
        await db.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    // Assignment Admin view: display all issued quotes and applicant contact details.
    public async Task<IActionResult> Admin() => View(await db.Insurees.AsNoTracking().OrderBy(i => i.Id).ToListAsync());

    public static decimal CalculateQuote(Insuree insuree, DateTime today)
    {
        // Calculate completed years, including applicants whose birthday is still ahead.
        today = today.Date;
        var birthDate = insuree.DateOfBirth.Date;
        if (birthDate == default || birthDate > today) throw new ArgumentException("Invalid birth date.");
        if (insuree.SpeedingTickets < 0) throw new ArgumentException("Tickets cannot be negative.");
        int age = today.Year - birthDate.Year;
        if (birthDate.AddYears(age) > today) age--;

        decimal quote = 50m;
        // These mutually exclusive branches cover every nonnegative age.
        quote += age <= 18 ? 100m : age <= 25 ? 50m : 25m;
        if (insuree.CarYear < 2000 || insuree.CarYear > 2015) quote += 25m;

        // Normalize whitespace and case so equivalent car names receive the same rate.
        if (string.Equals(insuree.CarMake.Trim(), "Porsche", StringComparison.OrdinalIgnoreCase))
        {
            quote += 25m;
            var model = string.Join(" ", insuree.CarModel.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
            if (string.Equals(model, "911 Carrera", StringComparison.OrdinalIgnoreCase)) quote += 25m;
        }

        quote += insuree.SpeedingTickets * 10m;
        // Apply percentage surcharges in the assignment's order, then round once.
        if (insuree.DUI) quote *= 1.25m;
        if (insuree.CoverageType) quote *= 1.50m;
        return decimal.Round(quote, 2, MidpointRounding.AwayFromZero);
    }
}
