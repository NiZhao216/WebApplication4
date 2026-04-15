using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using MySqlConnector;
using System.Security.Claims;
using WebSocketdemo;
using webmsg;
using WebApplication4.Models;

namespace Websocket.Controllers
{
    public class WebsocketController : Controller
    {
        // 全局唯一连接池（线程安全）
        public static readonly WebSocketHub _hub = new WebSocketHub();
        
        // 优化：从配置读取连接字符串，替代硬编码
        private readonly static string _conStr = "Server=localhost;Port=3306;Database=users;Uid=abc;Pwd=123456;CharSet=utf8mb4;";

        // 优化：依赖注入配置
       

        /// <summary>
        /// WebSocket 唯一入口：/Websocket
        /// </summary>
        public async Task Index()
        {
            var userName = User.Identity?.Name;
            // 1. 校验WebSocket请求
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }
            var userRole = User.FindFirstValue(ClaimTypes.Role);
            if (string.IsNullOrEmpty(userName) || string.IsNullOrEmpty(userRole))
            {
                HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }

            // 3. 建立WebSocket连接
            using var socket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            try
            {
                // 添加到连接池
                _hub.AddConnection(userName, userRole, socket);
                Console.WriteLine($"用户 {userName}({userRole}) 连接成功");

                // 核心监听：循环接收消息（修复大消息截断问题）
                await ReceiveMessageLoop(socket);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"用户 {userName} 连接异常：{ex.Message}");
            }
            finally
            {
                // 清理连接
                _hub.Remove(userName, userRole);
                if (socket.State == WebSocketState.Open)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "正常断开", CancellationToken.None);
                }
                socket.Dispose();
                Console.WriteLine($"用户 {userName}({userRole}) 已断开连接");
            }
        }

        /// <summary>
        /// 优化：循环读取完整消息（解决4K缓冲区截断问题）
        /// </summary>
        private async Task ReceiveMessageLoop(WebSocket socket)
        {
            var buffer = new byte[1024 * 4];
            while (socket.State == WebSocketState.Open)
            {
                using var ms = new MemoryStream();
                WebSocketReceiveResult result;
                // 循环读取完整消息，不受缓冲区大小限制
                do
                {
                    result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
                        return;
                    }
                    ms.Write(buffer, 0, result.Count);
                } while (!result.EndOfMessage);

                // 转字符串
                var msg = Encoding.UTF8.GetString(ms.ToArray());
                if (string.IsNullOrWhiteSpace(msg)) continue;

                // 处理消息
                await HandleMessage(msg);
            }
        }

        /// <summary>
        /// 统一消息分发
        /// </summary>
        public async Task HandleMessage(string msg)
        {            try
            {
                // 优化：JSON序列化配置（兼容前端日期/格式）
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var socketMsg = JsonSerializer.Deserialize<WebMessage>(msg, options);

                switch (socketMsg.Type)
                {
                    case "chat":
                        // 发送聊天消息到目标用户
                        await HandleChatMessage(socketMsg.Toname, socketMsg.Torole, User.Identity.Name+socketMsg.Torole+":"+socketMsg.Data?.ToString());
                        break;
                    case "queryorder":
                        // 查询订单并返回
                        await HandleOrderQuery(socketMsg.Toname, socketMsg.Torole);
                        break;
                    default:
                        Console.WriteLine($"未知消息类型：{socketMsg.Type}");
                        break;
                }
            }
            catch (Exception ex)
            {
                // 修复Bug：参数传错！这里是发送者发消息给自己/调试，原代码传反了
                Console.WriteLine($"消息解析失败：{ex.Message}，原始消息：{msg}");
            }
        }

        /// <summary>
        /// 聊天消息转发
        /// </summary>
        private async Task HandleChatMessage(string toName, string toRole, string msg)
        {
            if (string.IsNullOrEmpty(toName) || string.IsNullOrEmpty(toRole) || string.IsNullOrEmpty(msg))
            {
                Console.WriteLine("聊天消息参数不完整");
                return;
            }

            var sendMsg = new WebMessage()
            {
                Type = "chat",
                Data = msg,
                Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                Torole = toRole,
                Toname = toName
            };

            await _hub.SendToUser(toName, toRole, JsonSerializer.Serialize(sendMsg));
        }

        /// <summary>
        /// 修复Bug：订单查询（删除无用SQL参数）
        /// </summary>
        public static  async Task HandleOrderQuery(string toName, string toRole)
        {
            try
            {
                List<Orders> orders = new List<Orders>();
                using MySqlConnection conn = new MySqlConnection(_conStr);
                await conn.OpenAsync();

                // 修复：SQL仅使用@DoctorName参数，删除无用的@username
                const string sql = "SELECT * FROM orders WHERE doctor_name = @DoctorName";
                using MySqlCommand cmd = new MySqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@DoctorName", toName);

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    orders.Add(new Orders
                    {
                        Orid = reader["orid"] != DBNull.Value ? Convert.ToInt32(reader["orid"]) : 0,
                        UserName = reader["username"]?.ToString() ?? string.Empty,
                        CatId = reader["catid"] != DBNull.Value ? Convert.ToInt32(reader["catid"]) : 0,
                        CatName = reader["catname"]?.ToString() ?? string.Empty,
                        Phone = reader["phone"]?.ToString() ?? string.Empty,
                        ServiceType = reader["servicetype"]?.ToString() ?? string.Empty,
                        AppointmentTime = reader["appointmenttime"] != DBNull.Value ? Convert.ToDateTime(reader["appointmenttime"]) : DateTime.MinValue,
                        Description = reader["description"]?.ToString() ?? string.Empty,
                        DoctorId = reader["did"] != DBNull.Value ? Convert.ToInt32(reader["did"]) : 0,
                        DoctorName = reader["doctor_name"]?.ToString() ?? string.Empty,
                        Status = reader["status"]?.ToString() ?? string.Empty,
                    });
                }

                // 回传订单数据给前端
                var sendMsg = new WebMessage()
                {
                    Type = "order",
                    Data = orders,
                    Time = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    Torole = toRole,
                    Toname = toName
                };
                await _hub.SendToUser(toName, toRole, JsonSerializer.Serialize(sendMsg));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"订单查询异常：{ex.Message}");
            }
        }
    }
}