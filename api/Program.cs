using Microsoft.EntityFrameworkCore;
using Serilog;
using Serilog.Events;
using Jam.Api.Models;
using Microsoft.AspNetCore.Identity;
using Jam.Api.DAL;
using Jam.Api.DAL.AnswerOptionDAL;
using Jam.Api.DAL.PlayingSessionDAL;
using Jam.Api.DAL.SceneDAL;
using Jam.Api.DAL.StoryDAL;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.OpenApi.Models;
using Jam.Api.DAL.Services;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    ContentRootPath = AppContext.BaseDirectory
});

var connectionString = builder.Configuration.GetConnectionString("StoryDbContextConnection") ?? throw new InvalidOperationException("Connection string 'StoryDbContextConnection' not found.");

builder.Services.AddControllers(config =>
{
    // Made a global policy that requires authentication for everything
    // This can be overridden with [AllowAnonymous] on individual controllers or actions
    var policy = new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder()
                     .RequireAuthenticatedUser()
                     .Build();
    config.Filters.Add(new Microsoft.AspNetCore.Mvc.Authorization.AuthorizeFilter(policy));
})
.AddNewtonsoftJson(options =>
{
    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
    options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver();
});


builder.Services.AddEndpointsApiExplorer();
// for debugging:
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "JamReact API", Version = "v1" }); // Basic info for the API
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme // Define the Bearer auth scheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement // Require Bearer token for accessing the API
    {{ new OpenApiSecurityScheme // Reference the defined scheme
            { Reference = new OpenApiReference
                { Type = ReferenceType.SecurityScheme,
                  Id = "Bearer"}},
            new string[] {}
        }});
});

builder.Services.AddDistributedMemoryCache();

builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(60);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddDbContext<StoryDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration["ConnectionStrings:StoryDbContextConnection"]);
});

builder.Services.AddDbContext<AuthDbContext>(options =>
{
    options.UseSqlite(
        builder.Configuration["ConnectionStrings:AuthDbContextConnection"]);
});

builder.Services.AddIdentity<AuthUser, IdentityRole>()
    .AddEntityFrameworkStores<AuthDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddCors(options =>
    {
    options.AddPolicy("CorsPolicy", builder =>
    {
        builder.WithOrigins(
                "http://localhost:4000"  // custom Vite port (matches VITE_PORT)
            ) // Allow requests from the React frontend
               .AllowAnyMethod()
               .AllowAnyHeader()
               .AllowCredentials();
    });
});

builder.Services.AddScoped<IAnswerOptionRepository, AnswerOptionRepository>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IPlayingSessionRepository, PlayingSessionRepository>();
builder.Services.AddScoped<ISceneRepository, SceneRepository>();
builder.Services.AddScoped<IStoryRepository, StoryRepository>();

builder.Services.AddScoped<StoryCodeService>();

builder.Services.AddAuthorization(options =>
{
    // Policy for admin
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateIssuer = true, // Validate the token issuer is correct
        ValidateAudience = true, // Validate the token reciepient is correct 
        ValidateLifetime = true, // Validate the token has not expired
        ValidateIssuerSigningKey = true, // Validate the JWT signature
        ValidIssuer = builder.Configuration["Jwt:Issuer"], // Reading the issuer of the token
        ValidAudience = builder.Configuration["Jwt:Audience"], // Reading the audience for the token
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
            builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("JWT key not found in configuration.") // Reading the key from the configuration
        ))
    };
});

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

// Ensure Admin role/user exists (idempotent, no data reset)
async Task EnsureAdminUserAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AuthUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    const string adminRole = "Admin";
    const string adminUserName = "AdminUser";
    const string adminPassword = "AdminPassword123!";

    if (!await roleManager.RoleExistsAsync(adminRole))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRole));
    }

    var admin = await userManager.FindByNameAsync(adminUserName);
    if (admin == null)
    {
        admin = new AuthUser
        {
            UserName = adminUserName,
            EmailConfirmed = true
        };
        var createResult = await userManager.CreateAsync(admin, adminPassword);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException($"Could not create admin user: {string.Join(", ", createResult.Errors.Select(e => e.Description))}");
        }
    }

    if (!await userManager.IsInRoleAsync(admin, adminRole))
    {
        await userManager.AddToRoleAsync(admin, adminRole);
    }
}

await EnsureAdminUserAsync(app);

app.UseStaticFiles();

app.UseRouting();
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseAuthorization();
app.UseSession();
app.MapControllers();

app.Run();
