using AutoMapper;
using Patient_mgt.Application;
using Patient_mgt.Domain;
using Patient_mgt.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Patient_mgt.Infrastructure
{
    public class UserService : IUserService
    {
        private readonly IUser _repo;
        private readonly IMapper _mapper;

        public UserService(IUser repo, IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        public async Task<IEnumerable<GetUserDTO>> GetAllUsers()
        {
            var users = await _repo.GetAllUsers();
            return _mapper.Map<IEnumerable<GetUserDTO>>(users);
        }

        public async Task<GetUserDTO?> GetUserById(Guid id)
        {
            var user = await _repo.GetUserById(id);
            if (user == null) return null;
            return _mapper.Map<GetUserDTO>(user);
        }

        public async Task<UserDTO> CreateUser(CreateUserDTO dto)
        {
            try
            {
                // Check if email already exists
                var existingUsers = await _repo.GetAllUsers();
                if (existingUsers.Any(u => u.EmailId.ToLower() == dto.EmailId.ToLower()))
                {
                    throw new InvalidOperationException("Email address is already registered");
                }

                var user = _mapper.Map<User>(dto);
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
                var created = await _repo.AddUser(user);
                return _mapper.Map<UserDTO>(created);
            }
            catch (InvalidOperationException)
            {
                throw; // Re-throw validation errors as-is
            }
            catch (Exception ex)
            {
                throw new Exception($"Error creating user: {ex.Message}", ex);
            }
        }

        public async Task UpdateUser(Guid id, CreateUserDTO dto)
        {
            var user = _mapper.Map<User>(dto);
            user.UserId = id;
            user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            await _repo.UpdateUser(id, user);
        }

        public async Task<bool> DeleteUser(Guid id)
        {
            try
            {
                await _repo.DeleteUser(id);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
