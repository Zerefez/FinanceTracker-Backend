using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceTracker.Models;

[Table("SupplementDetails")]
[PrimaryKey(nameof(Weekday), nameof(CompanyName))]
public class SupplementDetails
{
    [Required]
    public DayOfWeek Weekday { get; set; }
    
    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal Amount { get; set; }
    
    [Required]
    public DateTime StartTime { get; set; }
    
    [Required]
    public DateTime EndTime { get; set; }
    
    [Required]
    public string CompanyName { get; set; }
    
    [Required]
    public string JobUserId { get; set; }
    
    [Required]
    public string JobCompanyName { get; set; }
    
    [ForeignKey("JobCompanyName,JobUserId")]
    public Job Job { get; set; }
}