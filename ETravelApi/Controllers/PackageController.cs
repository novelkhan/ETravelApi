using ETravelApi.Data;
using ETravelApi.DTOs.Admin;
using ETravelApi.DTOs.Package;
using ETravelApi.Models;
using ETravelApi.Models.Package;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using System.Linq;
using System.IO;

namespace ETravelApi.Controllers
{
    [Authorize(Roles = "Admin")]
    [Route("api/[controller]")]
    [ApiController]
    public class PackageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PackageController(ApplicationDbContext context)
        {
            _context = context;
        }




        [HttpPost("add-package")]
        public async Task<IActionResult> AddPackage([FromBody]AddPackageDto addPackageDto)
        {
            if (ModelState.IsValid)
            {
                // Map DTO to entity
                var package = new Package
                {
                    PackageName = addPackageDto.packagename,
                    Destination = addPackageDto.destination,
                    Price = addPackageDto.price
                };

                // Add package to database
                _context.Packages.Add(package);
                await _context.SaveChangesAsync();

                // Return success response
                //return Ok(new { message = "Package added successfully", packageId = package.PackageId });
                return Ok(new JsonResult(new { title = "Package Added", message = $"{addPackageDto.packagename} package has been added successfully"}));
            }

            // Return validation error response
            return BadRequest(ModelState);
        }




        [HttpGet("packages")]
        public async Task<ActionResult<IEnumerable<Package>>> GetPackages() 
        {
            if (_context.Packages == null)
            {
                return NotFound();
            }
            return await _context.Packages.ToListAsync();
        }


        [HttpGet("package/{id}")]
        public async Task<ActionResult<Package>> GetPackage(int id)
        {
            if (_context.Packages == null)
            {
                return NotFound();
            }

            var package = await _context.Packages
                .Include(d => d.PackageData)
                .ThenInclude(i => i.PackageImages)
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.PackageId == id);

            if (package == null)
            {
                return NotFound();
            }

            return Ok(package);
        }


        // PUT: api/package/package/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        //[HttpPut("package/{id}")]
        //public async Task<IActionResult> PutPackage(int id, Package package)
        //{
        //    if (id != package.PackageId)
        //    {
        //        return BadRequest();
        //    }

        //    _context.Entry(package).State = EntityState.Modified;

        //    try
        //    {
        //        await _context.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!PackageExists(id))
        //        {
        //            return NotFound();
        //        }
        //        else
        //        {
        //            throw;
        //        }
        //    }

        //    return NoContent();
        //}



        // PUT: api/package/package/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("package/{id}")]
        public async Task<IActionResult> PutPackage(int id, [FromForm] Package package)
        {
            if (id != package.PackageId)
            {
                return BadRequest(new { message = "Package ID does not match." });
            }

            // Retrieve the existing package along with related data
            var existingPackage = await _context.Packages
                .Include(p => p.PackageData)
                .ThenInclude(pd => pd.PackageImages)
                .FirstOrDefaultAsync(p => p.PackageId == id);

            if (existingPackage == null)
            {
                return NotFound(new { message = "Package not found." });
            }

            // Update basic package fields
            existingPackage.PackageName = package.PackageName;
            existingPackage.Destination = package.Destination;
            existingPackage.Price = package.Price;

            // Update PackageData if provided
            if (package.PackageData != null)
            {
                if (existingPackage.PackageData == null)
                {
                    existingPackage.PackageData = new PackageData
                    {
                        Description = package.PackageData.Description,
                        ViaDestination = package.PackageData.ViaDestination,
                        Date = (DateTime)ParseDateSafely(package.PackageData.Date),
                        AvailableSeat = package.PackageData.AvailableSeat,
                        PackageImages = new List<PackageImage>()
                    };
                }
                else
                {
                    existingPackage.PackageData.Description = package.PackageData.Description;
                    existingPackage.PackageData.ViaDestination = package.PackageData.ViaDestination;
                    existingPackage.PackageData.Date = (DateTime)ParseDateSafely(package.PackageData.Date);
                    existingPackage.PackageData.AvailableSeat = package.PackageData.AvailableSeat;
                }

                // Handle PackageImages
                if (package.PackageData.PackageImages != null && package.PackageData.PackageImages.Any())
                {
                    foreach (var newImage in package.PackageData.PackageImages)
                    {
                        var existingImage = existingPackage.PackageData.PackageImages
                            .FirstOrDefault(img => img.PackageImageId == newImage.PackageImageId);

                        if (newImage.imageFile != null) // Handle newly uploaded images
                        {
                            Console.WriteLine($"Processing new image file: {newImage.imageFile.FileName}");
                            var imageBytes = IFormFileToBytesArray(newImage.imageFile);

                            if (existingImage != null)
                            {
                                existingImage.filename = newImage.imageFile.FileName;
                                existingImage.filetype = newImage.imageFile.ContentType;
                                existingImage.filesize = ((float)newImage.imageFile.Length / 1024).ToString();
                                existingImage.filebytes = imageBytes;
                            }
                            else
                            {
                                existingPackage.PackageData.PackageImages.Add(new PackageImage
                                {
                                    filename = newImage.imageFile.FileName,
                                    filetype = newImage.imageFile.ContentType,
                                    filesize = ((float)newImage.imageFile.Length / 1024).ToString(),
                                    filebytes = imageBytes
                                });
                            }
                        }
                        else if (existingImage == null)
                        {
                            existingPackage.PackageData.PackageImages.Add(new PackageImage
                            {
                                filename = newImage.filename,
                                filetype = newImage.filetype,
                                filesize = newImage.filesize,
                                filebytes = newImage.filebytes
                            });
                        }
                    }
                }
            }

            // Debugging: Log before saving changes
            Console.WriteLine("Final state of PackageImages before saving:");
            foreach (var image in existingPackage.PackageData.PackageImages)
            {
                Console.WriteLine($"Filename: {image.filename}, FileType: {image.filetype}, Bytes: {image.filebytes?.Length ?? 0}");
            }

            try
            {
                await _context.SaveChangesAsync();
                return Ok(new { message = "Package updated successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the package.", error = ex.Message });
            }
        }





        // Helper method to parse DateTime safely
        private DateTime? ParseDateSafely(object dateInput)
        {
            if (dateInput is DateTime date)
            {
                return date; // If already DateTime, return as is
            }
            else if (dateInput is string dateString && DateTime.TryParse(dateString, out var parsedDate))
            {
                return parsedDate; // Parse string to DateTime
            }
            return null; // Return null if parsing fails
        }

        // Helper method to convert IFormFile to byte array
        public static byte[] IFormFileToBytesArray(IFormFile imageIFormFile)
        {
            using var ms = new MemoryStream();
            imageIFormFile.CopyTo(ms);
            return ms.ToArray();
        }








        private bool PackageExists(int id)
        {
            return (_context.Packages?.Any(e => e.PackageId == id)).GetValueOrDefault();
        }

    }
}