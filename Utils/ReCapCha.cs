public interface ICaptchaService
{
    Task<bool> VerifyCaptchaAsync(string captchaResponse);
}

public class CaptchaService : ICaptchaService
{
    private readonly string _secretKey;

    public CaptchaService(IConfiguration configuration)
    {
        _secretKey = configuration["GoogleReCAPTCHA:SecretKey"];
    }

    public async Task<bool> VerifyCaptchaAsync(string captchaResponse)
    {
        if (string.IsNullOrEmpty(captchaResponse))
        {
            return false;
        }

        using (var client = new HttpClient())
        {
            var url = $"https://www.google.com/recaptcha/api/siteverify?secret={_secretKey}&response={captchaResponse}";
            var response = await client.PostAsync(url, null);
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            var jsonString = await response.Content.ReadAsStringAsync();
            dynamic jsonData = Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString);

            return jsonData.success == true;
        }
    }
}
