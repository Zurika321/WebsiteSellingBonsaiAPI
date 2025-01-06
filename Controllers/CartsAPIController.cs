using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.DTOS;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.Utils;

namespace WebsiteSellingBonsaiAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsAPIController : ControllerBase
    {
        private readonly MiniBonsaiDBAPI _context;

        public CartsAPIController(MiniBonsaiDBAPI context)
        {
            _context = context;
        }

        // GET: api/Carts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Cart>>> GetCarts()
        {
            return await _context.Carts.ToListAsync();
        }

        // GET: api/Carts/5
        [HttpGet("{userId}")]
        public async Task<ActionResult<Cart>> GetCart(string userId)
        {
            var cart = await _context.Carts
                .Include(c => c.CART_ID)
                .FirstOrDefaultAsync(c => c.USE_ID == userId);

            if (cart == null)
            {
                return NotFound();
            }

            return cart;
        }

        [HttpPost("addbonsai")]
        public async Task<IActionResult> AddBonsai([FromBody] Addcart request)
        {
            // Lấy thông tin người dùng hiện tại
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized(new { Message = "Người dùng chưa đăng nhập." });
            }

            // Kiểm tra sản phẩm có tồn tại không
            var bonsai = await _context.Bonsais.FindAsync(request.bonsai_id);
            if (bonsai == null)
            {
                return NotFound(new { Message = "Sản phẩm không tồn tại." });
            }

            // Tìm giỏ hàng của người dùng
            var cart = await _context.Carts
                .Include(c => c.CartDetails)
                .FirstOrDefaultAsync(c => c.USE_ID == userId);

            if (cart == null)
            {
                var userInfo = HttpContext.Session.Get<ApplicationUser>("userInfo");
                cart = new Cart
                {
                    USE_ID = userId,
                    CreatedBy = userInfo.UserName,
                    CreatedDate = DateTime.Now,
                    UpdatedBy = userInfo.UserName,
                    UpdatedDate = DateTime.Now,
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();

                cart = await _context.Carts
                .Include(c => c.CartDetails)
                .FirstOrDefaultAsync(c => c.USE_ID == userId);
            }

            // Kiểm tra sản phẩm đã có trong giỏ hàng chưa
            var cartDetail = cart.CartDetails
                .FirstOrDefault(cd => cd.BONSAI_ID == request.bonsai_id);

            if (cartDetail == null)
            {
                // Thêm sản phẩm mới vào giỏ hàng
                cartDetail = new CartDetail
                {
                    CART_ID = cart.CART_ID,
                    BONSAI_ID = bonsai.Id,
                    Price = bonsai.Price,
                    Quantity = request.quantity,
                };
                cart.CartDetails.Add(cartDetail);
            }
            else
            {
                // Cập nhật số lượng sản phẩm trong giỏ hàng
                cartDetail.Quantity += request.quantity;
            }

            // Lưu thay đổi vào cơ sở dữ liệu
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Sản phẩm đã được thêm vào giỏ hàng." });
        }

        // PUT: api/Carts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCart(int id, Cart cart)
        {
            if (id != cart.CART_ID)
            {
                return BadRequest();
            }

            _context.Entry(cart).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CartExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Carts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Cart>> PostCart(Cart cart)
        {
            _context.Carts.Add(cart);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCart", new { id = cart.CART_ID }, cart);
        }

        // DELETE: api/Carts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCart(int id)
        {
            var cart = await _context.Carts.FindAsync(id);
            if (cart == null)
            {
                return NotFound();
            }

            _context.Carts.Remove(cart);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CartExists(int id)
        {
            return _context.Carts.Any(e => e.CART_ID == id);
        }
    }
}
