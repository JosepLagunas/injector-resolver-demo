namespace Yuki.Core.Interfaces.User
{
    public interface IUser : IDataComponent
    {
        string Name { get; set; }
        string SayHello();
    }
}