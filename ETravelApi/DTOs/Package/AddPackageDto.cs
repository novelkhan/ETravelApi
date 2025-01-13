using System.ComponentModel.DataAnnotations;

namespace ETravelApi.DTOs.Package
{
    public class AddPackageDto
    {
        [Required]
        public string packagename { get; set; }
        [Required]
        public string destination { get; set; }
        [Required]
        public int price { get; set; }
    }
}