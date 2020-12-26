using System;
using ApiService.Exceptions;

namespace Exceptions
{
    public class ObjectExistsException : HttpException
    {
        public ObjectExistsException(string message = "Object already exists") : base(message, 409)
        {
        }
    }
}
