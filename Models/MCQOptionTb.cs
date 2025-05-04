using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Minerva.Models
{
    [Table("MCQOptionTb")]
    public class MCQOptionTb
    {
        [Key]
        public int Option_id { get; set; }  // Primary Key

        [Required]
        public int Question_id { get; set; }  // Foreign Key to Question Table

        [Required]
        [MaxLength(255)]  // Option text
        public string Option_text { get; set; }

        // Navigation Property for Question (relationship with QQuestionTb table)
        [ForeignKey("Question_id")]
        [JsonIgnore]
        public QQuestionTb? Question { get; set; }
        public bool IsCorrect { get; internal set; }
    }
}
