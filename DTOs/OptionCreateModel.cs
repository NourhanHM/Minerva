// DTOs/OptionCreateModel.cs
using System.ComponentModel.DataAnnotations;

namespace Minerva.DTOs
{
    public class OptionCreateModel
    {
        [Required]
        public required string OptionText { get; set; }

        [Required]
        public bool IsCorrect { get; set; } // ?
    }
}