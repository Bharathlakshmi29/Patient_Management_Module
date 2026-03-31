using Patient_mgt.DTOs;

namespace Patient_mgt.Infrastructure
{
    public interface IIcdService
    {
        Task<List<IcdCodeDTO>> SearchIcdCodes(string query);
    }
}