using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.DTOS;
using System.Net.Http;
using WebsiteSellingBonsaiAPI.Models;

namespace WebsiteSellingBonsaiAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PhanLoaiBonsaiAPIController : ControllerBase
    {
        private readonly MiniBonsaiDBAPI _context;

        public PhanLoaiBonsaiAPIController(MiniBonsaiDBAPI context)
        {
            _context = context;
        }

        // GET: api/Bonsais
        [HttpGet]
        public async Task<ActionResult<PhanLoaiBonsaiDTO>> GetBonsais()
        {
            var types = await _context.Types
                .Select(t => new BonsaiType
                {
                    Id = t.Id,
                    Name = t.Name,
                    CreatedBy = t.CreatedBy,
                    CreatedDate = t.CreatedDate,
                    UpdatedDate = t.UpdatedDate,
                    UpdatedBy = t.UpdatedBy,
              
                })
                .ToListAsync();

            var generalMeanings = await _context.GeneralMeaning
                .Select(gm => new GeneralMeaning
                {
                    Id = gm.Id,
                    Meaning = gm.Meaning,
                    CreatedBy = gm.CreatedBy,
                    CreatedDate = gm.CreatedDate,
                    UpdatedDate = gm.UpdatedDate,
                    UpdatedBy = gm.UpdatedBy,
                })
                .ToListAsync();

            var styles = await _context.Styles
                .Select(s => new Style
                {
                    Id = s.Id,
                    Name = s.Name,
                    CreatedBy = s.CreatedBy,
                    CreatedDate = s.CreatedDate,
                    UpdatedDate = s.UpdatedDate,
                    UpdatedBy = s.UpdatedBy,
                })
                .ToListAsync();

            var phanLoaiBonsai = new PhanLoaiBonsaiDTO
            {
                Styles = styles,
                Types = types,
                GeneralMeanings = generalMeanings,

            };

            return Ok(phanLoaiBonsai);
        }

    }
}
