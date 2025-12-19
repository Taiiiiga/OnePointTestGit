using Microsoft.EntityFrameworkCore;
using System;
using TestOnePoint.Data;
using TestOnePoint.Models;

namespace TestOnePoint.Services
{
    public class UserService(TestOnePointContext context, IPasswordHasher hasher) : IUserService
    {
        public async Task<User> CreateUserAsync(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            if (string.IsNullOrWhiteSpace(user.Password)) throw new ArgumentException("Password must not be empty.", nameof(user.Password));

            // Hash the provided plain password and persist into PasswordHash (which maps to DB column "Password")
            user.PasswordHash = hasher.Hash(user.Password);
            user.Password = null; // clear plain password

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            return user;
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await context.Users.FindAsync(id);
            if (user == null)
            {
                return false;
            }
            context.Users.Remove(user);
            await context.SaveChangesAsync();
            return true;
        }

        public async Task<List<User>> GetAllUsersAsync()
            => await context.Users.ToListAsync();

        public async Task<User?> GetUserByIdAsync(int id)
            => await context.Users.FindAsync(id);

        public async Task<User?> UpdateUserAsync(int id, User user)
        {
            var existing = await context.Users.FindAsync(id);
            if (existing == null)
            {
                return null;
            }

            existing.Name = user.Name;
            existing.Email = user.Email;

            // If a new plain password was provided, hash it
            if (!string.IsNullOrWhiteSpace(user.Password))
            {
                existing.PasswordHash = hasher.Hash(user.Password);
            }

            context.Users.Update(existing);
            await context.SaveChangesAsync();

            return existing;
        }
    }
}