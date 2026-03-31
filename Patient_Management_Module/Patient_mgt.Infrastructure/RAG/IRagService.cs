using System;
using System.Collections.Generic;
using System.Text;

namespace Patient_mgt.Infrastructure.RAG
{
    public interface IRagService
    {
        Task<List<string>> GetRelevantChunks(string query);
    }
}
