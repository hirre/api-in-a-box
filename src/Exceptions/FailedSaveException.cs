namespace ApiInABox.Exceptions
{
    public class FailedSaveException : HttpException
    {
        public FailedSaveException(string message = "Failed saving object") : base(message, 500)
        {
        }
    }
}
