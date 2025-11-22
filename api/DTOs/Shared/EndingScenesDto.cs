using System.ComponentModel.DataAnnotations;

//namespace Jam.ViewModels.Shared;
namespace Jam.DTOs;

public class EndingScenesDto
{
    public int? StoryId { get; set; } // nullable, only used in editing mode

    [Required]
    public string GoodEnding { get; set; } = string.Empty;

    [Required]
    public string NeutralEnding { get; set; } = string.Empty;

    [Required]
    public string BadEnding { get; set; } = string.Empty;

    // Flag to indicate whether user is in editing or creating mode 
    public bool IsEditMode { get; set; }
}