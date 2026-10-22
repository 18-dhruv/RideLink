using Auth.model;

public interface IUserRepo:IRepo<int,User>
{
    Task<User> GetByEmail(string email);
    Task Update(User user);
}