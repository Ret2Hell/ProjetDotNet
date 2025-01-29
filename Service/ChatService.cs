using System.Threading.Tasks;

namespace ProjetDotNet.Service
{
    public class ChatService : IChatService
    {
        public async Task<string> GetResponseAsync(string systemPrompt, string userMessage)
        {
            // TODO: Integrate with your actual LLM (OpenAI, HuggingFace, etc.)
            // This is a stub implementation
            return await Task.FromResult($"[System: {systemPrompt}]\nResponse to: {userMessage}");
        }
    }
}