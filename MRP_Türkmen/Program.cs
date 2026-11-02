
using System.Net;
using System.Text;
using System.Text.Json;
using MRP.Presentation;

HttpListener listener = new HttpListener();
listener.Prefixes.Add("http://localhost:8080/");
listener.Start();

Console.WriteLine("MRP Server started!");
Console.WriteLine("Listening on http://localhost:8080/");

UserEndpoint userEndpoint = new();
MediaEndpoint mediaEndpoint = new();

while (true)
{
    HttpListenerContext context = await listener.GetContextAsync();

    string path = context.Request.Url?.AbsolutePath ?? "/";

    try
    {
        if (path.StartsWith("/api/users/"))
        {
            await userEndpoint.HandleRequest(context);
        }
        else if (path == "/api/media" ||
                 path.StartsWith("/api/media/"))
        {
            await mediaEndpoint.HandleRequest(context);
        }
        else if (path == "/" &&
                 context.Request.HttpMethod == "GET")
        {
            byte[] bytes = Encoding.UTF8.GetBytes(
                JsonSerializer.Serialize(
                    new { message = "Welcome to Media Ratings Platform!" }));

            context.Response.StatusCode = 200;
            context.Response.ContentType = "application/json";
            context.Response.ContentLength64 = bytes.Length;

            await context.Response.OutputStream.WriteAsync(bytes);
            context.Response.Close();
        }
        else
        {
            byte[] bytes = Encoding.UTF8.GetBytes(
                "{\"error\":\"Not Found\"}");

            context.Response.StatusCode = 404;
            context.Response.ContentType = "application/json";
            context.Response.ContentLength64 = bytes.Length;

            await context.Response.OutputStream.WriteAsync(bytes);
            context.Response.Close();
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine("Server error: " + ex.Message);

        try
        {
            if (context.Response.OutputStream.CanWrite)
            {
                byte[] bytes = Encoding.UTF8.GetBytes(
                    "{\"error\":\"Internal Server Error\"}");

                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                context.Response.ContentLength64 = bytes.Length;

                await context.Response.OutputStream.WriteAsync(bytes);
                context.Response.Close();
            }
        }
        catch (Exception)
        {
            // Verbindung wurde möglicherweise bereits geschlossen.
        }
    }
}
