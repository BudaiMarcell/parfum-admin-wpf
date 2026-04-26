using System;
using System.Net;

namespace ParfumAdmin_WPF.Services
{
    /// <summary>
    /// Wraps a non-success HTTP response from the Laravel API with a
    /// user-presentable Hungarian message. Throw this from <c>ApiService</c>
    /// and <c>AuthService</c> so ViewModels can display a clean message
    /// without leaking the BaseUrl, response body, or framework stack trace.
    /// </summary>
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }
        public string UserMessage { get; }

        public ApiException(HttpStatusCode statusCode, string userMessage)
            : base(userMessage)
        {
            StatusCode = statusCode;
            UserMessage = userMessage;
        }

        public ApiException(HttpStatusCode statusCode, string userMessage, Exception inner)
            : base(userMessage, inner)
        {
            StatusCode = statusCode;
            UserMessage = userMessage;
        }
    }
}
