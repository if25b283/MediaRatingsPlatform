using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace MRP.Business
{
    internal class AuthService
    {
        private static readonly ConcurrentDictionary<string,
            (int UserId, DateTime Expiration)> tokens = new();

        public string GenerateToken(int userId)
        {
            string token = Convert.ToHexString(
                RandomNumberGenerator.GetBytes(32));

            tokens[token] = (
                userId,
                DateTime.UtcNow.AddHours(2)
            );

            return token;
        }

        public int? ValidateToken(string? authorizationHeader)
        {
            if (string.IsNullOrWhiteSpace(authorizationHeader) ||
                !authorizationHeader.StartsWith(
                    "Bearer ", StringComparison.OrdinalIgnoreCase))
                return null;

            string token = authorizationHeader.Substring(7).Trim();

            if (tokens.TryGetValue(token, out var data))
            {
                if (data.Expiration > DateTime.UtcNow)
                    return data.UserId;

                tokens.TryRemove(token, out _);
            }

            return null;
        }
    }
}
