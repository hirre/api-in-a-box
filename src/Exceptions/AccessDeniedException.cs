namespace ApiInABox.Exceptions
{
    public class AccessDeniedException : HttpException
    {
        public AccessDeniedException(string message = "Access denied") : base(message, 403)
        {
        }
    }
}
