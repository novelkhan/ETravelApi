using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ETravelApi.Models
{
    public class CustomerFile
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int CustomerFileId { get; set; }
        public string Filename { get; set; }
        public string Filetype { get; set; }
        public string Filesize { get; set; }
        public byte[] Filebytes { get; set; }

        [Required]
        [ForeignKey("CustomerDataId")]
        public int CustomerDataId { get; set; }
    }
}
