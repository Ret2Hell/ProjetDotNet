using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProjetDotNet.Service
{
    public interface IChatService
    {
        Task<string> GetResponseAsync(List<Dictionary<string, string>> chatHistory);
    }
}