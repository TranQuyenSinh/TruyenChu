using System.Net;
using truyenchu.Data;

namespace truyenchu.ExtendMethods
{
    public static class AppExtends
    {
        public static void AddStatusCodePage(this WebApplication app)
        {
            app.UseStatusCodePages(appError =>
            {
                appError.Run(async context =>
                {
                    var statusCode = context.Response.StatusCode;

                    var content = System.IO.File.ReadAllText(
                        Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot", "html", "404.html")
                    );

                    await context.Response.WriteAsync(content);
                });
            });
        }
    }
}