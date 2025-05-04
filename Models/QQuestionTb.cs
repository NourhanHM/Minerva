using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Minerva.Models
{
    [Table("QQuestionTb")]
    public class QQuestionTb
    {
        [Key]
        public int Question_id { get; set; }  // Primary Key

        [Required]
        public int Quiz_id { get; set; }  // Foreign Key to Quiz Table

        [Required]
        [MaxLength(20)]  // Question type (e.g., MCQ, TrueFalse, Essay)
        public string Question_type { get; set; }

        [Required]
        public string Question_text { get; set; }  // Text of the question

        [Required]
        public int Question_grade { get; set; }  // Grade assigned for this question

        [Required]
        public string Model_answer { get; set; }  // Model answer for the question

        // Navigation Property for MCQ Options
        [JsonIgnore]
        public ICollection<MCQOptionTb> Options { get; set; } = new List<MCQOptionTb>();

        // Navigation Property for Quiz (relationship with Quiz table)
        [ForeignKey("Quiz_id")]
        [JsonIgnore]
        public Quiz? Quiz { get; set; }
    }
}

