using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace WinFormsApp1
{
    public class User
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public static class AuthHelper
    {
        private static string UsersFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Users.json");

        public static List<User> LoadUsers()
        {
            if (!File.Exists(UsersFilePath))
            {
                File.WriteAllText(UsersFilePath, "[]");
                return new List<User>();
            }

            try
            {
                string json = File.ReadAllText(UsersFilePath);
                // Case-insensitive so a hand-edited Users.json (camelCase keys) still loads.
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                return JsonSerializer.Deserialize<List<User>>(json, options) ?? new List<User>();
            }
            catch
            {
                return new List<User>();
            }
        }

        public static void SaveUsers(List<User> users)
        {
            string json = JsonSerializer.Serialize(users, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(UsersFilePath, json);
        }

        public static bool Register(string username, string password)
        {
            // Validate username
            if (string.IsNullOrWhiteSpace(username) || username.Length < 2 || username.Length > 30)
                return false;

            foreach (char c in username)
            {
                if (!char.IsLetterOrDigit(c) && c != '_')
                    return false;
            }

            // Validate password
            if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
                return false;

            var users = LoadUsers();

            // Check if username exists (guard against null usernames from malformed JSON)
            if (users.Exists(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase)))
                return false;

            // Create new user
            var newUser = new User
            {
                Username = username,
                PasswordHash = HashPassword(password),
                CreatedAt = DateTime.Now
            };

            users.Add(newUser);
            SaveUsers(users);
            return true;
        }

        public static bool Login(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return false;

            var users = LoadUsers();
            var user = users.Find(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));

            if (user == null || string.IsNullOrEmpty(user.PasswordHash))
                return false;

            return VerifyPassword(password, user.PasswordHash);
        }

        public static bool UserExists(string username)
        {
            var users = LoadUsers();
            return users.Exists(u => string.Equals(u.Username, username, StringComparison.OrdinalIgnoreCase));
        }

        private static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return builder.ToString();
            }
        }

        private static bool VerifyPassword(string password, string hash)
        {
            return HashPassword(password) == hash;
        }
    }
}