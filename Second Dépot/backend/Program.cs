string MyAllowSpecificOrigins = "AllowSpecificOrigins";

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();


builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
});

builder.Services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

builder.Services.AddEndpointsApiExplorer();

// Add DbContext
builder.Services.AddDbContext<ApplicationDBContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

//AUTH
builder.Services.AddSingleton<ICryptService, CryptService>();


builder.Services.AddScoped<ILiveRepository, LiveRepository>();
builder.Services.AddScoped<IStatisticRepository, StatisticRepository>();
builder.Services.AddScoped<IModerationRepository, ModerationRepository>();


//swagger
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();

//________________________________________________________

//----------------------------------------------------
//┌────────────────────────────────────┐
//│ JWT CONFIGURATION. 
//└────────────────────────────────────┘ 
//----------------------------------------------------

//swagger configuration
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Description = """Standard Authorization header using the Bearer scheme. Example : "bearer {token}" """,
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    c.OperationFilter<SecurityRequirementsOperationFilter>();
});

//authentification middleware
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)

    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration.GetSection("AppSettings:Token").Value!)),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

//--------------------------------------------------------------------------------------------------------------------------------------------

//CORS POLICY
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
        policy =>
            {
                //authorize access from api gateway
                policy.WithOrigins("http://localhost:5000", "http://localhost:8080", "http://localhost:44488", "http://tweetz.com", "http://100.96.208.53:44488"); // allow multiple origins (domain name tweetz.com)
                //policy.AllowAnyOrigin();
                _ = policy.AllowAnyHeader();
                _ = policy.AllowAnyMethod();
                _ = policy.AllowCredentials();
            });
});

/*
* Rate limiter configuration.
*/
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
    {
        // Utilisation de l'adresse IP comme clé de partition
        string remoteIpAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

        return RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: remoteIpAddress,
            factory: partition => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 150,
                Window = TimeSpan.FromMinutes(1)
            });
    });

    options.RejectionStatusCode = 429;
});

builder.Services.AddSignalR(options =>
{
    options.EnableDetailedErrors = true;
    options.MaximumReceiveMessageSize = 102400; // 100 KB
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("LiveChatAccess", policy =>
    {
        policy.RequireAuthenticatedUser();
    });
});

/*
* Body size request
*/
builder.Services.Configure<FormOptions>(options =>
    {
        options.MultipartBodyLengthLimit = 1_073_741_824; //1GO
    }
);



WebApplication app = builder.Build();


// Configure the HTTP request pipeline.
if (!app.Environment.IsProduction())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.UseRouting();
app.UseCors(MyAllowSpecificOrigins);

_ = app.Use(async (context, next) =>
{
    IHttpMaxRequestBodySizeFeature? maxRequestBodySizeFeature = context.Features.Get<IHttpMaxRequestBodySizeFeature>();
    if (maxRequestBodySizeFeature != null)
    {
        maxRequestBodySizeFeature.MaxRequestBodySize = 52428800; // 50 MB  
    }
    await next();
});

/*
* Rate limiter activation.
*/
if (app.Environment.IsProduction())
{
    _ = app.UseRateLimiter();
}

/*
* Content sec headers (Security headers + CSP protection)
*/
app.UseMiddleware<ContentSecHeaders>();

/*
* Configuring ASP.NET core auth middleware => handling TOKENS for requests.
*/
List<string> excludedPaths = [];

app.UseMiddleware<AuthCheckMiddleware>(excludedPaths);

app.UseAuthentication();
// app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// Only redirect to HTTPS in Production
if (app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseServiceDefaults();
app.MapHub<ChatHub>("/livechat");

app.Run();

public partial class Program { }
