using Patient_mgt.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Patient_mgt.Application
{
    public interface IUser
    {
        Task<IEnumerable<User>> GetAllUsers();
        Task<User> GetUserById(Guid id);
        Task<User> AddUser(User user);
        Task<User> UpdateUser(Guid id, User user);
        Task<bool> DeleteUser(Guid id);
    }
}
