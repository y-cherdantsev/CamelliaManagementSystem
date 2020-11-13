using System;
using RestSharp;
using System.IO;
using System.Web;
using System.Threading;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo

namespace CamelliaManagementSystem.Requests
{
    /// @author Yevgeniy Cherdantsev
    /// @date 14.03.2020 11:05:15
    /// <summary>
    /// Service which solves captchas
    /// </summary>
    internal static class CaptchaSolver
    {
        /// <summary>
        /// Solves captcha
        /// </summary>
        /// <param name="imagePath">Path to the image file</param>
        /// <param name="apiKey">Key for 2captcha API</param>
        /// <returns>Solved captcha string</returns>
        // ReSharper disable once UnusedMember.Global
        public static string SolveCaptcha(string imagePath, string apiKey)
        {
            var base64 = GetBase64FromImage(imagePath);
            var base64Encoded = HttpUtility.UrlEncode(base64);
            var captchaId = GetCaptchaId(base64Encoded, apiKey);
            var result = GetCaptchaAnswer(captchaId, apiKey);
            return result;
        }

        /// <summary>
        /// Solves captcha
        /// </summary>
        /// <param name="imageStream">Stream of the image</param>
        /// <param name="apiKey">Key for 2captcha API</param>
        /// <returns>Solved captcha string</returns>
        public static string SolveCaptcha(Stream imageStream, string apiKey)
        {
            var base64 = GetBase64FromImage(imageStream);
            var base64Encoded = HttpUtility.UrlEncode(base64);
            var captchaId = GetCaptchaId(base64Encoded, apiKey);
            var result = GetCaptchaAnswer(captchaId, apiKey);
            return result;
        }

        /// <summary>
        /// Asks for answer from API
        /// </summary>
        /// <param name="captchaId">ID of request</param>
        /// <param name="apiKey">Key for 2captcha API</param>
        /// <returns>Solved captcha</returns>
        /// <exception cref="CamelliaCaptchaSolverException">Occured when API ERROR appears</exception>
        private static string GetCaptchaAnswer(string captchaId, string apiKey)
        {
            while (true)
            {
                var url = $"https://2captcha.com/res.php?key={apiKey}&action=get&id={captchaId}";
                var client = new RestClient(url);
                var request = new RestRequest(Method.GET);
                var response = client.Execute(request);
                if (!response.Content.Equals("CAPCHA_NOT_READY"))
                    try
                    {
                        return response.Content.Split('|')[1];
                    }
                    catch (Exception)
                    {
                        throw new CamelliaCaptchaSolverException($"{response.Content}");
                    }

                Thread.Sleep(2000);
            }
        }

        /// <summary>
        /// Sends image and asks request ID from API
        /// </summary>
        /// <param name="base64">base64 representation of captcha</param>
        /// <param name="apiKey">Key for 2captcha API</param>
        /// <returns>Request ID</returns>
        /// <exception cref="CamelliaCaptchaSolverException">Occured when API ERROR appears</exception>
        private static string GetCaptchaId(string base64, string apiKey)
        {
            var client = new RestClient("https://2captcha.com/in.php");
            var request = new RestRequest(Method.POST);
            request.AddParameter("undefined", $"method=base64&key={apiKey}&body={base64}",
                ParameterType.RequestBody);
            var response = client.Execute(request);

            try
            {
                return response.Content.Split('|')[1];
            }
            catch (Exception)
            {
                throw new CamelliaCaptchaSolverException($"2captcha answer: '{response.Content}'");
            }
        }

        /// <summary>
        /// Get base64 representation of image
        /// </summary>
        /// <param name="imagePath">Full path to the image</param>
        /// <returns>base64 representation of image</returns>
        private static string GetBase64FromImage(string imagePath)
        {
            var imageBytes = File.ReadAllBytes(imagePath);
            var base64String = Convert.ToBase64String(imageBytes);
            return $"data:image/png;base64,{base64String}";
        }

        /// <summary>
        /// Get base64 representation of image
        /// </summary>
        /// <param name="stream">Stream of the image</param>
        /// <returns>base64 representation of image</returns>
        private static string GetBase64FromImage(Stream stream)
        {
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            var imageBytes = memoryStream.ToArray();
            var base64String = Convert.ToBase64String(imageBytes);
            return $"data:image/png;base64,{base64String}";
        }
    }
}