using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Minerva.Models;

[Table("AdminTb")]
public class Admin
{
    [Key]
    public int Admin_id { get; set; }

    [Required]
    public string Name { get; set; }

    [Required]
    public string Password { get; set; }

    // Foreign key relationship
  

    // Navigation property
 
}
