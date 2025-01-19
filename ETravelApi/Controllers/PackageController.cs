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

            // Update or create PackageData
            if (package.PackageData != null)
            {
                if (existingPackage.PackageData == null)
                {
                    existingPackage.PackageData = new PackageData
                    {
                        Description = package.PackageData.Description,
                        ViaDestination = package.PackageData.ViaDestination,
                        Date = ParseDateSafely(package.PackageData.Date) ?? DateTime.UtcNow,
                        AvailableSeat = package.PackageData.AvailableSeat,
                        PackageImages = new List<PackageImage>()
                    };
                }
                else
                {
                    existingPackage.PackageData.Description = package.PackageData.Description;
                    existingPackage.PackageData.ViaDestination = package.PackageData.ViaDestination;
                    existingPackage.PackageData.Date = ParseDateSafely(package.PackageData.Date) ?? existingPackage.PackageData.Date;
                    existingPackage.PackageData.AvailableSeat = package.PackageData.AvailableSeat;
                }

                // Handle package images
                if (package.PackageData.PackageImages != null && package.PackageData.PackageImages.Any())
                {
                    var imageIdsToKeep = package.PackageData.PackageImages
                        .Where(img => img.PackageImageId > 0)
                        .Select(img => img.PackageImageId)
                        .ToList();

                    // Remove images not in the updated list
                    existingPackage.PackageData.PackageImages
                        .RemoveAll(img => !imageIdsToKeep.Contains(img.PackageImageId));

                    foreach (var newImage in package.PackageData.PackageImages)
                    {
                        if (newImage.PackageImageId > 0) // Existing image
                        {
                            var existingImage = existingPackage.PackageData.PackageImages
                                .FirstOrDefault(img => img.PackageImageId == newImage.PackageImageId);

                            if (existingImage != null)
                            {
                                if (newImage.imageFile != null) // Replace with new file
                                {
                                    var imageBytes = IFormFileToBytesArray(newImage.imageFile);
                                    existingImage.filename = newImage.imageFile.FileName;
                                    existingImage.filetype = newImage.imageFile.ContentType;
                                    existingImage.filesize = ((float)newImage.imageFile.Length / 1024).ToString("F2");
                                    existingImage.filebytes = imageBytes;
                                }
                                else
                                {
                                    // Update metadata only if no new file is uploaded
                                    existingImage.filename = newImage.filename;
                                    existingImage.filetype = newImage.filetype;
                                    existingImage.filesize = newImage.filesize;
                                    existingImage.filebytes ??= newImage.filebytes;
                                }
                            }
                        }
                        else if (newImage.imageFile != null) // New image to be added
                        {
                            var imageBytes = IFormFileToBytesArray(newImage.imageFile);

                            existingPackage.PackageData.PackageImages.Add(new PackageImage
                            {
                                filename = newImage.imageFile.FileName,
                                filetype = newImage.imageFile.ContentType,
                                filesize = ((float)newImage.imageFile.Length / 1024).ToString("F2"),
                                filebytes = imageBytes,
                                PackageDataId = existingPackage.PackageData.PackageDataId
                            });
                        }
                    }
                }
                else
                {
                    // If no images are sent in the update, remove all existing images
                    existingPackage.PackageData.PackageImages.Clear();
                }
            }

            try
            {
                // Save changes to the database
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