public class ForbiddenAction : CustomException
{
    public override int StatusCode => 403;
    public ForbiddenAction(string message) : base(message)
    {
    }
}
