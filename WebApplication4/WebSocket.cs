using System.Collections.Concurrent;
using System.ComponentModel;
using System.Net.WebSockets;
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
namespace WebSocketdemo
{
    public class WebSocketHub
    {
        public static ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();

        public void AddConnection(string id, string role,WebSocket socket)
        {
            var key = $"{role}:{id}";
            _sockets.TryAdd(key, socket);
        } 
        public void Remove(string id , string role)
        {
            var key = $"{role}:{id}";
             _sockets.TryRemove(key, out _);
        }
        public async Task SendToUser(string role, string id, string message)
        {
            var key = $"{role}:{id}";
           
            if (_sockets.TryGetValue(key, out var socket) && socket.State == WebSocketState.Open)
            {
                var buffer = System.Text.Encoding.UTF8.GetBytes(message);
                await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }


    }
}