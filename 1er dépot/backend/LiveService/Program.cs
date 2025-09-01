string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

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

builder.Services.AddSingleton<ICryptService, CryptService>();
builder.Services.AddScoped<ILiveRepository, LiveRepository>();
builder.Services.AddHttpClient();


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
                _ = policy.AllowAnyOrigin();
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



var app = builder.Build();

app.MapHub<ChatHub>("/chatHub");
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseMiddleware<ContentSecHeaders>();


app.UseAuthentication();
app.UseServiceDefaults();
app.UseAuthorization();
app.MapControllers();

await app.RunAsync();
