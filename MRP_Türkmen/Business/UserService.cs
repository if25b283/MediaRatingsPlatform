using MRP.Domain;
using MRP.Data;

namespace MRP.Business
{
    internal class UserService
    {
        private readonly UserRepository repository = new();

        public bool Register(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password))
                return false;

            User user = new User(username, password);

            if (repository.UserExists(user))
                return false;

            return repository.AddUser(user);
        }

        public bool Login(string username, string password)
        {
            string? hash = repository.GetPasswordHash(username);

            return hash != null &&
                   BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public int? GetUserId(string username)
        {
            return repository.GetUserId(username);
        }
    }
}
