using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace CabinCrewCareer.Infrastructure.Hubs
{
    [Authorize] // Yêu cầu JWT authentication
    public class NotificationHub : Hub
    {
        // Track mapping giữa ConnectionId và UserId
        private static readonly ConcurrentDictionary<string, string> _connections = new();
        
        // Track mapping giữa UserId và list ConnectionIds (1 user có thể có nhiều tabs/devices)
        private static readonly ConcurrentDictionary<string, HashSet<string>> _userConnections = new();

        public override async Task OnConnectedAsync()
        {
            // Lấy userId từ JWT claims
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            
            // Có thể thử các claim names khác nếu cần
            if (string.IsNullOrEmpty(userId))
            {
                userId = Context.User?.FindFirst("userId")?.Value;
            }
            if (string.IsNullOrEmpty(userId))
            {
                userId = Context.User?.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;
            }

            if (!string.IsNullOrEmpty(userId))
            {
                // Lưu mapping connectionId -> userId
                _connections[Context.ConnectionId] = userId;

                // Lưu mapping userId -> connectionIds
                if (!_userConnections.ContainsKey(userId))
                {
                    _userConnections[userId] = new HashSet<string>();
                }
                _userConnections[userId].Add(Context.ConnectionId);

                // Add connection vào user group
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");

                // Lấy role từ JWT claims (nếu có)
                var role = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
                if (string.IsNullOrEmpty(role))
                {
                    role = Context.User?.FindFirst("http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
                }

                if (!string.IsNullOrEmpty(role))
                {
                    // Add vào role group
                    await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{role}");
                }

                Console.WriteLine($"SignalR: User {userId} connected with ConnectionId {Context.ConnectionId}");
                Console.WriteLine($"SignalR: User {userId} now has {_userConnections[userId].Count} active connection(s)");
            }
            else
            {
                Console.WriteLine($"SignalR: Anonymous connection {Context.ConnectionId} - no userId in token");
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (_connections.TryRemove(Context.ConnectionId, out var userId))
            {
                // Remove connection khỏi user's connection set
                if (_userConnections.ContainsKey(userId))
                {
                    _userConnections[userId].Remove(Context.ConnectionId);
                    
                    // Nếu user không còn connection nào, xóa entry
                    if (_userConnections[userId].Count == 0)
                    {
                        _userConnections.TryRemove(userId, out _);
                    }
                }

                Console.WriteLine($"SignalR: User {userId} disconnected (ConnectionId: {Context.ConnectionId})");
                Console.WriteLine($"SignalR: User {userId} remaining connections: {(_userConnections.ContainsKey(userId) ? _userConnections[userId].Count : 0)}");
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Optional: Methods để frontend có thể join groups thủ công (nếu cần)
        public async Task JoinRoleGroup(string role)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Role_{role}");
            Console.WriteLine($"SignalR: ConnectionId {Context.ConnectionId} joined Role_{role}");
        }

        public async Task JoinUserGroup(string userId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{userId}");
            Console.WriteLine($"SignalR: ConnectionId {Context.ConnectionId} joined User_{userId}");
        }

        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            Console.WriteLine($"SignalR: ConnectionId {Context.ConnectionId} joined {groupName}");
        }

        // Method để get info debug
        public string GetConnectionInfo()
        {
            var userId = _connections.GetValueOrDefault(Context.ConnectionId);
            var connectionCount = userId != null && _userConnections.ContainsKey(userId) 
                ? _userConnections[userId].Count 
                : 0;
            
            return $"ConnectionId: {Context.ConnectionId}, UserId: {userId}, Total connections: {connectionCount}";
        }
    }
}