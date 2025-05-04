using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minerva.Models
{
    [Table("SubjectTb")]
    public class Subject
    {
        [Key]
        public int Subject_id { get; set; }
        public string Name { get; set; }
        public int Doctor_id { get; set; }
        public string Description { get; set; }
        public string? Student_ids { get; set; } // Store JSON array of IDs
                                                 // Correct spelling
                                                 //  public int Student_id { get; set; } // Foreign Key for Student
                                                 // Navigation properties
                                                 // public ICollection<Student> Students { get; set; } = new List<Student>();
       public ICollection<StudentSubject> StudentSubjects { get; set; } = new List<StudentSubject>();
    }
}
