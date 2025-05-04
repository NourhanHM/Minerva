using Minerva.Models;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

[Table("AssignmentTb")]
public class Assignment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]  // Add this attribute
    public int Assignment_id { get; set; }

    public byte[] ModelAnswer { get; set; }
    public byte[] AssignmentFile { get; set; }
    public int Total_grade { get; set; }
    public string Title { get; set; }
    public int Subject_id { get; set; }
   
    public DateTime Deadline { get; set; } = DateTime.UtcNow.AddDays(7);
    public int Doctor_id { get; set; }
    public virtual ICollection<Attemp> Attempts { get; set; }
    // public string FilePath { get; set; }
}
