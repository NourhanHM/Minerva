using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minerva.Models
{
    [Table("StudentSubject")] // Explicitly mapping to correct table name
    public class StudentSubject
    {
        [Key, Column(Order = 1)]
        public int Student_id { get; set; }

        [Key, Column(Order = 2)]
        public int Subject_id { get; set; }

        // Navigation properties
        [ForeignKey("Student_id")]
        public Student Student { get; set; }

        [ForeignKey("Subject_id")]
        public Subject Subject { get; set; }
    }
}
