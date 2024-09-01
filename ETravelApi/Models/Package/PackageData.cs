using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace ETravelApi.Models.Package
{
    public class PackageData
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PackageDataId { get; set; }

        public string Description { get; set; }
        public string? ViaDestination { get; set; }
        [Required]
        public DateTime Date { get; set; }
        public int AvailableSeat {  get; set; }



        public List<PackageImage> PackageImages { get; set; } = new List<PackageImage>();

        [Required]
        [ForeignKey("PackageId")]
        public int PackageId { get; set; }
    }
}
