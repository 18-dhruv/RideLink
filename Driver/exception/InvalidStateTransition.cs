public class InvalidStateTransition : CustomException
{
    public override int StatusCode => 409;
    public InvalidStateTransition(string message) : base(message)
    {
    }
}
