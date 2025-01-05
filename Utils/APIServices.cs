using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using NuGet.Common;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using WebsiteSellingBonsaiAPI.DTOS;
using WebsiteSellingBonsaiAPI.DTOS.User;
using WebsiteSellingBonsaiAPI.Models;
using static System.Net.WebRequestMethods;
namespace WebsiteSellingBonsaiAPI.Utils
{
    public class APIServices
    {
        private readonly IWebHostEnvironment _hostEnv;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public APIServices(IWebHostEnvironment hostEnv, IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
        {
            _hostEnv = hostEnv;
            _httpClient = httpClientFactory.CreateClient();
            if (_httpClient.DefaultRequestHeaders.Contains("WebsiteSellingBonsai"))
            {
                _httpClient.DefaultRequestHeaders.Remove("WebsiteSellingBonsai");
            }
            _httpClient.DefaultRequestHeaders.Add("WebsiteSellingBonsai", "kjasdfh32112");
            _httpContextAccessor = httpContextAccessor;
            // Lấy token từ session
            //var token = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");
            //if (!string.IsNullOrEmpty(token))
            //{
            //    if (_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            //    {
            //        _httpClient.DefaultRequestHeaders.Remove("Authorization");
            //    }
            //    _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
            //}
        }

        public string GetUrl()
        {
            var request = _httpContextAccessor.HttpContext.Request;

            var scheme = request.Scheme;
            var host = request.Host.Value;

            // Xây dựng URL API động
            return $"{scheme}://{host}/api/";
            //return "https://localhost:44351/api/"


        }

        // Hàm xử lý ảnh
        public async Task<string> ProcessImage(IFormFile imageFile, string imageOldPath, string Link)
        {
            if (imageFile != null)
            {
                var webRootPath = _hostEnv.WebRootPath;
                var uploadFolder = Path.Combine(webRootPath, $"Data/{Link}");

                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var extension = Path.GetExtension(imageFile.FileName);
                var fileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
                var newFileName = $"{fileName}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
                var filePath = Path.Combine(uploadFolder, newFileName);

                //var newFileName = $"{Guid.NewGuid().toString()}{extension}"; bảo mật 1
                //var fileBytes = memoryStream.ToArray(); bảo mật siêu cấp
                //var hash = SHA256.HashData(fileBytes);
                //var newFileName = BitConverter.ToString(hash).Replace("-", "").ToLower();

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                if (!string.IsNullOrEmpty(imageOldPath))
                {
                    var erorrdeleteimage = DeleteImage(imageOldPath);
                }

                return $"Data/{Link}/{newFileName}";
            }

            return string.IsNullOrEmpty(imageOldPath) ? "" : imageOldPath;
        }

        public async Task<string> DeleteImage(string image)
        {
            if (string.IsNullOrEmpty(image))
            {
                return "Không có ảnh để xóa.";
            }

            var partimage = image.Split("/");
            var imagename = partimage.Last();
            var imageLink = image.Split("/")[0] + "/" + image.Split("/")[1];

            var filePath = Path.Combine(_hostEnv.WebRootPath, imageLink, imagename);

            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return "";
                }
                else
                {
                    return $"Tệp {filePath} không tồn tại.";
                }
            }
            catch (Exception ex)
            {
                return $"Lỗi khi xóa ảnh: {ex.Message}";
            }
        }

