using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minerva.Models
{
    [Table("StudentTb")]
    public class Student
    {
        [Key]
        public int Student_id { get; set; }
      //  public string Name { get; set; } // Add this property
        public string Username { get; set; }
        public string Password { get; set; }
 
        public string Email { get; set; }
        public int National_id { get; set; } // National ID
        public string Major { get; set; }

        public ICollection<StudentSubject> StudentSubjects { get; set; } = new List<StudentSubject>();
    }
}
