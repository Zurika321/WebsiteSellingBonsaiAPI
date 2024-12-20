using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Net.Http;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text.Json;
using WebsiteSellingBonsaiAPI.Models;
using static System.Net.WebRequestMethods;
namespace WebsiteSellingBonsaiAPI.Utils
{
    public class ProcessingServices
    {
        private readonly IWebHostEnvironment _hostEnv;
        private readonly HttpClient _httpClient;

        public ProcessingServices(IWebHostEnvironment hostEnv, IHttpClientFactory httpClientFactory)
        {
            _hostEnv = hostEnv;
            _httpClient = httpClientFactory.CreateClient();
            if (_httpClient.DefaultRequestHeaders.Contains("WebsiteSellingBonsai"))
            {
                _httpClient.DefaultRequestHeaders.Remove("WebsiteSellingBonsai");
            }
            _httpClient.DefaultRequestHeaders.Add("WebsiteSellingBonsai", "kjasdfh32112");
        }

        // Hàm xử lý ảnh
        public async Task<string> ProcessImage(IFormFile imageFile, string imageOldPath,string Link)
        {
            if (imageFile != null)
            {
                if (!string.IsNullOrEmpty(imageOldPath)) {
                    var erorrdeleteimage = DeleteImage(imageOldPath, Link);
                }
                
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
                //var fileHash = BitConverter.ToString(hash).Replace("-", "").ToLower();

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                return $"Data/{Link}/{newFileName}";
            }

            return string.IsNullOrEmpty(imageOldPath) ? "" : imageOldPath;
        }

        public async Task<string> DeleteImage( string image,string Link)
        {
            if (string.IsNullOrEmpty(image))
            {
                return "Không có ảnh để xóa.";
            }

            if (!Link.StartsWith("Data/"))
            {
                Link = "Data/" + Link;
            }

            var filePath = Path.Combine(_hostEnv.WebRootPath, Link, image);

            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                    return "";
                }
                else
                {
                    return "Tệp không tồn tại.";
                }
            }
            catch (Exception ex)
            {
                return $"Lỗi khi xóa ảnh: {ex.Message}";
            }
        }

        // Lấy dữ liệu qua phương thức GET, trả về danh sách
        public async Task<(List<T>? Data, string Error)> FetchDataApiGetList<T>(string apiEndpoint)
        {
            if (!apiEndpoint.StartsWith("https"))
            {
                apiEndpoint = "https://localhost:44351/api/" + apiEndpoint;
            }
            try
            {
                var response = await _httpClient.GetAsync(apiEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<List<T>>(
                        await response.Content.ReadAsStringAsync()
                    );

                    if (data == null)
                    {
                        return (data, "Không thể deserialize dữ liệu.");
                    }

                    return (data, "");
                }

                var errorMessage = $"GET {apiEndpoint} thất bại. Status Code: {response.StatusCode}";
                return (default, errorMessage);
            }
            catch (Exception ex)
            {
                var exceptionMessage = $"Lỗi khi thực hiện GET {apiEndpoint}: {ex.Message}";
                return (default, exceptionMessage);
            }
        }

        public async Task<(T? Data, string Error)> FetchDataApiGet<T>(string apiEndpoint)
        {
            if (!apiEndpoint.StartsWith("https"))
            {
                apiEndpoint = "https://localhost:44351/api/" + apiEndpoint;
            }
            try
            {
                var response = await _httpClient.GetAsync(apiEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    var data = Newtonsoft.Json.JsonConvert.DeserializeObject<T>(
                        await response.Content.ReadAsStringAsync()
                    );

                    if (data == null)
                    {
                        return (data, "Không thể deserialize dữ liệu.");
                    }

                    return (data, "");
                }

                var errorMessage = $"GET {apiEndpoint} thất bại. Status Code: {response.StatusCode}";
                return (default, errorMessage);
            }
            catch (Exception ex)
            {
                var exceptionMessage = $"Lỗi khi thực hiện GET {apiEndpoint}: {ex.Message}";
                return (default, exceptionMessage);
            }
        }

        // Gửi dữ liệu qua phương thức POST
        public async Task<(T? Data, string Error)> FetchDataApiPost<T>(string apiEndpoint, T product)
        {
            if (!apiEndpoint.StartsWith("https"))
            {
                apiEndpoint = "https://localhost:44351/api/" + apiEndpoint;
            }
            try
            {
                var response = await _httpClient.PostAsJsonAsync(apiEndpoint, product);

                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    return (JsonSerializer.Deserialize<T>(jsonData),"");
                }
                else
                {
                    return (default, $"POST {apiEndpoint} thất bại. Status Code: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                return (default, $"Lỗi khi thực hiện POST {apiEndpoint}: {ex.Message}");
            }
        }

        // Cập nhật dữ liệu qua phương thức PUT
        public async Task<(T? Data, string Error)> FetchDataApiPut<T>(string apiEndpoint, T product)
        {
            if (!apiEndpoint.StartsWith("https"))
            {
                apiEndpoint = "https://localhost:44351/api/" + apiEndpoint;
            }
            try
            {
                var response = await _httpClient.PutAsJsonAsync(apiEndpoint, product);

                if (response.IsSuccessStatusCode)
                {
                    var jsonData = await response.Content.ReadAsStringAsync();
                    return (JsonSerializer.Deserialize<T>(jsonData), "");
                }
                else
                {
                    return (default, $"PUT {apiEndpoint} thất bại. Status Code: {response.StatusCode}");
                }

            }
            catch (Exception ex)
            {
                return (default , $"Lỗi khi thực hiện PUT {apiEndpoint}: {ex.Message}");
            }
        }

        // Xóa dữ liệu qua phương thức DELETE
        public async Task<(bool Boll,string Error)> FetchDataApiDelete(string apiEndpoint)
        {
            if (!apiEndpoint.StartsWith("https"))
            {
                apiEndpoint = "https://localhost:44351/api/" + apiEndpoint;
            }
            try
            {
                var response = await _httpClient.DeleteAsync(apiEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    return (true, "");
                }
                else
                {
                    return (false, $"DELETE {apiEndpoint} thất bại. Status Code: {response.StatusCode}");
                }

            }
            catch (Exception ex)
            {
                return (false , $"Lỗi khi thực hiện DELETE {apiEndpoint}: {ex.Message}");
            }
        }
    }
}
