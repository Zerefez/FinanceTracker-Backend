using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Models;

[Table("Jobs")]
[PrimaryKey(nameof(CompanyName), nameof(UserId))]
public class Job
{
    [Required]
    public string CompanyName { get; set; }

    [Required]
    public string UserId { get; set; }

    [ForeignKey("UserId")]
    [JsonIgnore]
    public FinanceUser User { get; set; }

    public string? Title { get; set; }

    public string? EmploymentType { get; set; }

    public string? TaxCard { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal HourlyRate { get; set; }
}