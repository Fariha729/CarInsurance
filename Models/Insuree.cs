using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace CarInsurance.Models;

// Store the applicant, vehicle, driving history, coverage and calculated premium.
public class Insuree : IValidatableObject
{
    public int Id { get; set; }
    [Required, StringLength(60), Display(Name = "First name")]
    public string FirstName { get; set; } = "";
    [Required, StringLength(60), Display(Name = "Last name")]
    public string LastName { get; set; } = "";
    [Required, EmailAddress, StringLength(254), Display(Name = "Email address")]
    public string EmailAddress { get; set; } = "";
    [Required, DataType(DataType.Date), Display(Name = "Date of birth")]
    public DateTime DateOfBirth { get; set; }
    [Range(1886, 9999), Display(Name = "Car year")]
    public int CarYear { get; set; }
    [Required, StringLength(60), Display(Name = "Car make")]
    public string CarMake { get; set; } = "";
    [Required, StringLength(80), Display(Name = "Car model")]
    public string CarModel { get; set; } = "";
    [Display(Name = "Ever had a DUI?")]
    public bool DUI { get; set; }
    [Range(0, 1000), Display(Name = "Speeding tickets")]
    public int SpeedingTickets { get; set; }
    [Display(Name = "Full coverage")]
    public bool CoverageType { get; set; }
    // Never accept a premium supplied by a browser; the controller calculates it.
    [BindNever, DataType(DataType.Currency)]
    public decimal Quote { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (DateOfBirth == default || DateOfBirth.Date > DateTime.Today)
            yield return new ValidationResult("Enter a valid date of birth that is not in the future.", new[] { nameof(DateOfBirth) });
        if (CarYear > DateTime.Today.Year + 1)
            yield return new ValidationResult("The car year cannot be more than one year in the future.", new[] { nameof(CarYear) });
    }
}
