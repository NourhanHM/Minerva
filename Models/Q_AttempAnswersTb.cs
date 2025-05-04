using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minerva.Models
{
    [Table("QAttemptAnswerTb")]
    public class QAttemptAnswerTb
    {
        [Key]
        public int Attempt_answer_id { get; set; }  // Primary Key for the answer entry

        [Required]
        public int Attempt_id { get; set; }  // Foreign Key to QAttemptTb

        [Required]
        public int Question_id { get; set; }  // Foreign Key to QQuestionTb

        public string? Answer_text { get; set; }  // Text-based answer (for essay questions)

        public int? Option_id { get; set; }  // Option selected in case of MCQ (foreign key to MCQOptionTb)

        [Required]
        public bool Is_correct { get; set; }  // Whether the answer is correct or not (true/false)

        [Required]
        public int Answer_grade { get; set; }  // Grade awarded for the answer

        // Navigation Property to QAttemptTb (relating to the specific attempt)
        [ForeignKey("Attempt_id")]
        public QAttempTb? Attempt { get; set; }

        // Navigation Property to QQuestionTb (relating to the specific question)
        [ForeignKey("Question_id")]
        public QQuestionTb? Question { get; set; }

        // Navigation Property to MCQOptionTb (relating to the selected option, if applicable)
        [ForeignKey("Option_id")]
        public MCQOptionTb? Option { get; set; }
    }
}
