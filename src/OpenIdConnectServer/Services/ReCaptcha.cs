using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using OpenIdConnectServer.Models;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenIdConnectServer.Services
{
    public class ReCaptcha
    {
        private readonly IOptions<ReCaptchaSettings> _reCaptchaOptions;
        private readonly HttpClient _client = new HttpClient();
        private readonly ILogger<ReCaptcha> _logger;

        public ReCaptcha(IOptions<ReCaptchaSettings> reCaptchaOptions, ILoggerFactory loggerFactory)
        {
            _reCaptchaOptions = reCaptchaOptions;
            _logger = loggerFactory.CreateLogger<ReCaptcha>();
        }

        public string Key { get => _reCaptchaOptions.Value.Key; }

        public string GetUserIpAddress(HttpRequest request)
        {
            var forwardedFor = request.Headers["X-Forwarded-For"].FirstOrDefault();
            if (forwardedFor != null)
            {
                return forwardedFor
                    .Split(new char[] { ',' })
                    .FirstOrDefault();
            }
            else
            {
                return request.HttpContext.Connection.RemoteIpAddress.ToString();
            }
        }

        public async Task<ReCaptchaServerResponse> Verify(HttpRequest request, string captcha)
        {
            var recaptchaUri = _reCaptchaOptions.Value.Uri;
            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("secret", _reCaptchaOptions.Value.Secret),
                new KeyValuePair<string, string>("response", captcha),
                new KeyValuePair<string, string>("remoteip", GetUserIpAddress(request))
            });

            var response = await _client.PostAsync(recaptchaUri, content);
            var captchaResponse = JsonConvert.DeserializeObject<ReCaptchaServerResponse>(await response.Content.ReadAsStringAsync());

            _logger.LogInformation("captcha response {CAPTCHA}", captchaResponse);

            return captchaResponse;
        }
    }
}
