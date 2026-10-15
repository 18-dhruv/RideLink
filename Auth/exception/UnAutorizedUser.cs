public class UnAutorizedUser : CustomException
{
    public override int StatusCode => 401;  
    public UnAutorizedUser(string message) : base(message)
    {
    }
}