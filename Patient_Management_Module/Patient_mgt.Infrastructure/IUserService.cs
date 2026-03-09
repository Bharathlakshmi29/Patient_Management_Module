using Patient_mgt.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Patient_mgt.Infrastructure
{
    public interface IUserService
    {
        Task<IEnumerable<GetUserDTO>> GetAllUsers();
        Task<GetUserDTO> GetUserById(Guid id);
        Task<UserDTO> CreateUser(CreateUserDTO user);
        Task UpdateUser(Guid id, CreateUserDTO user);
        Task<bool> DeleteUser(Guid id);
    }
}
