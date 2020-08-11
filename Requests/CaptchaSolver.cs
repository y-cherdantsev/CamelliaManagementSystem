using System;
using System.IO;
using System.Security.Authentication;
using System.Threading;
using System.Web;
using RestSharp;

//TODO(REFACTOR)
namespace CamelliaManagementSystem.Requests
{

    /// @author Yevgeniy Cherdantsev
    /// @date 14.03.2020 11:05:15
    /// @version 1.0
    /// <summary>
    /// Service which solves captchas
    /// </summary>
    internal static class CaptchaSolver
    {

        public static string SolveCaptcha(string imagePath, string apiKey)
        {
            var base64 = GetBase64FromImage(imagePath);
            var base64Encoded = Encode(base64);
            var captchaId = GetCaptchaId(base64Encoded, apiKey);
            try
            {
                var result = GetCaptchaAnswer(captchaId, apiKey);
                return result;
            }
            catch (Exception)
            {
                return "";
            }
        }
        
        private static string GetCaptchaAnswer(string captchaId, string apiKey)
        {
            while (true)
            {
                var url = $"https://2captcha.com/res.php?key={apiKey}&action=get&id={captchaId}";
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET);
                var response = client.Execute(request);
                if (response.Content.Equals("ERROR_CAPTCHA_UNSOLVABLE"))
                    throw new AuthenticationException();
                if (!response.Content.Equals("CAPCHA_NOT_READY")) 
                    return response.Content.Split('|')[1];
                Thread.Sleep(2000);
            }
        }

        private static string GetCaptchaId(string base64, string apiKey)
        {
            try
            {
                var client = new RestClient("https://2captcha.com/in.php");
                var request = new RestRequest(Method.POST);
                request.AddParameter("undefined", $"method=base64&key={apiKey}&body={base64}",
                    ParameterType.RequestBody);
                var response = client.Execute(request);
                if (response.Content.ToUpper().Contains("ERROR_IP_NOT_ALLOWED"))
                    throw new Exception($"CAPTCHA: Invalid, viruby VPN '{response.Content}'");
                if (response.Content.ToUpper().Contains("ERROR_ZERO_BALANCE"))
                    throw new Exception($"CAPTCHA: Error zero balance");
                try
                {
                    return response.Content.Split('|')[1];
                }
                catch (Exception)
                {
                    throw new NullReferenceException($"2captcha answer: '{response.Content}'");
                }
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