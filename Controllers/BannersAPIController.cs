using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.DTOS;
using WebsiteSellingBonsaiAPI.Models;

namespace WebsiteSellingBonsaiAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BannersAPIController : ControllerBase
    {
        private readonly MiniBonsaiDBAPI _context;

        public BannersAPIController(MiniBonsaiDBAPI context)
        {
            _context = context;
        }

        // GET: api/Banners
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BannerDTO>>> Getbanners()
        {
            var Banners = await _context.banners.ToListAsync();
            var bannerDTOs = Banners.Select(banner => new BannerDTO {
                BAN_ID = banner.BAN_ID,
                Title = banner.Title,
                ImageOld = banner.ImageUrl,
            }).ToList();
            return Ok(bannerDTOs);
        }

        // GET: api/Banners/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BannerDTO>> GetBanner(int id)
        {
            var banner = await _context.banners.FindAsync(id);

            if (banner == null)
            {
                return NotFound();
            }
            var bannerDTO = new BannerDTO
            {
                BAN_ID = banner.BAN_ID,
                Title = banner.Title,
                ImageOld = banner.ImageUrl,
            };
            return bannerDTO;
        }

        // PUT: api/Banners/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBanner(int id, Banner banner)
        {
            if (id != banner.BAN_ID)
            {
                return BadRequest();
            }

            _context.Entry(banner).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetBanner", new { id = banner.BAN_ID }, banner);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BannerExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

        }

        // POST: api/Banners
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Banner>> PostBanner(Banner banner)
        {
            _context.banners.Add(banner);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBanner", new { id = banner.BAN_ID }, banner);
        }

        // DELETE: api/Banners/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBanner(int id)
        {
            var banner = await _context.banners.FindAsync(id);
            if (banner == null)
            {
                return NotFound();
            }

            _context.banners.Remove(banner);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BannerExists(int id)
        {
            return _context.banners.Any(e => e.BAN_ID == id);
        }
    }
}
