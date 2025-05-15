using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace FinanceTracker.Models;

[Table("WorkShifts")]
[PrimaryKey(nameof(StartTime), nameof(EndTime), nameof(UserId))]
public class WorkShift
{
    [Required]
    public DateTime StartTime { get; set; }
    
    [Required]
    public DateTime EndTime { get; set; }
    
    [Required]
    public string UserId { get; set; }
    
    [ForeignKey("UserId")]
    [JsonIgnore]
    public FinanceUser User { get; set; }
}