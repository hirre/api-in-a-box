namespace ApiInABox.Exceptions
{
    public class ObjectNotExistsException : HttpException
    {
        public ObjectNotExistsException(string message = "Object doesn't exist") : base(message, 404)
        {
        }
    }
}
