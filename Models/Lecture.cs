using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Minerva.Models
{
    [Table("LectureTb")]
    public class Lecture
    {
        [Key]
        public int Lec_id { get; set; }
        public DateTime Upload_Date { get; set; }
        public int Subject_id { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public int Doctor_id { get; set; }
        public byte[] FilePath { get; set; }
    }
}
