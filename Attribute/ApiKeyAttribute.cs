using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

namespace FGMEmailSenderApp.Attribute
{
    public class ApiKeyConfiguration
    {
        public string ApiKey { get; set; }
    }

    [AttributeUsage (AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class ApiKeyAttribute : System.Attribute, IAuthorizationFilter
    {
        private const string API_KEY_HEADER_NAME = "apikey";
        private readonly ApiKeyConfiguration _config;
        private string apiKey;

        public ApiKeyAttribute(ApiKeyConfiguration config)
        {
            _config = config;
            apiKey = config.ApiKey;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var submittedApiKey = GetSubmittedApiKey(context.HttpContext);

            if (!IsApiKeyValid(apiKey, submittedApiKey)) context.Result = new UnauthorizedResult();
        }

        private static string GetSubmittedApiKey(HttpContext context)
        {
            return context.Request.Headers[API_KEY_HEADER_NAME];
        }

        private static bool IsApiKeyValid(string apiKey, string submittedApiKey)
        {
            if(string.IsNullOrEmpty(submittedApiKey)) return false;

            var apiKeySpan = MemoryMarshal.Cast<char, byte>(apiKey.AsSpan());

            var submittedApiKeySpan = MemoryMarshal.Cast<char, byte>(submittedApiKey.AsSpan());

            return CryptographicOperations.FixedTimeEquals(apiKeySpan, submittedApiKeySpan);
        }
    }
}
