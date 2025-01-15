using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using WebsiteSellingBonsaiAPI.DTOS.Constants;
using WebsiteSellingBonsaiAPI.DTOS.Orders;
using WebsiteSellingBonsaiAPI.Models;
using WebsiteSellingBonsaiAPI.Utils;

namespace WebsiteSellingBonsaiAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersAPIController : ControllerBase
    {
        private readonly MiniBonsaiDBAPI _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrdersAPIController(MiniBonsaiDBAPI context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                                                    /*Admin*/
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrders()
        {
            // Sắp xếp các đơn hàng theo UpdatedDate giảm dần và chuyển đổi kết quả thành danh sách
            var orders = await _context.Orders
                .OrderByDescending(o => o.UpdatedDate)
                .ToListAsync();

            return Ok(orders);
        }

        // Lấy đơn hàng theo năm
        [HttpGet("year/{year}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByYear(int year)
        {
            var orders = await _context.Orders
                .Where(o => o.UpdatedDate.HasValue && o.UpdatedDate.Value.Year == year)
                .ToListAsync();

            if (!orders.Any())
            {
                return NotFound(new { Message = "Không tìm thấy đơn hàng nào trong năm này." });
            }

            return Ok(orders);
        }


        // Lấy đơn hàng theo tháng trong một năm cụ thể
        [HttpGet("year/{year}/month/{month}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByMonth(int year, int month)
        {
            var orders = await _context.Orders
                .Where(o => o.UpdatedDate.HasValue && o.UpdatedDate.Value.Year == year && o.UpdatedDate.Value.Month == month)
                .ToListAsync();

            if (!orders.Any())
            {
                return NotFound(new { Message = "Không tìm thấy đơn hàng nào trong tháng này." });
            }

            return Ok(orders);
        }

        // Lấy đơn hàng theo ngày cụ thể
        [HttpGet("date/{date}")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersByDate(DateTime date)
        {
            var orders = await _context.Orders
                .Where(o => o.UpdatedDate.HasValue && o.UpdatedDate.Value.Date == date.Date)
                .ToListAsync();

            if (!orders.Any())
            {
                return NotFound(new { Message = "Không tìm thấy đơn hàng nào trong ngày này." });
            }

            return Ok(orders);
        }

        [HttpGet("current")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrdersForCurrentDateMonthYear()
        {
            DateTime currentDate = DateTime.Now;
            int currentMonth = currentDate.Month;
            int currentYear = currentDate.Year;

            // Lấy đơn hàng theo ngày hiện tại
            var ordersByDate = await _context.Orders
                .Where(o => o.UpdatedDate.HasValue && o.UpdatedDate.Value.Date == currentDate)
                .ToListAsync();

            // Lấy đơn hàng theo tháng hiện tại
            var ordersByMonth = await _context.Orders
                .Where(o => o.UpdatedDate.HasValue && o.UpdatedDate.Value.Month == currentMonth && o.UpdatedDate.Value.Year == currentYear)
                .ToListAsync();

            // Lấy đơn hàng theo năm hiện tại
            var ordersByYear = await _context.Orders
                .Where(o => o.UpdatedDate.HasValue && o.UpdatedDate.Value.Year == currentYear)
                .ToListAsync();

            // Kết hợp kết quả
            var combinedOrders = ordersByDate
                .Union(ordersByMonth)
                .Union(ordersByYear)
                .Distinct()
                .ToList();

            if (!combinedOrders.Any())
            {
                return NotFound(new { Message = "Không tìm thấy đơn hàng nào trong ngày, tháng hoặc năm hiện tại." });
            }

            return Ok(combinedOrders);
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Order>> GetOrder(int id)
        {
            var order = await _context.Orders.FindAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            return order;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
                                                                         /*User*/
        ///////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

        [HttpPost("getorderbyuserid")]
        public async Task<ActionResult<IEnumerable<Order>>> GetOrderbyUser_id()
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "Người dùng chưa đăng nhập." });
                }

                var orders = await _context.Orders
                    .Where(o => o.USE_ID == userId)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Bonsai)
                    .ToListAsync();

                if (orders == null || !orders.Any())
                {
                    return NotFound(new { Message = "Không tìm thấy đơn hàng nào." });
                }

                return Ok(orders);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        [HttpPut("update_status/{ORDER_ID}")]
        public async Task<IActionResult> UpdateStatus(int ORDER_ID, [FromBody] string status)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized(new { Message = "Không thể xác định người dùng." });
                }

                // Tìm đơn hàng với ORDER_ID
                var order = await _context.Orders.FindAsync(ORDER_ID);
                if (order == null)
                {
                    return NotFound(new { Message = "Đơn hàng không tồn tại." });
                }

                // Kiểm tra quyền sở hữu đơn hàng
                if (order.USE_ID != userId)
                {
                    return new ObjectResult(new { Message = "Bạn không có quyền cập nhật trạng thái đơn hàng này." })
                    {
                        StatusCode = StatusCodes.Status403Forbidden
                    };
                }

                // Cập nhật trạng thái đơn hàng
                order.Status = status;

                // Lưu thay đổi vào cơ sở dữ liệu
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Trạng thái đơn hàng đã được cập nhật thành công." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Đã xảy ra lỗi: {ex.Message}" });
            }
        }

        [HttpPost("create_payment")]
        public async Task<ActionResult<Order>> CreatePayment([FromBody] Create_order create_Order)
        {
            var productIds = create_Order.list_bonsai.Contains('_')
                            ? create_Order.list_bonsai.Split('_').Select(int.Parse).ToList()
                            : new List<int> { int.Parse(create_Order.list_bonsai) };

            var list_quantity = create_Order.list_quantity.Contains('_')
                              ? create_Order.list_quantity.Split('_').Select(int.Parse).ToList()
                              : new List<int> { int.Parse(create_Order.list_quantity) };

            if (productIds.Count != list_quantity.Count)
            {
                return BadRequest(new { Message = $"Danh sách sản phẩm và số lượng không bằng nhau" });
            }

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userName = User.Identity?.Name;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { Message = "Không thể xác định người dùng." });
            }

            var products = await _context.Bonsais
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync();

            var order = new Order
            {
                USE_ID = userId,
                PaymentMethod = "Thanh toán khi nhận hàng",
                Status = StatusOrder.NotConfirmed,
                CancelReason = "",
                Address = create_Order.Address,
                CreatedBy = userName,
                UpdatedBy = userName,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                OrderDetails = new List<OrderDetail>()
            };

            decimal total = 0;
            List<OrderDetail> listorderdetail = new List<OrderDetail>();
            for (int i = 0; i < productIds.Count; i++)
            {
                int productId = productIds[i];
                int quantityInCart = list_quantity[i];

                var product = products.FirstOrDefault(p => p.Id == productId);
                if (product == null)
                {
                    return BadRequest(new { Message = $"Sản phẩm với ID {productId} không tồn tại." });
                }

                if (product.Quantity < quantityInCart)
                {
                    return BadRequest(new { Message = $"Sản phẩm {product.BonsaiName} không đủ số lượng trong kho." });
                }

                var orderDetail = new OrderDetail
                {
                    BONSAI_ID = product.Id,
                    Quantity = quantityInCart,
                    Price = product.Price,
                    ORDER_ID = order.ORDER_ID,
                    Bonsai = product,
                };
                listorderdetail.Add(orderDetail);
                total += product.Price * quantityInCart;
            }
            order.OrderDetails = listorderdetail;
            order.Total = total;

            //HttpContext.Session.Set<Order>("Payment", order);
            return Ok(order);
        }

        [HttpPost("create_order")]
        public async Task<ActionResult> CreateOrder([FromBody] Order order)
        {
            if (order == null || order.OrderDetails == null || !order.OrderDetails.Any())
            {
                return BadRequest("Dữ liệu không đầy đủ.");
            }
            var ordernew = new Order
            {
                USE_ID = order.USE_ID,
                PaymentMethod = order.PaymentMethod,
                Status = order.Status,
                CancelReason = order.CancelReason,
                Address = order.Address,
                CreatedBy = order.CreatedBy,
                UpdatedBy = order.UpdatedBy,
                CreatedDate = DateTime.Now,
                UpdatedDate = DateTime.Now,
                Total = order.Total,
            };

            var listOrderDetailsNew = new List<OrderDetail>();
            foreach (var od in order.OrderDetails)
            {
                //od.ORDER_D_ID = default;
                var orderdetails = new OrderDetail
                {
                    BONSAI_ID = od.BONSAI_ID,
                    Quantity = od.Quantity,
                    Price = od.Price,
                };
                listOrderDetailsNew.Add(orderdetails);
            }

            order.OrderDetails = listOrderDetailsNew;

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    _context.Orders.Add(ordernew);
                    await _context.SaveChangesAsync();

                    foreach (var detail in order.OrderDetails)
                    {
                        detail.ORDER_ID = ordernew.ORDER_ID;
                        _context.OrderDetails.Add(detail);
                        var bonsai = await _context.Bonsais.FindAsync(detail.BONSAI_ID);
                        if (bonsai == null)
                        {
                            return NotFound(new { Message = $"Không tìm thấy Bonsai với ID: {detail.BONSAI_ID}" });
                        }

                        // Trừ số lượng của Bonsai
                        if (bonsai.Quantity < detail.Quantity)
                        {
                            return BadRequest(new { Message = $"Số lượng Bonsai không đủ {detail.Quantity} sản phẩm.{bonsai.BonsaiName} còn lại: {bonsai.Quantity} sản phẩm" });
                        }

                        var cart = await _context.Carts
                                    .Include(c => c.CartDetails)
                                    .FirstOrDefaultAsync(c => c.USE_ID == ordernew.USE_ID);

                        if (cart != null)
                        {
                            var cartDetailsToRemove = cart.CartDetails
                                .Where(cd => cd.BONSAI_ID == detail.BONSAI_ID)
                                .ToList();

                            if (cartDetailsToRemove.Any())
                            {
                                _context.CartDetails.RemoveRange(cartDetailsToRemove);
                            }
                        }
                    }
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    return Ok(new { Message = "Đơn hàng đã được tạo thành công." });
                }
                catch (Exception ex)
                {
                    //await transaction.RollbackAsync();
                    //return StatusCode(500, new { Message = $"Lỗi khi tạo đơn hàng: {ex.Message}" });
                    var innerException = ex.InnerException;
                    // Ghi log hoặc xử lý innerException để biết chi tiết lỗi
                    return StatusCode(500, new { Message = $"Lỗi khi tạo đơn hàng: {innerException?.Message}" });
                }
            }
        }

        //[HttpPost("create_order")]
        //public async Task<ActionResult> CreateOrder(Create_order create_Order)
        //{
        //    var productIds = create_Order.list_bonsai.Contains('_')
        //                    ? create_Order.list_bonsai.Split('_').Select(int.Parse).ToList()
        //                    : new List<int> { int.Parse(create_Order.list_bonsai) };

        //    var list_quantity = create_Order.list_quantity.Contains('_')
        //                      ? create_Order.list_quantity.Split('_').Select(int.Parse).ToList()
        //                      : new List<int> { int.Parse(create_Order.list_quantity) };

        //    if (productIds.Count != list_quantity.Count)
        //    {
        //        return BadRequest(new { Message = $"Danh sách sản phẩm và số lượng không bằng nhau" });
        //    }

        //    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    var userName = User.Identity?.Name;

        //    if (string.IsNullOrEmpty(userId))
        //    {
        //        return Unauthorized(new { Message = "Không thể xác định người dùng." });
        //    }

        //    var products = await _context.Bonsais
        //        .Where(p => productIds.Contains(p.Id))
        //        .ToListAsync();

        //    var order = new Order
        //    {
        //        USE_ID = userId,
        //        PaymentMethod = "Thanh toán khi nhận hàng",
        //        Status = StatusOrder.NotConfirmed,
        //        CancelReason = "",
        //        Address = create_Order.Address,
        //        CreatedBy = userName,
        //        UpdatedBy = userName,
        //        CreatedDate = DateTime.Now,
        //        UpdatedDate = DateTime.Now,
        //    };

        //    decimal total = 0;
        //    for (int i = 0; i < productIds.Count; i++)
        //    {
        //        int productId = productIds[i];
        //        int quantityInCart = list_quantity[i];

        //        var product = products.FirstOrDefault(p => p.Id == productId);
        //        if (product == null)
        //        {
        //            return BadRequest(new { Message = $"Sản phẩm với ID {productId} không tồn tại." });
        //        }

        //        if (product.Quantity < quantityInCart)
        //        {
        //            return BadRequest(new { Message = $"Sản phẩm {product.BonsaiName} không đủ số lượng trong kho." });
        //        }

        //        var orderDetail = new OrderDetail
        //        {
        //            BONSAI_ID = product.Id,
        //            Quantity = quantityInCart,
        //            Price = product.Price,
        //            ORDER_ID = order.ORDER_ID,
        //        };
        //        order.OrderDetails.Add(orderDetail);

        //        product.Quantity -= quantityInCart;

        //        total += product.Price * quantityInCart;
        //    }

        //    order.Total = total;

        //    _context.Orders.Add(order);
        //    await _context.SaveChangesAsync();

        //    return Ok(new { Message = "Đặt hàng thành công." });
        //}
    }
}
