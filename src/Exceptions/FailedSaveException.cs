using System;
using ApiService.Exceptions;

namespace Exceptions
{
    public class FailedSaveException : HttpException
    {
        public FailedSaveException(string message = "Failed saving object") : base(message, 500)
        {
        }
    }
}
