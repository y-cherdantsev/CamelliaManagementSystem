using System;
using System.IO;
using System.Security.Authentication;
using System.Threading;
using System.Web;
using RestSharp;

namespace Camellia_Management_System.Requests
{

    /// @author Yevgeniy Cherdantsev
    /// @date 14.03.2020 11:05:15
    /// @version 1.0
    /// <summary>
    /// INPUT
    /// </summary>
    /// <code>
    /// 
    /// </code>


    public static class CaptchaSolver
    {
        private const string ApiKey = "1bcedad1e4e52767b3bda6bf7aa11461";
        public static string SolveCaptcha(string imagePath)
        {
            var base64 = Encode(GetBase64FromImage(imagePath));
            var captchaId = GetCaptchaId(base64);
            try
            {
                var result = GetCaptchaAnswer(captchaId);
                return result;
            }
            catch (Exception e)
            {
                return "";
            }
        }
        
        private static string GetCaptchaAnswer(string captchaId)
        {
            while (true)
            {
                var url = $"https://2captcha.com/res.php?key={ApiKey}&action=get&id={captchaId}";
                Console.WriteLine($"URL: {url}");
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET);
                var response = client.Execute(request);
                Console.WriteLine(">>> " + response.Content);
                if (response.Content.Equals("ERROR_CAPTCHA_UNSOLVABLE"))
                    throw new AuthenticationException();
                if (!response.Content.Equals("CAPCHA_NOT_READY")) 
                    return response.Content.Split('|')[1];
                Thread.Sleep(2000);
            }
        }

        private static string GetCaptchaId(string base64)
        {
            try
            {
                var client = new RestClient("https://2captcha.com/in.php");
                var request = new RestRequest(Method.POST);
                request.AddParameter("undefined", $"method=base64&key={ApiKey}&body={base64}",
                    ParameterType.RequestBody);
                var response = client.Execute(request);
                return response.Content.Split('|')[1];
            }
            catch (IndexOutOfRangeException)
            {
                throw new ArgumentNullException();
            }
        }

        private static string Encode(string base64)
        {
            return HttpUtility.UrlEncode(base64);
        }
        
        private static string GetBase64FromImage(string imagePath)
        {
            var imageBytes = File.ReadAllBytes(imagePath);
            var base64String = Convert.ToBase64String(imageBytes);
            return $"data:image/png;base64,{base64String}";
        }
    }
}