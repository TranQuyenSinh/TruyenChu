using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using truyenchu.Data;
using truyenchu.ExtendMethods;
using truyenchu.Models;
using truyenchu.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews()
                .AddNewtonsoftJson(options =>
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore
                );
builder.Services.AddRazorPages();

/* ================ DbContext ================ */
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("TruyenChu"));
});

/* ================ Service ================ */
builder.Services.AddTransient(typeof(StoryService), typeof(StoryService));

var app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
// file tĩnh trong /Uploads
app.UseStaticFiles(new StaticFileOptions()
{
    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "Uploads")),
    RequestPath = "/contents"
});
app.UseCookiePolicy();

app.UseRouting();
app.AddStatusCodePage();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapAreaControllerRoute(
    name: "default2",
    pattern: "{controller=ViewStory}/{action=Index}/{id?}", areaName: "ViewStory");

app.MapRazorPages();

app.Run();
