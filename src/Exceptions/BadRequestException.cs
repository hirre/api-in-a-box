namespace ApiInABox.Exceptions
{
    public class BadRequestException : HttpException
    {
        public BadRequestException(string message = "Bad request") : base(message, 400)
        {
        }
    }
}
