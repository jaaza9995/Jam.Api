using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Jam.Models;
using Microsoft.AspNetCore.Identity;
using Jam.DAL;
using Jam.DAL.StoryDAL;
using Jam.DAL.SceneDAL;
using Jam.DAL.AnswerOptionDAL;
using Jam.DAL.PlayingSessionDAL;
using Jam.DAL.ApplicationUserDAL;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = AppContext.BaseDirectory
});

var connectionString = builder.Configuration.GetConnectionString("StoryDbContextConnection") ?? throw new InvalidOperationException("Connection string 'StoryDbContextConnection' not found.");

builder.Services.AddControllers().AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    // options.SerializerSettings.ContractResolver = new DefaultContractResolver();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<StoryDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration["ConnectionStrings:StoryDbContextConnection"]);
});
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<StoryDbContext>()
    .AddDefaultTokenProviders();


builder.Services.AddControllers();
builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
        });

builder.Services.AddScoped<IAnswerOptionRepository, AnswerOptionRepository>();
builder.Services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
builder.Services.AddScoped<IPlayingSessionRepository, PlayingSessionRepository>();
builder.Services.AddScoped<ISceneRepository, SceneRepository>();
builder.Services.AddScoped<IStoryRepository, StoryRepository>();

var loggerConfiguration = new LoggerConfiguration()
    .MinimumLevel.Information() // levels: Trace< Information < Warning < Erorr < Fatal
    .WriteTo.File($"APILogs/app_{DateTime.Now:yyyyMMdd_HHmmss}.log")
    .Filter.ByExcluding(e => e.Properties.TryGetValue("SourceContext", out var value) &&
                            e.Level == LogEventLevel.Information &&
                            e.MessageTemplate.Text.Contains("Executed DbCommand"));
var logger = loggerConfiguration.CreateLogger();
builder.Logging.AddSerilog(logger);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    await DBInit.SeedAsync(app);
    app.UseSwagger();
    app.UseSwaggerUI();
    
}
app.UseStaticFiles();
app.UseRouting();
app.UseCors("CorsPolicy");
app.MapControllers();
    
app.Run();