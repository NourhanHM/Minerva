using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace Minerva.DTOs
{
    public class QuizSubmissionModel
    {
        [Required]
        public int StudentId { get; set; }

        [Required]
        public int QuizId { get; set; }

        [Required]
        public List<AnswerModel> Answers { get; set; } = new List<AnswerModel>();
    }

    public class AnswerModel
    {
        [Required]
        public int QuestionId { get; set; }

        public string? Answer { get; set; } // Supports True/False and Essay answers

        public int? OptionId { get; set; } // For MCQs, store the selected option ID
    }
}