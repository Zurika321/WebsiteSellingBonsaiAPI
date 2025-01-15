using Azure;
using Azure.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using NuGet.Common;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Security.Policy;
using System.Text.Json;
using System.Text.Json.Serialization;
using WebsiteSellingBonsaiAPI.DTOS.Constants;
using WebsiteSellingBonsaiAPI.DTOS.Orders;
using WebsiteSellingBonsaiAPI.DTOS.User;
using WebsiteSellingBonsaiAPI.Utils;
using WebsiteSellingBonsaiAPI.Models;
using static System.Net.WebRequestMethods;
using System.Text;
using System.Data;

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

            // Thêm header 'WebsiteSellingBonsai' nếu chưa tồn tại
            if (!_httpClient.DefaultRequestHeaders.Contains("WebsiteSellingBonsai"))
            {
                _httpClient.DefaultRequestHeaders.Add("WebsiteSellingBonsai", "kjasdfh32112");
            }

            _httpContextAccessor = httpContextAccessor;
            var token = _httpContextAccessor.HttpContext?.Session.GetString("AuthToken");

            // Thêm header 'Authorization' nếu có token và header chưa tồn tại
            if (!string.IsNullOrEmpty(token) && !_httpClient.DefaultRequestHeaders.Contains("Authorization"))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        //public class token
        //{
        //    public string tokenstring { get; set; }
        //}
        private class mes
        {
            [JsonPropertyName("status")]
            public string Status { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; }
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
                //var newFileName = $"{Guid.NewGuid().toString()}{extension}"; bảo mật 1
                //var fileBytes = memoryStream.ToArray(); bảo mật siêu cấp
                //var hash = SHA256.HashData(fileBytes);
                //var newFileName = BitConverter.ToString(hash).Replace("-", "").ToLower();

                var filePath = Path.Combine(uploadFolder, newFileName);
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
                    var jsonString = await response.Content.ReadAsStringAsync();
                    List<T> data = null;

                    try
                    {
                        // Deserializing the response
                        data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(jsonString);
                    }
                    catch (JsonException jsonEx)
                    {
                        // Ghi nhận lỗi JSON và nội dung phản hồi không hợp lệ
                        return (default, new ThongBao
                        {
                            Message = $"Phản hồi không đúng định dạng JSON: {jsonEx.Message}. Nội dung phản hồi: {jsonString}",
                            MessageType = TypeThongBao.Danger,
                            DisplayTime = 5
                        });
                    }

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

                var errorMessage = $"GET LIST {apiUrl} thất bại. Status Code: {response.StatusCode}";
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    errorMessage = "Bạn không có quyền truy cập API này.";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    errorMessage = "Bạn cần đăng nhập trước.";
                }

                return (default, new ThongBao
                {
                    Message = errorMessage,
                    MessageType = TypeThongBao.Warning,
                    DisplayTime = 5
                });
            }
            catch (HttpRequestException httpEx)
            {
                // Lỗi khi kết nối đến API
                return (default, new ThongBao
                {
                    Message = $"Lỗi kết nối API: {httpEx.Message}. Kiểm tra lại đường dẫn hoặc kết nối mạng.",
                    MessageType = TypeThongBao.Danger,
                    DisplayTime = 5
                });
            }
            catch (Exception ex)
            {
                // Các lỗi khác
                return (default, new ThongBao
                {
                    Message = $"Lỗi khi thực hiện GET {apiUrl}: {ex.Message}",
                    MessageType = TypeThongBao.Danger,
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
                if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    errorMessage = "Bạn không có quyền truy cập API này.";
                }else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    errorMessage = "Bạn cần đăng nhập trước";
                }
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
                var jsonContent = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(apiUrl, jsonContent);

                string jsonString = await response.Content.ReadAsStringAsync();

                string message = null;
                if (!string.IsNullOrEmpty(jsonString))
                {
                    try
                    {
                        var apiResponse = JsonSerializer.Deserialize<mes>(jsonString);
                        message = apiResponse?.Message;
                    }
                    catch (JsonException jsonEx)
                    {
                        // Ghi nhận lỗi JSON và nội dung phản hồi không hợp lệ
                        message = $"Phản hồi không đúng định dạng JSON: {jsonEx.Message}. Nội dung phản hồi: {jsonString}";
                    }
                }

                if (response.IsSuccessStatusCode)
                {
                    return (true, new ThongBao
                    {
                        Message = message ?? "Thêm dữ liệu thành công.",
                        MessageType = TypeThongBao.Success,
                        DisplayTime = 5
                    });
                }
                else
                {
                    var errorMessage = $"POST {apiUrl} thất bại. Status Code: {response.StatusCode}";
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        errorMessage = "Bạn không có quyền truy cập API này.";
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        errorMessage = "Bạn cần đăng nhập trước.";
                    }

                    return (false, new ThongBao
                    {
                        Message = message ?? errorMessage,
                        MessageType = TypeThongBao.Warning,
                        DisplayTime = 5
                    });
                }
            }
            catch (HttpRequestException httpEx)
            {
                // Lỗi khi kết nối đến API
                return (false, new ThongBao
                {
                    Message = $"Lỗi kết nối API: {httpEx.Message}. Kiểm tra lại đường dẫn hoặc kết nối mạng.",
                    MessageType = TypeThongBao.Danger,
                    DisplayTime = 5
                });
            }
            catch (Exception ex)
            {
                // Các lỗi khác
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
                string jsonString = await response.Content.ReadAsStringAsync();

                string message = null;
                if (!string.IsNullOrEmpty(jsonString))
                {
                    try
                    {
                        var apiResponse = JsonSerializer.Deserialize<mes>(jsonString);
                        message = apiResponse?.Message;
                    }
                    catch (JsonException jsonEx)
                    {
                        // Ghi nhận lỗi JSON và nội dung phản hồi không hợp lệ
                        message = $"Phản hồi không đúng định dạng JSON: {jsonEx.Message}. Nội dung phản hồi: {jsonString}";
                    }
                }

                if (response.IsSuccessStatusCode)
                {
                    return (true, new ThongBao
                    {
                        Message = message ?? "Cập nhật dữ liệu thành công.",
                        MessageType = TypeThongBao.Success,
                        DisplayTime = 5
                    });
                }
                else
                {
                    var errorMessage = $"PUT {apiUrl} thất bại. Status Code: {response.StatusCode}";
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        errorMessage = "Bạn không có quyền truy cập API này.";
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        errorMessage = "Bạn cần đăng nhập trước.";
                    }

                    return (false, new ThongBao
                    {
                        Message = message ?? errorMessage,
                        MessageType = TypeThongBao.Warning,
                        DisplayTime = 5
                    });
                }
            }
            catch (HttpRequestException httpEx)
            {
                // Lỗi khi kết nối đến API
                return (false, new ThongBao
                {
                    Message = $"Lỗi kết nối API: {httpEx.Message}. Kiểm tra lại đường dẫn hoặc kết nối mạng.",
                    MessageType = TypeThongBao.Danger,
                    DisplayTime = 5
                });
            }
            catch (Exception ex)
            {
                // Các lỗi khác
                return (false, new ThongBao
                {
                    Message = $"Lỗi khi thực hiện PUT {apiUrl}: {ex.Message}",
                    MessageType = TypeThongBao.Danger,
                    DisplayTime = 5
                });
            }
        }


        // Xóa dữ liệu qua phương thức DELETE
        public async Task<(bool Success, ThongBao thongbao)> FetchDataApiDelete(string apiEndpoint, string image)
        {
            var Url = GetUrl();
            var apiUrl = $"{Url}{apiEndpoint}";

            try
            {
                var response = await _httpClient.DeleteAsync(apiUrl);
                string jsonString = await response.Content.ReadAsStringAsync();

                string message = null;
                if (!string.IsNullOrEmpty(jsonString))
                {
                    try
                    {
                        var apiResponse = JsonSerializer.Deserialize<mes>(jsonString);
                        message = apiResponse?.Message;
                    }
                    catch (JsonException jsonEx)
                    {
                        // Ghi nhận lỗi JSON và nội dung phản hồi không hợp lệ
                        message = $"Phản hồi không đúng định dạng JSON: {jsonEx.Message}. Nội dung phản hồi: {jsonString}";
                    }
                }

                if (response.IsSuccessStatusCode)
                {
                    if (!string.IsNullOrEmpty(image))
                    {
                        DeleteImage(image);
                    }
                    return (true, new ThongBao
                    {
                        Message = message ?? "Xóa dữ liệu thành công.",
                        MessageType = TypeThongBao.Success,
                        DisplayTime = 5
                    });
                }
                else
                {
                    var errorMessage = $"DELETE {apiUrl} thất bại. Status Code: {response.StatusCode}";
                    if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
                    {
                        errorMessage = "Bạn không có quyền truy cập API này.";
                    }
                    else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                    {
                        errorMessage = "Bạn cần đăng nhập trước.";
                    }

                    return (false, new ThongBao
                    {
                        Message = message ?? errorMessage,
                        MessageType = TypeThongBao.Warning,
                        DisplayTime = 5
                    });
                }
            }
            catch (HttpRequestException httpEx)
            {
                // Lỗi khi kết nối đến API
                return (false, new ThongBao
                {
                    Message = $"Lỗi kết nối API: {httpEx.Message}. Kiểm tra lại đường dẫn hoặc kết nối mạng.",
                    MessageType = TypeThongBao.Danger,
                    DisplayTime = 5
                });
            }
            catch (Exception ex)
            {
                // Các lỗi khác
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
                //var jsonContent = new StringContent(JsonSerializer.Serialize(register), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsJsonAsync(apiUrl, register);
                var jsonString = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                //var apiResponse = JsonSerializer.Deserialize<ResponseModel>(jsonString,options);
                var apiResponse = JsonSerializer.Deserialize<mes>(jsonString);
                var message = apiResponse?.Message;

                if (response.IsSuccessStatusCode)
                {
                    return (true, new ThongBao
                    {
                        Message = message ?? "Đăng ký thành công!",
                        MessageType = TypeThongBao.Success,
                        DisplayTime = 5
                    });
                }
                else
                {
                    return (false, new ThongBao
                    {
                        Message = message ?? $"POST {apiUrl} thất bại. Status Code: {response.StatusCode}",
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
            string apiUrl = $"{Url}Authenticate/Login";
            try
            {
                var response = await _httpClient.PostAsJsonAsync(apiUrl, login);

                var apiResponse = await response.Content.ReadFromJsonAsync<ResponseModel>();
                if (apiResponse.Status == "Error")
                {
                    return (false, new ThongBao
                    {
                        Message = apiResponse.Message,
                        MessageType = TypeThongBao.Warning,
                        DisplayTime = 5
                    }, "");
                }
                if (response.IsSuccessStatusCode)
                {
                    _httpContextAccessor.HttpContext?.Session.SetString("AuthToken", apiResponse.Message );
                    var (Success, thongbao, userInfoDTO) = await Getuserinfo(apiResponse.Message);
                    _httpContextAccessor.HttpContext?.Session.Set<ApplicationUserDTO>("userInfo", userInfoDTO);

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
        public async Task<(bool Success, ThongBao thongbao, ApplicationUserDTO userInfo)> Getuserinfo(string token)
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
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApplicationUserDTO>();

                    return (true, new ThongBao
                    {
                        Message = "Lấy thông tin thành công!",
                        MessageType = TypeThongBao.Success,
                        DisplayTime = 5
                    }, userInfo: apiResponse ?? new ApplicationUserDTO());
                }
                else
                {
                    return (false, new ThongBao
                    {
                        Message = $"GET {apiUrl} thất bại. Status Code: {response.StatusCode}",
                        MessageType = TypeThongBao.Warning,
                        DisplayTime = 5
                    }, new ApplicationUserDTO
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
                }, new ApplicationUserDTO
                {
                    UserName = $"{ex.Message}"
                });
            }
        }

        //Create payment
        public async Task<(bool Success, ThongBao thongbao)> Create_Session_Payment(Create_order create_Order)
        {
            if (string.IsNullOrEmpty(create_Order.Address)) return (false, new ThongBao
            {
                Message = "Vui lòng nhập địa chỉ để giao hàng",
                MessageType = TypeThongBao.Warning,
                DisplayTime = 5
            });
            var Url = GetUrl();
            string apiUrl = $"{Url}OrdersAPI/create_payment";
            try
            {
                var response = await _httpClient.PostAsJsonAsync(apiUrl, create_Order);

                if (response.IsSuccessStatusCode)
                {
                    var order = Newtonsoft.Json.JsonConvert.DeserializeObject<Order>(
                        await response.Content.ReadAsStringAsync()
                    );

                    if (order != null)
                    {
                        // Lưu trữ đối tượng Order vào session
                        _httpContextAccessor.HttpContext?.Session.Set<Order>("Payment_Order", order);
                        //_httpContextAccessor.HttpContext?.Session.Set("ThongBao", new ThongBao
                        //{
                        //    Message =  order.Address,
                        //    MessageType = TypeThongBao.Success,
                        //    DisplayTime = 5
                        //});
                        return (true, new ThongBao
                        {
                            Message = "Tạo session thành công",
                            MessageType = TypeThongBao.Success,
                            DisplayTime = 5
                        });
                    }
                    else
                    {
                        return (false, new ThongBao
                        {
                            Message = "Không thể chuyển đổi dữ liệu từ API.",
                            MessageType = TypeThongBao.Warning,
                            DisplayTime = 5
                        });
                    }
                }
                else
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<mes>(jsonString);
                    var message = apiResponse?.Message;
                    return (false, new ThongBao
                    {
                        Message = message ?? $"POST {apiUrl} thất bại. Mã trạng thái: {response.StatusCode}",
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
        public async Task<(bool Success, ThongBao thongbao)> changeAvatar(string avatar)
        {
            var Url = GetUrl();
            string apiUrl = $"{Url}Authenticate/changeAvatar";  // Xác định URL của API

            try
            {
                var response = await _httpClient.PostAsJsonAsync(apiUrl, avatar);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApplicationUserDTO>();
                    _httpContextAccessor.HttpContext?.Session.Set<ApplicationUserDTO>("userInfo", apiResponse ?? new ApplicationUserDTO());

                    return (true, new ThongBao
                    {
                        Message = "Thay đổi avatar thành công!",
                        MessageType = TypeThongBao.Success,
                        DisplayTime = 5
                    });
                }
                else
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<mes>(jsonString);
                    var message = apiResponse?.Message;

                    return (false, new ThongBao
                    {
                        Message = message ?? $"POST {apiUrl} thất bại. Status Code: {response.StatusCode}",
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
        public async Task<(bool Success, ThongBao thongbao)> changeInformation(ChangeInformation ci)
        {
            var Url = GetUrl();
            string apiUrl = $"{Url}Authenticate/changeInformation";

            try
            {
                var response = await _httpClient.PostAsJsonAsync(apiUrl, ci);

                if (response.IsSuccessStatusCode)
                {
                    var apiResponse = await response.Content.ReadFromJsonAsync<ApplicationUserDTO>();
                    _httpContextAccessor.HttpContext?.Session.Set<ApplicationUserDTO>("userInfo", apiResponse ?? new ApplicationUserDTO());

                    return (true, new ThongBao
                    {
                        Message = "Thay đổi thông tin thành công!",
                        MessageType = TypeThongBao.Success,
                        DisplayTime = 5
                    });
                }
                else
                {
                    var jsonString = await response.Content.ReadAsStringAsync();
                    var apiResponse = JsonSerializer.Deserialize<mes>(jsonString);
                    var message = apiResponse?.Message;

                    return (false, new ThongBao
                    {
                        Message = message ?? $"POST {apiUrl} thất bại. Status Code: {response.StatusCode}",
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
    }
}










//public async Task<(List<T>? Data, ThongBao thongbao)> FetchDataApiGetList<T>(string apiEndpoint)
//{
//    var Url = GetUrl();

//    var apiUrl = $"{Url}{apiEndpoint}";
//    try
//    {
//        // Gửi yêu cầu GET đến API
//        var response = await _httpClient.GetAsync(apiUrl);

//        if (response.IsSuccessStatusCode)
//        {
//            var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(
//                await response.Content.ReadAsStringAsync()
//            );

//            if (data == null)
//            {
//                return (default, new ThongBao
//                {
//                    Message = "Không thể deserialize dữ liệu.",
//                    MessageType = TypeThongBao.Warning,
//                    DisplayTime = 5
//                });
//            }

//            return (data, new ThongBao
//            {
//                Message = "Lấy dữ liệu thành công.",
//                MessageType = TypeThongBao.Success,
//                DisplayTime = 5
//            });
//        }

//        var errorMessage = $"GET LIST {apiUrl} thất bại. Status Code: {response.StatusCode}";
//        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
//        {
//            errorMessage = "Bạn không có quyền truy cập API này.";
//        }
//        else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
//        {
//            errorMessage = "Bạn cần đăng nhập trước";
//        }
//        return (default, new ThongBao
//        {
//            Message = errorMessage,
//            MessageType = TypeThongBao.Warning,
//            DisplayTime = 5
//        });
//    }
//    catch (Exception ex)
//    {
//        var exceptionMessage = $"Lỗi khi thực hiện GET {apiUrl}: {ex.Message}";
//        return (default, new ThongBao
//        {
//            Message = exceptionMessage,
//            MessageType = TypeThongBao.Danger,
//            DisplayTime = 5
//        });
//    }
//}


//public async Task<(bool Success, ThongBao thongbao)> FetchDataApiPost<T>(string apiEndpoint, T product)
//{
//    var Url = GetUrl();

//    var apiUrl = $"{Url}{apiEndpoint}";
//    try
//    {
//        var jsonContent = new StringContent(JsonSerializer.Serialize(product), Encoding.UTF8, "application/json");
//        var response = await _httpClient.PostAsync(apiUrl, jsonContent);
//        //var response = await _httpClient.PostAsJsonAsync(apiUrl, product);

//        var jsonString = await response.Content.ReadAsStringAsync();
//        var apiResponse = JsonSerializer.Deserialize<mes>(jsonString);
//        var message = apiResponse?.Message;

//        if (response.IsSuccessStatusCode)
//        {
//            return (true, new ThongBao
//            {
//                Message = message ?? "",
//                MessageType = TypeThongBao.Success,
//                DisplayTime = 5
//            });
//        }
//        else
//        {
//            var errorMessage = $"POST {apiUrl} thất bại. Status Code: {response.StatusCode}";
//            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
//            {
//                errorMessage = "Bạn không có quyền truy cập API này.";
//            }
//            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
//            {
//                errorMessage = "Bạn cần đăng nhập trước";
//            }
//            return (false, new ThongBao
//            {
//                Message = message ?? errorMessage,
//                MessageType = TypeThongBao.Warning,
//                DisplayTime = 5
//            });
//        }
//    }
//    catch (Exception ex)
//    {
//        return (false, new ThongBao
//        {
//            Message = $"Lỗi khi thực hiện POST {apiUrl}: {ex.Message}",
//            MessageType = TypeThongBao.Danger,
//            DisplayTime = 5
//        });
//    }
//}


//public async Task<(bool Success, ThongBao thongbao)> FetchDataApiPut<T>(string apiEndpoint, T product)
//{
//    var Url = GetUrl();

//    var apiUrl = $"{Url}{apiEndpoint}";
//    try
//    {
//        var response = await _httpClient.PutAsJsonAsync(apiUrl, product);
//        var jsonString = await response.Content.ReadAsStringAsync();
//        var apiResponse = JsonSerializer.Deserialize<mes>(jsonString);
//        var message = apiResponse?.Message;
//        if (response.IsSuccessStatusCode)
//        {
//            return (true, new ThongBao
//            {
//                Message = message ?? "",
//                MessageType = TypeThongBao.Success,
//                DisplayTime = 5
//            });
//        }
//        else
//        {
//            var errorMessage = $"PUT {apiUrl} thất bại. Status Code: {response.StatusCode}";
//            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
//            {
//                errorMessage = "Bạn không có quyền truy cập API này.";
//            }
//            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
//            {
//                errorMessage = "Bạn cần đăng nhập trước";
//            }
//            return (false, new ThongBao
//            {
//                Message = message ?? errorMessage,
//                MessageType = TypeThongBao.Warning,
//                DisplayTime = 5
//            });
//        }
//    }
//    catch (Exception ex)
//    {
//        return (false, new ThongBao
//        {
//            Message = $"Lỗi khi thực hiện PUT {apiUrl}: {ex.Message}",
//            MessageType = TypeThongBao.Danger,
//            DisplayTime = 5
//        });
//    }
//}

//public async Task<(bool Success, ThongBao thongbao)> FetchDataApiDelete(string apiEndpoint,string image)
//{

//    var Url = GetUrl();

//    var apiUrl = $"{Url}{apiEndpoint}";
//    try
//    {
//        var response = await _httpClient.DeleteAsync(apiUrl);
//        var jsonString = await response.Content.ReadAsStringAsync();
//        var apiResponse = JsonSerializer.Deserialize<mes>(jsonString);
//        var message = apiResponse?.Message;
//        if (response.IsSuccessStatusCode)
//        {
//            if(!string.IsNullOrEmpty(image))
//            {
//                DeleteImage(image);
//            }
//            return (true, new ThongBao
//            {
//                Message = message ?? "",
//                MessageType = TypeThongBao.Success,
//                DisplayTime = 5
//            });
//        }
//        else
//        {
//            var errorMessage = $"DELETE {apiUrl} thất bại. Status Code: {response.StatusCode}";
//            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
//            {
//                errorMessage = "Bạn không có quyền truy cập API này.";
//            }
//            else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
//            {
//                errorMessage = "Bạn cần đăng nhập trước";
//            }
//            return (false, new ThongBao
//            {
//                Message = message ?? errorMessage,
//                MessageType = TypeThongBao.Warning,
//                DisplayTime = 5
//            });
//        }
//    }
//    catch (Exception ex)
//    {
//        return (false, new ThongBao
//        {
//            Message = $"Lỗi khi thực hiện DELETE {apiUrl}: {ex.Message}",
//            MessageType = TypeThongBao.Danger,
//            DisplayTime = 5
//        });
//    }
//}

//public async Task<(bool Success, ThongBao thongbao)> Register(RegisterModel register)
//{
//    var Url = GetUrl();
//    string apiUrl = $"{Url}Authenticate/register";
//    try
//    {
//        var response = await _httpClient.PostAsJsonAsync(apiUrl, register);
//        var jsonString = await response.Content.ReadAsStringAsync();

//        // Kiểm tra nội dung phản hồi
//        Console.WriteLine($"API Response: {jsonString}");

//        if (!response.IsSuccessStatusCode)
//        {
//            return (false, new ThongBao
//            {
//                Message = $"POST {apiUrl} thất bại. Nội dung phản hồi: {jsonString}",
//                MessageType = TypeThongBao.Warning,
//                DisplayTime = 5
//            });
//        }

//        var options = new JsonSerializerOptions
//        {
//            PropertyNameCaseInsensitive = true
//        };
//        var apiResponse = JsonSerializer.Deserialize<ResponseModel>(jsonString, options);

//        if (apiResponse == null || string.IsNullOrEmpty(apiResponse.Message))
//        {
//            return (false, new ThongBao
//            {
//                Message = "Phản hồi từ API không hợp lệ.",
//                MessageType = TypeThongBao.Warning,
//                DisplayTime = 5
//            });
//        }

//        return (true, new ThongBao
//        {
//            Message = apiResponse.Message,
//            MessageType = TypeThongBao.Success,
//            DisplayTime = 5
//        });
//    }
//    catch (Exception ex)
//    {
//        return (false, new ThongBao
//        {
//            Message = $"Lỗi khi thực hiện POST {apiUrl}: {ex.Message}",
//            MessageType = TypeThongBao.Danger,
//            DisplayTime = 5
//        });
//    }
//}