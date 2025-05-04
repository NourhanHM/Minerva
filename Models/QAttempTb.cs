using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minerva.Models
{
    [Table("Q_AttempTb")]
    public class QAttempTb
    {
        [Key]
        public int Q_attemp_id { get; set; }

        [Required]
        public string Feedback { get; set; }

        [Required]
        public int Quiz_id { get; set; }

        [Required]
        public int Student_id { get; set; }

        [Required]
        public int Question_score { get; set; }

        [Required]
        public byte[] Answer { get; set; }

        [Required]
        public DateTime Enrollment_date { get; set; }

        // Navigation properties
        [ForeignKey("Quiz_id")]
        public Quiz Quiz { get; set; }

        [ForeignKey("Student_id")]
        public Student Student { get; set; }

        // Collection navigation property for answers
        public virtual ICollection<QAttemptAnswerTb> Answers { get; set; }
    }
}