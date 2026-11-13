using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c => { c.SwaggerDoc("v1", new() { Title = "RaceDay API", Version = "v1", Description = "RaceDay event management API for Organisers and Participants." }); });
builder.Services.AddDbContext<RaceDayDbContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("RaceDay")));
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options => { options.Cookie.Name = ".RaceDay.Session"; options.Cookie.HttpOnly = true; options.Cookie.IsEssential = true; options.IdleTimeout = TimeSpan.FromMinutes(30); });
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options => { options.Cookie.Name = ".RaceDay.Auth"; options.Cookie.HttpOnly = true; options.Cookie.SameSite = SameSiteMode.Lax; options.LoginPath = "/api/auth/login"; options.Events.OnRedirectToLogin = ctx => { ctx.Response.StatusCode = StatusCodes.Status401Unauthorized; return Task.CompletedTask; }; options.Events.OnRedirectToAccessDenied = ctx => { ctx.Response.StatusCode = StatusCodes.Status403Forbidden; return Task.CompletedTask; }; });
builder.Services.AddAuthorization();
builder.Services.AddScoped<AuthService>();
var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseHttpsRedirection(); app.UseSession(); app.UseAuthentication(); app.UseAuthorization(); app.MapControllers();
app.MapGet("/", () => Results.Ok(new { application = "RaceDay API", status = "running", swagger = "/swagger" })).AllowAnonymous();
app.Run();
public partial class Program { }
