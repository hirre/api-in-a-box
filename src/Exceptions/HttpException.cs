using System;

namespace ApiService.Exceptions
{
    [Serializable]
    public class HttpException : Exception
    {
        public int HttpStatusCode { get; }

        public HttpException(string message, int httpStatusCode) : base(message)
        {
            HttpStatusCode = httpStatusCode;
        }
    }
}