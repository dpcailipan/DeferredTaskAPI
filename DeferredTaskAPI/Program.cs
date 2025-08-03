using DeferredTaskAPI.Data;
using DeferredTaskAPI.Repositories;
using DeferredTaskAPI.Repositories.Interfaces;
using DeferredTaskAPI.Services;
using DeferredTaskAPI.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
    });
    options.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddDbContext<AuthDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("AuthDbConnectionString"))
);
builder.Services.AddAuthentication();
builder.Services.AddIdentityApiEndpoints<IdentityUser>()
    .AddEntityFrameworkStores<AuthDbContext>();

builder.Services.AddDbContext<DeferredTaskDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DeferredTaskDbConnectionString"))
);
builder.Services.AddLogging(builder => builder.AddConsole());
builder.Services.AddScoped<IScheduledTasksRepository, ScheduledTasksRepository>();
builder.Services.AddScoped<IScheduledTaskService, ScheduledTaskService>();
builder.Services.AddHostedService<ScheduledTaskProcessorService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    _ = app.UseSwagger();
    _ = app.UseSwaggerUI();
}

app.MapIdentityApi<IdentityUser>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
