using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace ETravelApi.Models.Package
{
    public class PackageImage
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int PackageImageId { get; set; }

        public string filename { get; set; }
        public string filetype { get; set; }
        public string? filesize { get; set; }
        public byte[]? filebytes { get; set; }

        [Required]
        [ForeignKey("PackageDataId")]
        public int PackageDataId { get; set; }


        [NotMapped]
        public IFormFile? imageFile { get; set; }
        [NotMapped]
        public string? url { get; set; }
    }
}
