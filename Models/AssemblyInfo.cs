using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minerva.Models
{
    public class StudentSubject
    {
        [Key, Column(Order = 1)]
        public int Student_id { get; set; }

        [Key, Column(Order = 2)]
        public int Subject_id { get; set; }

        // Navigation properties
        public Student Student { get; set; }
        public Subject Subject { get; set; }
    }
}
