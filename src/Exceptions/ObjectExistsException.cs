namespace ApiInABox.Exceptions
{
    public class ObjectExistsException : HttpException
    {
        public ObjectExistsException(string message = "Object already exists") : base(message, 409)
        {
        }
    }
}
