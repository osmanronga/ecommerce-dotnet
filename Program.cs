using System.Text;
using BestStoreApi.Filters;
using BestStoreApi.Interfaces;
using BestStoreApi.Middelwares;
using BestStoreApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

//read configuration from appsettings.json
// var AppName = builder.Configuration["AppName"];
// var Log = builder.Configuration["Logging:LogLevel:Default"];

// Console.WriteLine("AppName: " + AppName + ", Log: " + Log);

// Add services to the container.

// here we can add filters to the controller to use like middleware for allrequests
builder.Services.AddControllers(options =>
{
    options.Filters.Add<StatFilter>();
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    // we use OAuth2 authentication to make us user JWToken
    options.AddSecurityDefinition("OAuth2", new OpenApiSecurityScheme
    {
        Description = "Please Enter Token",
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });

    // add security information to each operation for OAuth2
    options.OperationFilter<SecurityRequirementsOperationFilter>();

});

// Add my own services to the container
builder.Services.AddScoped<TimeService>();
builder.Services.AddScoped<EmailSender>();
// builder.Services.AddTransient<IEmailSender, EmailSender>();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("BestStoreConnection");
    options.UseSqlServer(connectionString);
});

// add authentication 

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(
                                    builder.Configuration["JwtSettings:key"]!
                                )
                            )
    };
});

// Add services to the container

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// to make share files available in app to appload and download them
app.UseStaticFiles();

// we can use class middleware here
app.UseMiddleware<StatsMiddelware>();

//we can make ower own inline middleware here
// app.Use((context, next) =>
// {
//     // handel the request (befor executing the controller)
//     DateTime requestTime = DateTime.Now;

//     var result = next(context);

//     // handel the request (befor executing the controller)
//     DateTime responseTime = DateTime.Now;
//     TimeSpan timeSpanDuration = responseTime - requestTime;

//     Console.WriteLine("[inline middleware] process Duration = " + timeSpanDuration.TotalMilliseconds + " ms");

//     return result;
// });
//we can make ower own inline middleware here


app.UseHttpsRedirection();

//you can use authentication or take it to shipping automatically when useaddauth

// app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
