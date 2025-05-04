// DTOs/QuestionCreateModel.cs
using System.ComponentModel.DataAnnotations;

namespace Minerva.DTOs
{
    public class QuestionCreateModel
    {
        [Required(ErrorMessage = "Question type is required.")]
        [StringLength(50, ErrorMessage = "Question type cannot exceed 50 characters.")]
        public string QuestionType { get; set; }

        [Required(ErrorMessage = "Question text is required")]
        public string QuestionText { get; set; }

        [Required(ErrorMessage = "Question grade is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Question grade must be a non-negative number.")]
        public int QuestionGrade { get; set; }

        [Required(ErrorMessage = "Model answer is required")]
        public string ModelAnswer { get; set; }  // Store option ID or "True"/"False"

        public List<OptionCreateModel> Options { get; set; } = new List<OptionCreateModel>();// Optional, only for MCQs
    }
}
