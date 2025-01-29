using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using ProjetDotNet.Models;
using ProjetDotNet.Service;
using System;
using System.Threading;

namespace ProjetDotNet.Controllers
{
    [Route("/ws/chat")]
    public class ChatController : ControllerBase
    {
        private readonly IFileService _fileService;
        private readonly IChatService _chatService;
        private readonly IPdfParserService _pdfParserService;
        public ChatController(
            IFileService fileService,
            IChatService chatService,
            IPdfParserService pdfParserService)
        {
            _fileService = fileService;
            _chatService = chatService;
            _pdfParserService = pdfParserService;
        }

        public async Task Handle()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                await HandleWebSocketConnection(webSocket);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }

        private async Task SendMessage(WebSocket webSocket, string message)
        {
            var messageBytes = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(
                new ArraySegment<byte>(messageBytes, 0, messageBytes.Length),
                WebSocketMessageType.Text,
                true,
                CancellationToken.None
            );
        }

        private async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var documentId = 0;
            FileModel fileModel = null;
            List<Dictionary<string, string>> chatHistory = new List<Dictionary<string, string>>();
            String  content = null; 
            // Step 1: Receive document ID
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var documentIdStr = Encoding.UTF8.GetString(buffer, 0, result.Count);
                if (int.TryParse(documentIdStr, out documentId))
                {
                    fileModel = await _fileService.GetFileAsync(documentId);
                    content = await _pdfParserService.ParsePdfToText(fileModel.FilePath);
                }
            }

            // Validate document exists
            if (fileModel == null)
            {
                var errorMessage = "Invalid document ID. Please provide a valid document ID as the first message.";
                var errorBytes = Encoding.UTF8.GetBytes(errorMessage);
                await webSocket.SendAsync(new ArraySegment<byte>(errorBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Invalid document ID", CancellationToken.None);
                return;
            }

            var systemPrompt = $@"You are a university document assistant. 
            Help students with the document:  {content}
            Focus on providing clear explanations and context about this document.";
            chatHistory.Add(new Dictionary<string, string>
            {
                { "role", "system" },
                { "content", systemPrompt }
            });
            var welcomeMessage = "Welcome! Ask me anything about the document.";
            await SendMessage(webSocket, welcomeMessage);
            chatHistory.Add(new Dictionary<string, string>
            {
                { "role", "assistant" },
                { "content", welcomeMessage }
            });

            // Chat loop
            while (webSocket.State == WebSocketState.Open)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var userMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);

                    // Add user message to history
                    chatHistory.Add(new Dictionary<string, string>
                    {
                        { "role", "user" },
                        { "content", userMessage }
                    });


                    // Get response from Groq
                    var response = await _chatService.GetResponseAsync(chatHistory);

                    // Add assistant response to history
                    chatHistory.Add(new Dictionary<string, string>
                    {
                        { "role", "assistant" },
                        { "content", userMessage }
                    });

                    await SendMessage(webSocket, response);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                }
            }
        }
    }
}