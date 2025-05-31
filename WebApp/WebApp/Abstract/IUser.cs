using WebApp.Models;

namespace WebApp.Abstract
{
    public interface IUser
    {
        IEnumerable<User?> GetUsers();
        User? GetUserById(int id);
        User? GetUserByEmail(string email);
        User AddUser(User user);
        bool BlockUser(int id);
        bool UnblockUser(int id);
        bool DeleteUser(int id);
        void UpdateLastSeen(int userId, DateTime last_seen);

    }
}
