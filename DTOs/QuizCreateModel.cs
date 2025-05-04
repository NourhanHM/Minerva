// DTOs/QuizCreateModel.cs
using System.ComponentModel.DataAnnotations;

namespace Minerva.DTOs
{
    public class QuizCreateModel
    {
        [Required(ErrorMessage = "Title is required.")]
        [StringLength(50, ErrorMessage = "Title cannot exceed 50 characters.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Doctor ID is required.")]
        public int DoctorId { get; set; } // Matches Doctor_id in QuizTb

        [Required(ErrorMessage = "Final grade is required.")]
        [Range(0, int.MaxValue, ErrorMessage = "Final grade must be a non-negative number.")]
        public int FinalGrade { get; set; }

        [Required(ErrorMessage = "Deadline is required.")]
        public DateTime Deadline { get; set; }

        [Required(ErrorMessage = "At least one question is required.")]
        public List<QuestionCreateModel> Questions { get; set; }

        [Required(ErrorMessage = "Subject ID is required.")] // ADDED SubjectId
        public int SubjectId { get; set; } // ADDED SubjectId
    }
}