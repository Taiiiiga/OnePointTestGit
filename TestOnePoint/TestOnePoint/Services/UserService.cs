using Microsoft.EntityFrameworkCore;
using System;
using TestOnePoint.Data;
using TestOnePoint.Models;

namespace TestOnePoint.Services
{
    public class UserService(TestOnePointContext context) : IUserService
    {
        public async Task<User> CreateUserAsync(User user)
        {
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
        {
            return await context.Users.FindAsync(id);
        }

        public async Task<  User?> UpdateUserAsync(int id, User user)
        {   
            var existing = await context.Users.FindAsync(id);
            if (existing == null)
            {
                return null;
            }

            // Update fields
            existing.Name = user.Name;
            existing.Email = user.Email;
            existing.Password = user.Password;

            context.Users.Update(existing);
            await context.SaveChangesAsync();

            return existing;
        }
    }
}
