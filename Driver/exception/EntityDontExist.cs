public class EntityDontExist : CustomException
{
    public override int StatusCode => 404;  
    public EntityDontExist(string message) : base(message)
    {
    }
}