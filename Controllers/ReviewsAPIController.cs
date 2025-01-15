using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.Models;

namespace WebsiteSellingBonsaiAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReviewsAPIController : ControllerBase
    {
        private readonly MiniBonsaiDBAPI _context;

        public ReviewsAPIController(MiniBonsaiDBAPI context)
        {
            _context = context;
        }

        // GET: api/Reviews
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviews()
        {
            return await _context.Reviews.ToListAsync();
        }

        [HttpGet("GetReviewByBonsaiId/{id}")]
        public async Task<ActionResult<IEnumerable<Review>>> GetReviewByBonsaiId(int id)
        {
            var review = await _context.Reviews.Where(r => r.BONSAI_ID == id).ToListAsync();

            if (review == null)
            {
                return BadRequest( new { Message = "Sản phẩm chưa có comment nào" });
            }

            return review;
        }
        [HttpPost("AddReview")]
        public async Task<ActionResult<IEnumerable<Review>>> AddReview(int id)
        {
            

            return Ok(new { Message = "Ok" });
        }
    }
}
