public class CustomException : Exception
{
    public virtual int StatusCode => 400;       
    public CustomException(string message):base(message)
    {
        
    }
    public CustomException(string message,Exception inner):base(message,inner)
    {
        
    }
}