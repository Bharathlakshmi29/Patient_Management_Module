using Microsoft.EntityFrameworkCore;
using Patient_mgt.Application;
using Patient_mgt.Data;
using Patient_mgt.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Patient_mgt.Infrastructure
{
    public class UserRepository : IUser
    {
        private readonly PatientContext _context;

        public UserRepository(PatientContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetAllUsers()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User> GetUserById(Guid id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.UserId == id) ?? throw new KeyNotFoundException();
        }

        public async Task<User> AddUser(User user)
        {
            user.UserId = Guid.NewGuid();
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> UpdateUser(Guid id, User user)
        {
            if (id != user.UserId) throw new KeyNotFoundException();
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<bool> DeleteUser(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null) throw new KeyNotFoundException();
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
