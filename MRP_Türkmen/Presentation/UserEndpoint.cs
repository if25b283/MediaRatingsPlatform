using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using MRP.Business;

namespace MRP.Presentation
{
    internal class UserEndpoint
    {
        private readonly UserService service = new();
        private readonly AuthService authService = new();

        public async Task HandleRequest(HttpListenerContext context)
        {
            string path = context.Request.Url?.AbsolutePath ?? "";

            if (context.Request.HttpMethod != "POST")
            {
                await SendResponse(context, 405,
                    new { error = "Method Not Allowed" });
                return;
            }

            if (path == "/api/users/register")
            {
                await RegisterUser(context);
            }
            else if (path == "/api/users/login")
            {
                await LoginUser(context);
            }
            else
            {
                await SendResponse(context, 404,
                    new { error = "Not Found" });
            }
        }

        public async Task RegisterUser(HttpListenerContext context)
        {
            var user = await ReadUser(context);

            if (user == null)
            {
                await SendResponse(context, 400,
                    new { error = "Invalid JSON" });
                return;
            }

            bool success = service.Register(
                user.Username, user.Password);

            await SendResponse(context, success ? 201 : 400,
                new { success });
        }

        public async Task LoginUser(HttpListenerContext context)
        {
            var user = await ReadUser(context);

            if (user == null)
            {
                await SendResponse(context, 400,
                    new { error = "Invalid JSON" });
                return;
            }

            bool success = service.Login(
                user.Username, user.Password);

            if (!success)
            {
                await SendResponse(context, 401,
                    new { error = "Invalid credentials" });
                return;
            }


            int? userId = service.GetUserId(user.Username);

            if (userId == null)
            {
                await SendResponse(context, 401,
                    new { error = "Invalid credentials" });
                return;
            }

            string token = authService.GenerateToken(userId.Value);


            await SendResponse(context, 200,
                new { success = true, token });
        }

        private static async Task<LoginRequest?> ReadUser(
            HttpListenerContext context)
        {
            try
            {
                return await JsonSerializer.DeserializeAsync<LoginRequest>(
                    context.Request.InputStream,
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });
            }
            catch (JsonException)
            {
                return null;
            }
        }

        private static async Task SendResponse(
            HttpListenerContext context, int status, object data)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(data));

            context.Response.StatusCode = status;
            context.Response.ContentType = "application/json";
            context.Response.ContentLength64 = bytes.Length;

            await context.Response.OutputStream.WriteAsync(bytes);
            context.Response.Close();
        }

        private class LoginRequest
        {
            public string Username { get; set; } = "";
            public string Password { get; set; } = "";
        }
    }
}
