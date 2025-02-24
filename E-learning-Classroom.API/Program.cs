using E_learning_Classroom.API.Domain.Entities;
using E_learning_Classroom.API.Extentions;
using E_learning_Classroom.API.Extentions.Automapper;
using E_learning_Classroom.API.Infrastructure.Context;
using E_learning_Classroom.API.Infrastructure.Repository;
using E_learning_Classroom.API.Service.Interface;
using E_learning_Classroom.API.Service.Interface.Repository;
using E_learning_Classroom.API.Service.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
 
// Adding Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "User Auth", Version = "v1", Description = "Services to Authenticate user" });


    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Please enter a valid token in the following format: {your token here} do not add the word 'Bearer' before it."
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header,
            },
            new List<string>()
        }
    });
});
// Adding Database context 
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

// Adding Identity

builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureIdentity();
builder.Services.ConfigureJwt(builder.Configuration);
builder.Services.ConfigureCors();

// Adding Services  
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ITokenService, TokenServiceImple>();

builder.Services.AddScoped<IRoleRepository, RoleRepository>();

// Regsitering AutoMapper
builder.Services.AddAutoMapper(typeof(MappingProfile).Assembly);
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();


// Authentication : Register - login - logout
// Authorization : Roles - Permissions - Policy
// Middleware: Default - Customs
// Security API : API versioning - IPs Whitlist - CORS - Headers XXS
// FluentValidation - Global using - General Response - Error Handling - Logging
// Repository DP - DI - Unit of work - CQRS - SOLID  
// Razore - Ajax
// Cache 
// Payment Integration
// Email hangfire

// User stories




// ASP .NET Core API
// Simi clean architure - MVC - Domain - Infrastracture - Alpplication - API
// Repository DP - Repository - Services 
// Generic Repository 
// Untit of work DP
// Custom configration 
// Context - connection string - Entities with propos and migration 
// Entity Framework 
// Automapper - congigration



// Ready Application
// Identity - JWT  
// User stories



