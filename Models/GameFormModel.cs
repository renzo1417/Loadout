using System.ComponentModel.DataAnnotations;
namespace MidtermExam.Models;
public sealed class GameFormModel
{
    [Required(ErrorMessage="Enter a game title.")]
    [StringLength(100, ErrorMessage="Use 100 characters or fewer.")]
    public string Title { get; set; } = "";
    [StringLength(1000)] public string CoverUrl { get; set; } = "";
    [StringLength(180)] public string Purpose { get; set; } = "";
    [Range(15,240, ErrorMessage="Use a session length between 15 and 240 minutes.")]
    public int Minutes { get; set; } = 60;
    [Range(0,100)] public int Energy { get; set; } = 50;
    [StringLength(100)] public string Primary { get; set; } = "";
    [StringLength(100)] public string Secondary { get; set; } = "";
}
