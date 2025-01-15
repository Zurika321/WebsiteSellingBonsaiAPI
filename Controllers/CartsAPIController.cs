using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Azure.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.DTOS.Carts;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.Utils;

namespace WebsiteSellingBonsaiAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CartsAPIController : ControllerBase
    {
        private readonly MiniBonsaiDBAPI _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartsAPIController(MiniBonsaiDBAPI context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        //[Authorize]
        [HttpGet("GetCart")]
        public async Task<ActionResult<Cart>> GetCart()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized(new { Message = "Người dùng chưa đăng nhập." });
            }

            var cart = await _context.Carts
                    .Include(c => c.CartDetails)
                .ThenInclude(cd => cd.Bonsai)
                .FirstOrDefaultAsync(c => c.USE_ID == userId);

            if (cart == null)
            {
                // Nếu giỏ hàng chưa tồn tại, tạo mới
                cart = new Cart
                {
                    USE_ID = userId,
                    CreatedBy = User.Identity.Name,
                    CreatedDate = DateTime.Now,
                    UpdatedBy = User.Identity.Name,
                    UpdatedDate = DateTime.Now,
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
                cart.CartDetails = new List<CartDetail>();
                return Ok(cart);

            }
            else
            {
                var cartt = new Cart
                {
                    USE_ID = cart.USE_ID,
                    CreatedBy = cart.CreatedBy,
                    CreatedDate = cart.CreatedDate,
                    UpdatedBy = cart.UpdatedBy,
                    UpdatedDate = cart.UpdatedDate,
                    CartDetails = cart.CartDetails.Select(c => new CartDetail
                    {
                        CART_D_ID = c.CART_D_ID,
                        CART_ID = c.CART_ID,
                        Bonsai = c.Bonsai,
                        BONSAI_ID = c.BONSAI_ID,
                        Quantity = c.Quantity,
                        Price = c.Price,
                    }).ToList(),
                };
                return Ok(cartt);
            }
        }

        [Authorize]
        [HttpPost("AddBonsai")]
        public async Task<IActionResult> AddBonsai([FromBody] Addcart request)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (userId == null)
                {
                    return Unauthorized(new { Message = "Người dùng chưa đăng nhập." });
                }

                var bonsai = await _context.Bonsais.FindAsync(request.bonsai_id);
                if (bonsai == null)
                {
                    return NotFound(new { Message = "Sản phẩm không tồn tại." });
                }

                var cart = await _context.Carts
                    .Include(c => c.CartDetails)
                    .FirstOrDefaultAsync(c => c.USE_ID == userId);

                if (cart == null)
                {
                    cart = new Cart
                    {
                        USE_ID = userId,
                        CreatedBy = User.Identity.Name,
                        CreatedDate = DateTime.Now,
                        UpdatedBy = User.Identity.Name,
                        UpdatedDate = DateTime.Now,
                        CartDetails = new List<CartDetail>()
                    };
                    _context.Carts.Add(cart);
                }

                var cartDetail = cart.CartDetails
                    .FirstOrDefault(cd => cd.BONSAI_ID == request.bonsai_id);

                bool themmoi = false;
                if (cartDetail == null)
                {
                    cartDetail = new CartDetail
                    {
                        CART_ID = cart.CART_ID,
                        BONSAI_ID = bonsai.Id,
                        Price = bonsai.Price,
                        Quantity = request.quantity,
                    };
                    cart.CartDetails.Add(cartDetail);
                    themmoi = true;
                }
                else
                {
                    if (cartDetail.Quantity == 10)
                    {
                        return Ok(new { Message = "Đã đạt số lượng tối đa (10 cái) trong giỏ hàng" });
                    }
                    cartDetail.Quantity += request.quantity;
                }

                await _context.SaveChangesAsync();

                string mesenger = themmoi ? "Sản phẩm đã được thêm vào giỏ hàng." : $"Sản phẩm đã có trong giỏ hàng, số lượng hiện tại {cartDetail.Quantity}";
                return Ok(new { Message = mesenger });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Đã xảy ra lỗi khi thêm sản phẩm vào giỏ hàng: {ex}" });
            }
        }

        [HttpPut("update_cart")]
        public async Task<IActionResult> update_cart(Update_cart update_cart)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "Không thể xác định người dùng." });
                }

                var cartDetail = await _context.CartDetails
                    .Include(cd => cd.Cart)
                    .FirstOrDefaultAsync(cd => cd.CART_D_ID == update_cart.CART_D_ID);

                if (cartDetail == null)
                {
                    return NotFound(new { Message = "Sản phẩm trong giỏ hàng không tồn tại." });
                }

                if (cartDetail.Cart.USE_ID != userId)
                {
                    return BadRequest(new { Message = "Không thể sửa sản phẩm của giỏ hàng khác." });
                }

                if (cartDetail.Quantity > 10 || cartDetail.Quantity < 1)
                {
                    return BadRequest(new { Message = "Không thể sửa số lượng sản phẩm thấp hơn 1 hoặc lớn hơn 10." });
                }

                cartDetail.Quantity = update_cart.quantity;

                await _context.SaveChangesAsync();

                return Ok(new { Message = "Sản phẩm đã được cập nhật số lượng." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Đã xảy ra lỗi khi xóa sản phẩm khỏi giỏ hàng: {ex.Message}" });
            }
        }

        [Authorize]
        [HttpDelete("{CART_D_ID}")]
        public async Task<IActionResult> RemoveBonsaiInCart(int CART_D_ID)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "Không thể xác định người dùng." });
                }

                var cartDetail = await _context.CartDetails
                    .Include(cd => cd.Cart)
                    .FirstOrDefaultAsync(cd => cd.CART_D_ID == CART_D_ID);

                if (cartDetail == null)
                {
                    return NotFound(new { Message = "Sản phẩm trong giỏ hàng không tồn tại." });
                }

                if (cartDetail.Cart.USE_ID != userId)
                {
                    return BadRequest(new { Message = "Không thể xóa sản phẩm của giỏ hàng khác." });
                }

                _context.CartDetails.Remove(cartDetail);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Sản phẩm đã được xóa khỏi giỏ hàng." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Đã xảy ra lỗi khi xóa sản phẩm khỏi giỏ hàng: {ex.Message}" });
            }
        }
    }
}
