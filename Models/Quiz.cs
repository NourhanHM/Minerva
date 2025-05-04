using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Minerva.Models
{
    [Table("QuizTb")]
    public class Quiz
    {
        [Key]
        public int Quiz_id { get; set; }  // Primary Key for Quiz

        [Required]
        public int Subject_id { get; set; }  // Foreign key to SubjectTb

        [Required]
        [MaxLength(50)]  // Maximum length of 50 for the title
        public string Title { get; set; }  // Title of the quiz

        [Required]
        public int Doctor_id { get; set; }  // Foreign key to DoctorTb

        [Required]
        public int Final_grade { get; set; }  // Final grade for the quiz

        [Required]
        public DateTime Deadline { get; set; }  // Deadline for the quiz

        // Navigation Property for Subject (relationship with Subject table)
        [ForeignKey("Subject_id")]
        [JsonIgnore]  // Prevent circular reference when serializing
        public Subject? Subject { get; set; }

        // Navigation Property for Doctor (relationship with Doctor table)
        [ForeignKey("Doctor_id")]
        [JsonIgnore]  // Prevent circular reference when serializing
        public Doctor? Doctor { get; set; }

        // Navigation Property for Questions related to the Quiz (prevent circular reference when serializing)
        [JsonIgnore]
        public ICollection<QQuestionTb> Questions { get; set; } = new List<QQuestionTb>();
    }
}

