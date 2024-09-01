using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

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
    }
}
