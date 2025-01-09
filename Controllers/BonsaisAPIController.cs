using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.DTOS;
using System.Net.Http;
using Microsoft.AspNetCore.Authorization;
using WebsiteSellingBonsaiAPI.DTOS.Constants;

namespace WebsiteSellingBonsaiAPI.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class BonsaisAPIController : ControllerBase
    {
        private readonly MiniBonsaiDBAPI _context;
        public BonsaisAPIController(MiniBonsaiDBAPI context)
        {
            _context = context;
        }

        // GET: api/Bonsais
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BonsaiDTO>>> GetBonsais()
        {
            // Lấy tất cả dữ liệu từ bảng Bonsais
            var bonsais = await _context.Bonsais.Include(b => b.Type)
                                                .Include(b => b.Style)
                                                .Include(b => b.GeneralMeaning)
                                                .ToListAsync();

            // Map dữ liệu từ Bonsai sang BonsaiDTO
            var bonsaiDTOs = bonsais.Select(bonsai => new BonsaiDTO
            {
                Id = bonsai.Id,
                BonsaiName = bonsai.BonsaiName,
                Description = bonsai.Description,
                FengShuiMeaning = bonsai.FengShuiMeaning,
                Size = bonsai.Size,
                YearOld = bonsai.YearOld,
                MinLife = bonsai.MinLife,
                MaxLife = bonsai.MaxLife,
                Price = bonsai.Price,
                Quantity = bonsai.Quantity,
                ImageOld = bonsai.Image, // Gán Image cũ từ Bonsai
                nopwr = bonsai.NOPWR,
                rates = bonsai.Rates,
                TypeId = bonsai.TypeId,
                Type = new BonsaiType
                {
                    Id = bonsai.Type.Id,
                    Name = bonsai.Type.Name,
                    CreatedDate = bonsai.Type.CreatedDate,
                    CreatedBy = bonsai.Type.CreatedBy,
                    UpdatedBy = bonsai.Type.UpdatedBy,
                    UpdatedDate = bonsai.Type.UpdatedDate,

                },
                StyleId = bonsai.StyleId,
                Style = new Style
                {
                    Id = bonsai.Style.Id,
                    Name = bonsai.Style.Name,
                    CreatedDate = bonsai.Style.CreatedDate,
                    CreatedBy = bonsai.Style.CreatedBy,
                    UpdatedBy = bonsai.Style.UpdatedBy,
                    UpdatedDate = bonsai.Style.UpdatedDate,
                },
                GeneralMeaningId = bonsai.GeneralMeaningId,
                GeneralMeaning = new GeneralMeaning
                {
                    Id = bonsai.GeneralMeaning.Id,
                    Meaning = bonsai.GeneralMeaning.Meaning,
                    CreatedDate = bonsai.GeneralMeaning.CreatedDate,
                    CreatedBy = bonsai.GeneralMeaning.CreatedBy,
                    UpdatedBy = bonsai.GeneralMeaning.UpdatedBy,
                    UpdatedDate = bonsai.GeneralMeaning.UpdatedDate,
                },
                CreatedDate = bonsai.CreatedDate,
                CreatedBy = bonsai.CreatedBy,
                UpdatedBy = bonsai.UpdatedBy,
                UpdatedDate = bonsai.UpdatedDate,
            }).ToList();

            return Ok(bonsaiDTOs); // Trả về danh sách DTO
        }

        // GET: api/Bonsais/5
        //[AllowAnonymous]
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<BonsaiDTO>> GetBonsai(int id)
        {
            // Sử dụng Include để bao gồm các bảng liên quan
            var bonsai = await _context.Bonsais
                .Include(b => b.Type)
                .Include(b => b.Style)
                .Include(b => b.GeneralMeaning)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bonsai == null)
            {
                return NotFound();
            }

            var bonsaiDTO = new BonsaiDTO
            {
                Id = bonsai.Id,
                BonsaiName = bonsai.BonsaiName,
                Description = bonsai.Description,
                FengShuiMeaning = bonsai.FengShuiMeaning,
                Size = bonsai.Size,
                YearOld = bonsai.YearOld,
                MinLife = bonsai.MinLife,
                MaxLife = bonsai.MaxLife,
                Price = bonsai.Price,
                Quantity = bonsai.Quantity,
                ImageOld = bonsai.Image,
                nopwr = bonsai.NOPWR,
                rates = bonsai.Rates,
                TypeId = bonsai.TypeId,
                Type = bonsai.Type != null ? new BonsaiType
                {
                    Id = bonsai.Type.Id,
                    Name = bonsai.Type.Name,
                    CreatedDate = bonsai.Type.CreatedDate,
                    CreatedBy = bonsai.Type.CreatedBy,
                    UpdatedBy = bonsai.Type.UpdatedBy,
                    UpdatedDate = bonsai.Type.UpdatedDate,
                } : null,
                StyleId = bonsai.StyleId,
                Style = bonsai.Style != null ? new Style
                {
                    Id = bonsai.Style.Id,
                    Name = bonsai.Style.Name,
                    CreatedDate = bonsai.Style.CreatedDate,
                    CreatedBy = bonsai.Style.CreatedBy,
                    UpdatedBy = bonsai.Style.UpdatedBy,
                    UpdatedDate = bonsai.Style.UpdatedDate,
                } : null,
                GeneralMeaningId = bonsai.GeneralMeaningId,
                GeneralMeaning = bonsai.GeneralMeaning != null ? new GeneralMeaning
                {
                    Id = bonsai.GeneralMeaning.Id,
                    Meaning = bonsai.GeneralMeaning.Meaning,
                    CreatedDate = bonsai.GeneralMeaning.CreatedDate,
                    CreatedBy = bonsai.GeneralMeaning.CreatedBy,
                    UpdatedBy = bonsai.GeneralMeaning.UpdatedBy,
                    UpdatedDate = bonsai.GeneralMeaning.UpdatedDate,
                } : null,
                CreatedDate = bonsai.CreatedDate,
                CreatedBy = bonsai.CreatedBy,
                UpdatedBy = bonsai.UpdatedBy,
                UpdatedDate = bonsai.UpdatedDate,
            };

            return bonsaiDTO;
        }


        // PUT: api/Bonsais/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Policy = UserRoles.User)]
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBonsai(int id, [FromBody] Bonsai bonsai)
        {
            if (id != bonsai.Id)
            {
                return BadRequest("ID không khớp.");
            }

            _context.Entry(bonsai).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetBonsai), new { id = bonsai.Id }, bonsai);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BonsaiExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
        }


        // POST: api/Bonsais
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [Authorize(Policy = UserRoles.User)]
        [HttpPost]
        public async Task<ActionResult<Bonsai>> PostBonsai(Bonsai bonsai)
        {
            try
            {
                // Thêm đối tượng bonsai vào cơ sở dữ liệu
                _context.Bonsais.Add(bonsai);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetBonsai), new { id = bonsai.Id }, bonsai);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return StatusCode(500, "Có lỗi xảy ra khi lưu dữ liệu.");
            }
        }


        // DELETE: api/Bonsais/5
        [Authorize(Policy = UserRoles.Admin)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBonsai(int id)
        {
            var bonsai = await _context.Bonsais.FindAsync(id);
            if (bonsai == null)
            {
                return NotFound();
            }

            _context.Bonsais.Remove(bonsai);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BonsaiExists(int id)
        {
            return _context.Bonsais.Any(e => e.Id == id);
        }
    }
}
