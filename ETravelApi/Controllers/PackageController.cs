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
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class PackageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PackageController(ApplicationDbContext context)
        {
            _context = context;
        }



        [Authorize(Roles = "Admin")]
        [HttpPost("add-package")]
        public async Task<IActionResult> AddPackage([FromBody] AddPackageDto addPackageDto)
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
                return Ok(new JsonResult(new { title = "Package Added", message = $"{addPackageDto.packagename} package has been added successfully" }));
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
        // Endpoint to update an existing package by its ID
        // Protect against overposting attacks by limiting input to expected fields
        [Authorize(Roles = "Admin")]
        [HttpPut("package/{id}")]
        public async Task<IActionResult> PutPackage(int id, [FromForm] Package package)
        {
            // Validate that the ID in the route matches the package ID in the payload
            if (id != package.PackageId)
            {
                return BadRequest(new { message = "Package ID does not match." });
            }

            // Retrieve the existing package from the database, including related data
            var existingPackage = await _context.Packages
                .Include(p => p.PackageData) // Include package details
                .ThenInclude(pd => pd.PackageImages) // Include associated images
                .FirstOrDefaultAsync(p => p.PackageId == id);

            // If the package doesn't exist, return a 404 error
            if (existingPackage == null)
            {
                return NotFound(new { message = "Package not found." });
            }

            // Update basic package properties
            existingPackage.PackageName = package.PackageName;
            existingPackage.Destination = package.Destination;
            existingPackage.Price = package.Price;

            // Handle the PackageData update logic
            if (package.PackageData != null)
            {
                // If the existing package doesn't have PackageData, create it
                if (existingPackage.PackageData == null)
                {
                    existingPackage.PackageData = new PackageData
                    {
                        Description = package.PackageData.Description,
                        ViaDestination = package.PackageData.ViaDestination,
                        Date = ParseDateSafely(package.PackageData.Date) ?? DateTime.UtcNow,
                        AvailableSeat = package.PackageData.AvailableSeat,
                        PackageImages = new List<PackageImage>() // Initialize empty image list
                    };
                }
                else
                {
                    // Update fields of the existing PackageData
                    existingPackage.PackageData.Description = package.PackageData.Description;
                    existingPackage.PackageData.ViaDestination = package.PackageData.ViaDestination;
                    existingPackage.PackageData.Date = ParseDateSafely(package.PackageData.Date) ?? existingPackage.PackageData.Date;
                    existingPackage.PackageData.AvailableSeat = package.PackageData.AvailableSeat;
                }

                // Manage the package images
                if (package.PackageData.PackageImages != null && package.PackageData.PackageImages.Any())
                {
                    // Get IDs of images to keep
                    var imageIdsToKeep = package.PackageData.PackageImages
                        .Where(img => img.PackageImageId > 0)
                        .Select(img => img.PackageImageId)
                        .ToList();

                    // Remove images that are no longer included in the update
                    existingPackage.PackageData.PackageImages
                        .RemoveAll(img => !imageIdsToKeep.Contains(img.PackageImageId));

                    // Process each image in the update
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
                                    // Update only metadata if no new file is uploaded
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

            // Save changes to the database and handle any errors
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


        

        [HttpGet("packages-with-details")]
        public async Task<ActionResult<IEnumerable<object>>> GetPackagesWithDetails()
        {
            var packages = await _context.Packages
                .Include(p => p.PackageData)
                .ThenInclude(pd => pd.PackageImages)
                .OrderBy(p => p.PackageId)
                .Select(p => new
                {
                    packageId = p.PackageId,
                    packageName = p.PackageName,
                    destination = p.Destination,
                    price = p.Price,
                    dateCreated = p.DateCreated,
                    packageData = p.PackageData == null ? null : new
                    {
                        description = p.PackageData.Description,
                        viaDestination = p.PackageData.ViaDestination,
                        date = p.PackageData.Date,
                        availableSeat = p.PackageData.AvailableSeat,
                        packageImages = p.PackageData.PackageImages.Select(img => new
                        {
                            packageImageId = img.PackageImageId,
                            filename = img.filename,
                            filetype = img.filetype,
                            filesize = img.filesize,
                            filebytes = img.filebytes
                        }).ToList()
                    }
                })
                .ToListAsync();

            return Ok(packages);
        }





        /// <summary>
        /// Safely parses a date from various input types.
        /// </summary>
        /// <param name="dateInput">The input object representing the date, which can be a DateTime or string.</param>
        /// <returns>
        /// A nullable DateTime:
        /// - Returns the DateTime if the input is valid.
        /// - Returns null if the input is invalid or cannot be parsed.
        /// </returns>
        private DateTime? ParseDateSafely(object dateInput)
        {
            // Check if input is already a DateTime
            if (dateInput is DateTime date)
            {
                return date; // Return the DateTime as is
            }
            // Attempt to parse if the input is a string
            else if (dateInput is string dateString && DateTime.TryParse(dateString, out var parsedDate))
            {
                return parsedDate; // Return the successfully parsed DateTime
            }

            // Return null if parsing fails or input is not a valid date representation
            return null;
        }




        /// <summary>
        /// Converts an IFormFile to a byte array.
        /// </summary>
        /// <param name="imageIFormFile">The uploaded file to be converted.</param>
        /// <returns>A byte array containing the file's data.</returns>
        public static byte[] IFormFileToBytesArray(IFormFile imageIFormFile)
        {
            // Ensure the file is not null before proceeding
            if (imageIFormFile == null)
            {
                throw new ArgumentNullException(nameof(imageIFormFile), "The file cannot be null.");
            }

            // Create a memory stream to hold the file data
            using var memoryStream = new MemoryStream();

            // Copy the file's data into the memory stream
            imageIFormFile.CopyTo(memoryStream);

            // Return the byte array from the memory stream
            return memoryStream.ToArray();
        }









        private bool PackageExists(int id)
        {
            return (_context.Packages?.Any(e => e.PackageId == id)).GetValueOrDefault();
        }

    }
}