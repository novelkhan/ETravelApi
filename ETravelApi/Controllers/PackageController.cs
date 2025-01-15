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



        [HttpPut("package/{id}")]
        public async Task<IActionResult> PutPackage(int id, [FromForm] Package package)
        {
            if (id != package.PackageId)
            {
                return BadRequest(new { message = "Package ID does not match." });
            }

            // Retrieve the existing package with its related data
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

            // Update or create PackageData if provided
            if (package.PackageData != null)
            {
                if (existingPackage.PackageData == null)
                {
                    existingPackage.PackageData = new PackageData
                    {
                        Description = package.PackageData.Description,
                        ViaDestination = package.PackageData.ViaDestination,
                        Date = package.PackageData.Date,
                        AvailableSeat = package.PackageData.AvailableSeat,
                        PackageImages = new List<PackageImage>() // Initialize images list
                    };
                }
                else
                {
                    existingPackage.PackageData.Description = package.PackageData.Description;
                    existingPackage.PackageData.ViaDestination = package.PackageData.ViaDestination;
                    existingPackage.PackageData.Date = package.PackageData.Date;
                    existingPackage.PackageData.AvailableSeat = package.PackageData.AvailableSeat;
                }

                // Update PackageImages
                if (package.PackageData.PackageImages != null)
                {
                    // Remove existing images if needed
                    _context.PackageImages.RemoveRange(existingPackage.PackageData.PackageImages);

                    // Add new images
                    foreach (var newImage in package.PackageData.PackageImages)
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






        private bool PackageExists(int id)
        {
            return (_context.Packages?.Any(e => e.PackageId == id)).GetValueOrDefault();
        }

    }
}