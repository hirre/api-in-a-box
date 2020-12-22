using System;

namespace ApiService.Exceptions
{
    [Serializable]
    internal class HttpException : Exception
    {
        internal int HttpStatusCode { get; }

        internal HttpException(string message, int httpStatusCode) : base(message)
        {
            HttpStatusCode = httpStatusCode;
        }
    }
}