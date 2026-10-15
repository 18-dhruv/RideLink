public class EntityAlreadyExist : CustomException
{
    public override int StatusCode => 409;
    public EntityAlreadyExist(string message) : base(message)
    {
    }
}