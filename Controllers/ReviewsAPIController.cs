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
using WebsiteSellingBonsaiAPI.DTOS.Review;

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
        public async Task<ActionResult<IEnumerable<Review>>> AddReview(AddReview addReview)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.Identity?.Name;

            if (userId == null || userName == null)
            {
                return BadRequest(new { Message = $"Cần đăng nhập trước khi đăng comment" });
            }


            if (addReview == null || addReview.bonsai_id == null)
            {
                return BadRequest(new { Message = "dữ liệu ko đủ hoặc ko đúng định dạng" });
            }

            if (addReview.rate < 1 || addReview.rate > 5)
            {
                return BadRequest(new { Message = "số sao phải nằm trong khoản 1-5" });
            }

            var bonsai = await _context.Bonsais.FirstOrDefaultAsync(b => b.Id == addReview.bonsai_id);

            if (bonsai == null)
            {
                return BadRequest(new { Message = $"Không tìm thấy sản phẩm với id {addReview.bonsai_id}" });
            }

            var hasPurchased = await _context.Orders
                .Include(o => o.OrderDetails)
                .AnyAsync(o => o.USE_ID == userId && o.OrderDetails.Any(od => od.BONSAI_ID == addReview.bonsai_id));
            if (!hasPurchased)
            {
                return BadRequest(new { Message = "Bạn cần mua sản phẩm này trước khi đánh giá." });
            }

            var review = await _context.Reviews.FirstOrDefaultAsync(r => r.BONSAI_ID == addReview.bonsai_id && r.USE_ID == userId);

            var mes = "";
            if (review == null) { 
                var newreview = new Review
                {
                    Comment = addReview.comment ?? "",
                    Rate = addReview.rate,
                    BONSAI_ID = addReview.bonsai_id,
                    USE_ID = userId,
                    CreatedBy = userName,
                    CreatedDate = DateTime.Now,
                    UpdatedBy = userName,
                    UpdatedDate = DateTime.Now,
                };

                double rate = ((bonsai.Rates ?? 0) * (bonsai.NOPWR ?? 0) + (addReview != null ? addReview.rate : 0))/ ((bonsai.NOPWR ?? 0) + 1);
                double roundedRate = Math.Round(rate, 1);
                bonsai.Rates = roundedRate;
                bonsai.NOPWR = bonsai.NOPWR + 1;

                mes = "Đăng bình luận thành công!";
                _context.Update(bonsai);
                _context.Add(newreview);
            }
            else
            {
                double rate = ((bonsai.Rates ?? 0) * (bonsai.NOPWR ?? 0) - review.Rate + (addReview != null ? addReview.rate : 0)) / (bonsai.NOPWR ?? 0);
                double roundedRate = Math.Round(rate, 1);
                bonsai.Rates = roundedRate;

                review.Comment = addReview.comment;
                review.Rate = addReview.rate;
                review.UpdatedDate = DateTime.Now;

                mes = "Chỉnh sửa bình luận thành công!";
                _context.Update(review);
            }

            await _context.SaveChangesAsync();
            return Ok(new { Message = mes });
        }
    }
}
