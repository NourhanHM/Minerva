using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minerva.Models
{
    [Table("DoctorTb")]
    public class Doctor
    {
        [Key]
        public int Doctor_id { get; set; }
     
        public required string Name { get; set; }
        public required string Email { get; set; } // Added Email
        public required string Password { get; set; }
        public required string Department { get; set; } // Added Department
    }
}
