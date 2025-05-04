using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minerva.Models
{
    [Table("GradesTb")]
    public class Grade
    {
        [Key]
        public int GradeId { get; set; }

        [ForeignKey("Student")]
        public int StudentId { get; set; }
        public Student Student { get; set; }

        [ForeignKey("Subject")]
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }

        [ForeignKey("Assignment")]
        public int? AssignmentId { get; set; }
        public Assignment Assignment { get; set; }

        [ForeignKey("Quiz")]
        public int? QuizId { get; set; }
        public Quiz Quiz { get; set; }

        public double GradeValue { get; set; } // Renamed from "Grade"
    }
}
