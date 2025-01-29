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
        private readonly IFileCollectionService _fileCollectionService;
        private readonly IChatService _chatService;

        public ChatController(
            IFileCollectionService fileCollectionService,
            IChatService chatService)
        {
            _fileCollectionService = fileCollectionService;
            _chatService = chatService;
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

        private async Task HandleWebSocketConnection(WebSocket webSocket)
        {
            var buffer = new byte[1024 * 4];
            var documentId = 0;
            Document document = null;

            // Step 1: Receive document ID
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            if (result.MessageType == WebSocketMessageType.Text)
            {
                var documentIdStr = Encoding.UTF8.GetString(buffer, 0, result.Count);
                if (int.TryParse(documentIdStr, out documentId))
                {
                    document = await _fileCollectionService.GetDocumentByIdAsync(documentId);
                }
            }

            // Validate document exists
            if (document == null)
            {
                var errorMessage = "Invalid document ID. Please provide a valid document ID as the first message.";
                var errorBytes = Encoding.UTF8.GetBytes(errorMessage);
                await webSocket.SendAsync(new ArraySegment<byte>(errorBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                await webSocket.CloseAsync(WebSocketCloseStatus.InvalidPayloadData, "Invalid document ID", CancellationToken.None);
                return;
            }

            // Step 2: Prepare system prompt
            var systemPrompt = $@"You are a university document assistant. 
                Help students with the document: {document.DocumentName} 
                (Type: {document.ContentType}, Size: {document.DocumentSize} bytes). 
                Focus on providing clear explanations and context about this document.";

            // Step 3: Send welcome message
            var welcomeMessage = $@"Welcome to the {document.DocumentName} assistant! 
                Ask me anything about this document.";
            var welcomeBytes = Encoding.UTF8.GetBytes(welcomeMessage);
            await webSocket.SendAsync(new ArraySegment<byte>(welcomeBytes), WebSocketMessageType.Text, true, CancellationToken.None);

            // Step 4: Handle chat loop
            while (webSocket.State == WebSocketState.Open)
            {
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var userMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    var response = await _chatService.GetResponseAsync(systemPrompt, userMessage);
                    var responseBytes = Encoding.UTF8.GetBytes(response);
                    await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                else if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Connection closed", CancellationToken.None);
                }
            }
        }
    }
}