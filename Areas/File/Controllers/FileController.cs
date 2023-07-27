using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using elFinder.NetCore;
using elFinder.NetCore.Drivers.FileSystem;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace truyenchu.Area.File.Controllers
{
    [Authorize]
    [Route("/file")]
    [Area("File")]
    public class FileController : Controller
    {
        IWebHostEnvironment _env;
        public FileController(IWebHostEnvironment env) => _env = env;

        public IActionResult Index()
        {
            return View();
        }
        // Url để client-side kết nối đến backend
        // /el-finder-file-system/connector
        [Route("/file-manager-connector")]
        public async Task<IActionResult> Connector()
        {
            var connector = GetConnector();
            var result = await connector.ProcessAsync(Request);
            if (result is JsonResult)
            {
                var json = result as JsonResult;
                return Content(JsonSerializer.Serialize(json.Value), json.ContentType);
            }
            else
            {
                return result;
            }
        }

        // Địa chỉ để truy vấn thumbnail
        // /el-finder-file-system/thumb
        [Route("/thumb/{hash}")]
        public async Task<IActionResult> Thumbs(string hash)
        {
            var connector = GetConnector();
            return await connector.GetThumbnailAsync(HttpContext.Request, HttpContext.Response, hash);
        }

        private Connector GetConnector()
        {
            // Thư mục gốc lưu trữ là wwwwroot/files (đảm bảo có tạo thư mục này)
            string pathroot = "Uploads"; // => default: wwwwroot/files
            string requestUrl = "contents";

            var driver = new FileSystemDriver();

            string absoluteUrl = UriHelper.BuildAbsolute(Request.Scheme, Request.Host);
            var uri = new Uri(absoluteUrl);

            // Nếu dùng thư mục wwww/files
            // string rootDirectory = Path.Combine(_env.WebRootPath, pathroot); => đường dẫn tương đối

            // Nếu dùng thư mục /Uploads
            string rootDirectory = Path.Combine(_env.ContentRootPath, pathroot); // => đường dẫn vật lý thực của thư mục ứng dụng

            // https://localhost:5001/files/
            // string url = $"{uri.Scheme}://{uri.Authority}/{pathroot}/";
            string url = $"{uri.Scheme}://{uri.Authority}/{requestUrl}/";
            string urlthumb = $"{uri.Scheme}://{uri.Authority}/thumb/";


            var root = new RootVolume(rootDirectory, url, urlthumb)
            {
                //IsReadOnly = !User.IsInRole("Administrators")
                IsReadOnly = false, // Can be readonly according to user's membership permission
                IsLocked = false, // If locked, files and directories cannot be deleted, renamed or moved
                Alias = "Thư mục ứng dụng", // Beautiful name given to the root/home folder
                //MaxUploadSizeInKb = 2048, // Limit imposed to user uploaded file <= 2048 KB
                //LockedFolders = new List<string>(new string[] { "Folder1" }
                ThumbnailSize = 100,
            };


            driver.AddRoot(root);

            return new Connector(driver)
            {
                // This allows support for the "onlyMimes" option on the client.
                MimeDetect = MimeDetectOption.Internal
            };
        }
    }
}