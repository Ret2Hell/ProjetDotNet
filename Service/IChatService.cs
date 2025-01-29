using System.Threading.Tasks;

namespace ProjetDotNet.Service
{
    public interface IChatService
    {
        Task<string> GetResponseAsync(string systemPrompt, string userMessage);
    }
}