using Westwind.AspNetCore.LiveReload;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddLiveReload(config =>
{
    config.FolderToMonitor = Path.Combine(config.FolderToMonitor, "wwwroot/documents");

    config.FileInclusionFilter = path =>
    {
        if (! path.Contains("output.pgn"))
        {
            return FileInclusionModes.ContinueProcessing;
        }
        else
        {
            Thread.Sleep(500);
            return FileInclusionModes.ForceRefresh;
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseLiveReload();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Pdf}/{action=Index}/{id?}");


new Thread(new ThreadStart(PdfTesting.HelloWorld)).Start();

app.Run();
