using System.Net;

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

                    var content = @$"
                    <!DOCTYPE html>
                        <head>
                            <meta charset='UTF-8'>
                            <title>Lỗi: {statusCode}</title>
                        </head>
                        <body>
                            <h2>
                                Có lỗi xảy ra: {statusCode} - đường dẫn không tồn tại
                            </h2>
                            <p>
                                Có thể truyện bạn muốn xem vì lý do nào đó mà đã bị thay đổi link, 
                                hãy <a href='/'>trở về trang chủ</a> và thử tìm kiếm với tên truyện đó. 
                                Chúng mình rất xin lỗi vì sự bất tiện này!
                            </p>
                        </body>
                        </html>
                    ";

                    await context.Response.WriteAsync(content);
                });
            });
        }
    }
}