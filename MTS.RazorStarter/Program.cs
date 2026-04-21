using Microsoft.EntityFrameworkCore;
using Mts.Infrastructure;
using Mts.Domain;
using MTS.RazorStarter.Services;

var builder = WebApplication.CreateBuilder(args);

// Services
builder.Services.AddScoped<FramePartService>();
builder.Services.AddScoped<FrameDrawingService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<StationDashboardService>();
builder.Services.AddScoped<NavigationService>();
builder.Services.AddScoped<BomService>();
builder.Services.AddScoped<FrameBomExtractor>();

// Razor Pages + Auth
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Stations/Dashboard");
});

// DB
builder.Services.AddDbContext<MtsDbContext>(options =>
    options.UseSqlite("Data Source=mts.db"));

// Optional API client
builder.Services.Configure<MtsApiOptions>(builder.Configuration.GetSection("MtsApi"));
builder.Services.AddHttpClient<MtsApiClient>((serviceProvider, client) =>
{
    var options = serviceProvider
        .GetRequiredService<IConfiguration>()
        .GetSection("MtsApi")
        .Get<MtsApiOptions>() ?? new MtsApiOptions();

    if (!string.IsNullOrWhiteSpace(options.BaseUrl))
    {
        client.BaseAddress = new Uri(options.BaseUrl);
    }

    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds > 0 ? options.TimeoutSeconds : 30);
});

// Auth
builder.Services.AddAuthorization();
builder.Services.AddAuthentication("Cookies")
    .AddCookie("Cookies", options =>
    {
        options.LoginPath = "/";
        options.AccessDeniedPath = "/";
    });

var app = builder.Build();

// Middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// DB Init + Seed
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MtsDbContext>();

    db.Database.EnsureCreated();

    if (!db.Items.Any())
    {
        var item = new Item
        {
            ItemNo = "12345",
            ItemType = ItemType.FramePart,
            Title = "Test Frame Part",
            UnitOfMeasure = "EA",
            LifecycleState = LifecycleState.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        db.Items.Add(item);
        db.SaveChanges();

        db.ItemRevisions.Add(new ItemRevision
        {
            ItemId = item.Id,
            RevisionCode = "A",
            IsCurrent = true,
            ReleaseState = ReleaseState.Released,
            CreatedAt = DateTime.UtcNow
        });

        db.SaveChanges();
    }
}

// Routes
app.MapRazorPages();

app.Run();
