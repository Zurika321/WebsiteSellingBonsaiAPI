using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using WebsiteSellingBonsaiAPI.DTOS.User;

namespace WebsiteSellingBonsaiAPI.Utils
{
    public class UrlService : IUrlService
    {
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IUrlHelperFactory _urlHelperFactory;

        public UrlService(IActionContextAccessor actionContextAccessor, IUrlHelperFactory urlHelperFactory)
        {
            _actionContextAccessor = actionContextAccessor;
            _urlHelperFactory = urlHelperFactory;
        }

        public string GenerateUrl(
            string action = "Index",
            string controller = "Home",
            object values = null,
            string area = null,
            string scheme = null)
        {
            var actionContext = _actionContextAccessor.ActionContext;
            if (actionContext == null)
            {
                throw new InvalidOperationException("Không thể truy cập vào ActionContext.");
            }

            var urlHelper = _urlHelperFactory.GetUrlHelper(actionContext);
            var routeValues = new RouteValueDictionary(values);

            // Gán giá trị mặc định cho area nếu chưa truyền vào
            //if (!string.IsNullOrEmpty(area))
            //{
                routeValues["area"] = area;
            //}

            // Lấy scheme mặc định từ HTTP request nếu không được cung cấp
            scheme ??= actionContext.HttpContext.Request.Scheme;

            return urlHelper.Action(action, controller, routeValues, scheme);
        }
    }
}
