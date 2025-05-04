using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minerva.Models
{
    [Table("AttempTb")]

    public class Attemp
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Attemp_id { get; set; }

        [Required]
        public int Student_id { get; set; }

        [Required]
        public int Assignment_id { get; set; }

     
        public string Feedback { get; set; }

        [Required]
        public double Grade { get; set; }

        [Required]
        public byte[] Answer { get; set; }

        [Required]
        public DateTime Enrollment_date { get; set; }

        // Navigation Properties
        [ForeignKey("Student_id")]
        public virtual Student Student { get; set; }

        [ForeignKey("Assignment_id")]
        public virtual Assignment Assignment { get; set; }
    }
}
