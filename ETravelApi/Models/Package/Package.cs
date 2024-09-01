using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System;

namespace ETravelApi.Models.Package
{
    public class Package
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PackageId { get; set; }
        [Required]
        public string PackageName { get; set; }
        [Required]
        public string Destination { get; set; }
        [Required]
        public int Price { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        

        public PackageData PackageData { get; set; } = new PackageData();
    }
}