        // Lấy dữ liệu qua phương thức GET, trả về danh sách
        public async Task<(List<T>? Data, ThongBao thongbao)> FetchDataApiGetList<T>(string apiEndpoint)
        {
            var Url = GetUrl();

            var apiUrl = $"{Url}{apiEndpoint}";
            try
            {
                // Gửi yêu cầu GET đến API
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(
                        await response.Content.ReadAsStringAsync()
                    );

                    if (data == null)
                    {
                        return (default, new ThongBao
                        {
                            Message = "Không thể deserialize dữ liệu.",
                            MessageType = TypeThongBao.Warning,
                            DisplayTime = 5
                        });
                    }

                    return (data, new ThongBao
                    {
                        Message = "Lấy dữ liệu thành công.",
                        MessageType = TypeThongBao.Success,
                        DisplayTime = 5
                    });
                }

                var errorMessage = $"GET {apiUrl} thất bại. Status Code: {response.StatusCode}";
                return (default, new ThongBao
                {
                    Message = errorMessage,
                    MessageType = TypeThongBao.Warning,
                    DisplayTime = 5
                });
            }
            catch (Exception ex)
            {
                var exceptionMessage = $"Lỗi khi thực hiện GET {apiUrl}: {ex.Message}";
                return (default, new ThongBao
                {
                    Message = exceptionMessage,
                    MessageType = TypeThongBao.Success,
                    DisplayTime = 5
                });
            }
        }

        public async Task<(T? Data, ThongBao thongbao)> FetchDataApiGet<T>(string apiEndpoint)
        {
            var Url = GetUrl();

            var apiUrl = $"{Url}{apiEndpoint}";
            try
            {
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(
                        await response.Content.ReadAsStringAsync()
                    );

                    if (data == null)
                    {
                        return (default, new ThongBao
                        {
                            Message = "Lấy dữ liệu thất bại",
                            MessageType = TypeThongBao.Warning,
                            DisplayTime = 5
                        });
                    }

                    return (data, new ThongBao
                    {
                        Message = "Lấy dữ liệu thành công",
                        MessageType = TypeThongBao.Success,
                        DisplayTime = 5
                    }); ;
                }

                var errorMessage = $"GET {apiUrl} thất bại. Status Code: {response.StatusCode}";
                return (default, new ThongBao
                {
                    Message = errorMessage,
                    MessageType = TypeThongBao.Warning,
                    DisplayTime = 5
                });
            }
            catch (Exception ex)
            {
                var exceptionMessage = $"Lỗi khi thực hiện GET {apiUrl}: {ex.Message}";
                return (default, new ThongBao
                {
                    Message = exceptionMessage,
                    MessageType = TypeThongBao.Danger,
                    DisplayTime = 5
                }); ;
            }
        }

        // Gửi dữ liệu qua phương thức POST
        public async Task<(bool Success, ThongBao thongbao)> FetchDataApiPost<T>(string apiEndpoint, T product)
        {
            var Url = GetUrl();

            var apiUrl = $"{Url}{apiEndpoint}";
            try
            {
                var response = await _httpClient.PostAsJsonAsync(apiUrl, product);

                if (response.IsSuccessStatusCode)
                {
                    return (true, new ThongBao
                    {
                        Message = "Tạo thành công",
                        MessageType = TypeThongBao.Success,
                        DisplayTime = 5
                    });
                }
                else
                {
                    return (false, new ThongBao
                    {
                        Message = $"POST {apiUrl} thất bại. Status Code: {response.StatusCode}",
                        MessageType = TypeThongBao.Warning,
                        DisplayTime = 5
                    });
                }
            }
            catch (Exception ex)
            {
                return (false, new ThongBao
                {
                    Message = $"Lỗi khi thực hiện POST {apiUrl}: {ex.Message}",
                    MessageType = TypeThongBao.Danger,
                    DisplayTime = 5
                });
            }
        }

        // Cập nhật dữ liệu qua phương thức PUT
        public async Task<(bool Success, ThongBao thongbao)> FetchDataApiPut<T>(string apiEndpoint, T product)
        {
            var Url = GetUrl();

            var apiUrl = $"{Url}{apiEndpoint}";
            try
            {
                

                var response = await _httpClient.PutAsJsonAsync(apiUrl, product);

                if (response.IsSuccessStatusCode)
                {
                    return (true, new ThongBao
                    {
                        Message = "Cập nhật thành công",
                        MessageType = TypeThongBao.Success,
                        DisplayTime = 5
                    });
                }
                else
                {
                    return (false, new ThongBao
                    {
                        Message = $"PUT {apiUrl} thất bại. Status Code: {response.StatusCode}",
                        MessageType = TypeThongBao.Warning,
                        DisplayTime = 5
                    });
                }
            }
            catch (Exception ex)
            {
                return (false, new ThongBao
                {
                    Message = $"Lỗi khi thực hiện PUT {apiUrl}: {ex.Message}",
                    MessageType = TypeThongBao.Danger,
                    DisplayTime = 5
                });
            }
        }

        // Xóa dữ liệu qua phương thức DELETE
        public async Task<(bool Success, ThongBao thongbao)> FetchDataApiDelete(string apiEndpoint,string image)
        {
            if(!string.IsNullOrEmpty(image))
            {
                DeleteImage(image);
            }
            var Url = GetUrl();

            var apiUrl = $"{Url}{apiEndpoint}";
            try
            {
                var response = await _httpClient.DeleteAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    return (true, new ThongBao
                    {
                        Message = "Xóa thành công",
                        MessageType = TypeThongBao.Success,
                        DisplayTime = 5
                    });
                }
                else
                {
                    return (false, new ThongBao
                    {
                        Message = $"DELETE {apiUrl} thất bại. Status Code: {response.StatusCode}",
                        MessageType = TypeThongBao.Warning,
                        DisplayTime = 5
                    });
                }
            }
            catch (Exception ex)
            {
                return (false, new ThongBao
                {
                    Message = $"Lỗi khi thực hiện DELETE {apiUrl}: {ex.Message}",
                    MessageType = TypeThongBao.Danger,
                    DisplayTime = 5
                });
            }
        }

        // đăng ký 
        public async Task<(bool Success, ThongBao thongbao)> Register(RegisterModel register)
        {
            var Url = GetUrl();
            string apiUrl = $"{Url}Authenticate/register";
            try
            {
                var response = await _httpClient.PostAsJsonAsync(apiUrl, register);

                if (response.IsSuccessStatusCode)
                {
                    return (true, new ThongBao
                    {
                        Message = "Đăng ký thành công!",
                        MessageType = TypeThongBao.Success,
                        DisplayTime = 5
                    });
                }
                else
                {
                    return (false, new ThongBao
                    {
                        Message = $"POST {apiUrl} thất bại. Status Code: {response.StatusCode}",
                        MessageType = TypeThongBao.Warning,
                        DisplayTime = 5
                    });
                }
            }
            catch (Exception ex)
            {
                return (false, new ThongBao
                {
                    Message = $"Lỗi khi thực hiện POST {apiUrl}: {ex.Message}",
                    MessageType = TypeThongBao.Danger,
                    DisplayTime = 5
                });
            }
        }
        // Đăng nhập
        public async Task<(bool Success, ThongBao thongbao, string token)> Login(LoginModel login)
        {
            var Url = GetUrl();
            string apiUrl = $"{Url}Authenticate/login";
            try
            {
                var response = await _httpClient.PostAsJsonAsync(apiUrl, login);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ResponseModel>();
                    _httpContextAccessor.HttpContext?.Session.Set("AuthToken", apiResponse.Message);
                    var (Success, thongbao, userInfo) = await Getuserinfo(apiResponse.Message);
                    _httpContextAccessor.HttpContext?.Session.Set<ApplicationUser>("userInfo", userInfo);

                    return (true, new ThongBao
                    {
                        Message = "Đăng nhập thành công!",
                        MessageType = TypeThongBao.Success,
                        DisplayTime = 5
                    }, apiResponse.Message);
                }
                else
                {
                    return (false, new ThongBao
                    {
                        Message = $"POST {apiUrl} thất bại. Status Code: {response.StatusCode}",
                        MessageType = TypeThongBao.Warning,
                        DisplayTime = 5
                    }, "");
                }
            }
            catch (Exception ex)
            {
                return (false, new ThongBao
                {
                    Message = $"Lỗi khi thực hiện POST {apiUrl}: {ex.Message}",
                    MessageType = TypeThongBao.Danger,
                    DisplayTime = 5
                }, "");
            }
        }
        // userInfo
        public async Task<(bool Success, ThongBao thongbao, ApplicationUser userInfo)> Getuserinfo(string token)
        {
            var Url = GetUrl();
            string apiUrl = $"{Url}Authenticate/userinfo";

            try
            {
                // Thêm token vào header
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                // Gửi yêu cầu GET
                var response = await _httpClient.GetAsync(apiUrl);

                if (response.IsSuccessStatusCode)
                {
                    // Đọc thông tin người dùng từ phản hồi
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApplicationUser>();

                    return (true, new ThongBao
                    {
                        Message = "Lấy thông tin thành công!",
                        MessageType = TypeThongBao.Success,
                        DisplayTime = 5
                    }, userInfo: apiResponse ?? new ApplicationUser());
                }
                else
                {
                    return (false, new ThongBao
                    {
                        Message = $"GET {apiUrl} thất bại. Status Code: {response.StatusCode}",
                        MessageType = TypeThongBao.Warning,
                        DisplayTime = 5
                    }, new ApplicationUser
                    {
                        UserName = $"{response.StatusCode}",
                    });
                }
            }
            catch (Exception ex)
            {
                return (false, new ThongBao
                {
                    Message = $"Lỗi khi thực hiện GET {apiUrl}: {ex.Message}",
                    MessageType = TypeThongBao.Danger,
                    DisplayTime = 5
                }, new ApplicationUser
                {
                    UserName = $"{ex.Message}"
                });
            }
        }
    }
}
