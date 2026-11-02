using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using MRP.Business;
using MRP.Domain;

namespace MRP.Presentation
{
    internal class MediaEndpoint
    {
        private readonly MediaService service = new();
        private readonly AuthService authService = new();

        private static readonly JsonSerializerOptions jsonOptions = new()
        {
            PropertyNameCaseInsensitive = true,
            Converters = { new JsonStringEnumConverter() }
        };

        public async Task HandleRequest(HttpListenerContext context)
        {
            string path = context.Request.Url?.AbsolutePath ?? "";
            string method = context.Request.HttpMethod;

            string[] parts = path.Trim('/').Split('/');

            // Route: /api/media
            if (parts.Length == 2 &&
                parts[0] == "api" &&
                parts[1] == "media")
            {
                if (method == "GET")
                    await GetAllMedia(context);
                else if (method == "POST")
                    await CreateMedia(context);
                else
                    await SendResponse(context, 405,
                        new { error = "Method Not Allowed" });

                return;
            }

            // Route: /api/media/{id}
            if (parts.Length == 3 &&
                parts[0] == "api" &&
                parts[1] == "media" &&
                int.TryParse(parts[2], out int id) && id > 0)
            {
                if (method == "GET")
                    await GetMediaById(context, id);
                else if (method == "PUT")
                    await UpdateMedia(context, id);
                else if (method == "DELETE")
                    await DeleteMedia(context, id);
                else
                    await SendResponse(context, 405,
                        new { error = "Method Not Allowed" });

                return;
            }

            await SendResponse(context, 404,
                new { error = "Not Found" });
        }

        // GET /api/media
        private async Task GetAllMedia(HttpListenerContext context)
        {
            var media = service.GetAll();
            await SendResponse(context, 200, media);
        }

        // GET /api/media/1
        private async Task GetMediaById(
            HttpListenerContext context, int id)
        {
            var media = service.GetById(id);

            if (media == null)
            {
                await SendResponse(context, 404,
                    new { error = "Media not found" });
                return;
            }

            await SendResponse(context, 200, media);
        }

        // POST /api/media
        private async Task CreateMedia(HttpListenerContext context)
        {
            int? userId = GetUserId(context);

            if (userId == null)
            {
                await SendResponse(context, 401,
                    new { error = "Unauthorized" });
                return;
            }

            Media? media = await ReadMedia(context);

            if (media == null)
            {
                await SendResponse(context, 400,
                    new { error = "Invalid media data" });
                return;
            }

            // Ersteller wird aus dem Token bestimmt
            media.CreatorId = userId.Value;

            try
            {
                int id = service.AddMedia(media);

                await SendResponse(context, 201,
                    new { success = true, id });
            }
            catch (ArgumentException ex)
            {
                await SendResponse(context, 400,
                    new { error = ex.Message });
            }
        }

        // PUT /api/media/1
        private async Task UpdateMedia(
            HttpListenerContext context, int id)
        {
            int? userId = GetUserId(context);

            if (userId == null)
            {
                await SendResponse(context, 401,
                    new { error = "Unauthorized" });
                return;
            }

            Media? existing = service.GetById(id);

            if (existing == null)
            {
                await SendResponse(context, 404,
                    new { error = "Media not found" });
                return;
            }

            if (existing.CreatorId != userId.Value)
            {
                await SendResponse(context, 403,
                    new { error = "Forbidden" });
                return;
            }

            Media? media = await ReadMedia(context);

            if (media == null)
            {
                await SendResponse(context, 400,
                    new { error = "Invalid media data" });
                return;
            }

            media.Id = id;
            media.CreatorId = userId.Value;

            try
            {
                bool success = service.UpdateMedia(media);

                await SendResponse(context, success ? 200 : 404,
                    new { success });
            }
            catch (ArgumentException ex)
            {
                await SendResponse(context, 400,
                    new { error = ex.Message });
            }
        }

        // DELETE /api/media/1
        private async Task DeleteMedia(
            HttpListenerContext context, int id)
        {
            int? userId = GetUserId(context);

            if (userId == null)
            {
                await SendResponse(context, 401,
                    new { error = "Unauthorized" });
                return;
            }

            Media? existing = service.GetById(id);

            if (existing == null)
            {
                await SendResponse(context, 404,
                    new { error = "Media not found" });
                return;
            }

            if (existing.CreatorId != userId.Value)
            {
                await SendResponse(context, 403,
                    new { error = "Forbidden" });
                return;
            }

            bool success = service.DeleteMedia(id, userId.Value);

            await SendResponse(context, success ? 200 : 404,
                new { success });
        }

        private int? GetUserId(HttpListenerContext context)
        {
            string? header = context.Request.Headers["Authorization"];
            return authService.ValidateToken(header);
        }

        private static async Task<Media?> ReadMedia(
            HttpListenerContext context)
        {
            try
            {
                var input =
                    await JsonSerializer.DeserializeAsync<MediaInput>(
                        context.Request.InputStream, jsonOptions);

                if (input == null || input.Type == null)
                    return null;

                return new Media
                {
                    Title = input.Title,
                    Description = input.Description,
                    Type = input.Type.Value,
                    ReleaseYear = input.ReleaseYear,
                    Genres = input.Genres,
                    AgeRestriction = input.AgeRestriction
                };
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
                JsonSerializer.Serialize(data, jsonOptions));

            context.Response.StatusCode = status;
            context.Response.ContentType =
                "application/json; charset=utf-8";
            context.Response.ContentLength64 = bytes.Length;

            await context.Response.OutputStream.WriteAsync(bytes);
            context.Response.Close();
        }

        private class MediaInput
        {
            public string Title { get; set; } = "";
            public string Description { get; set; } = "";
            public MediaType? Type { get; set; }
            public int ReleaseYear { get; set; }
            public List<string> Genres { get; set; } = new();
            public int AgeRestriction { get; set; }
        }
    }
}
