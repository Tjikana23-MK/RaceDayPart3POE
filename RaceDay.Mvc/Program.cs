using RaceDay.Mvc.Services;
var builder=WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o=>{o.Cookie.Name=".RaceDay.Mvc.Session";o.Cookie.HttpOnly=true;o.Cookie.IsEssential=true;o.IdleTimeout=TimeSpan.FromMinutes(30);});
builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient<RaceDayApiClient>(c=>c.BaseAddress=new Uri(builder.Configuration["ApiSettings:BaseUrl"]??"http://localhost:5000/"));
var app=builder.Build();
if(!app.Environment.IsDevelopment()){app.UseExceptionHandler("/Home/Error");app.UseHsts();}
app.UseHttpsRedirection();app.UseStaticFiles();app.UseRouting();app.UseSession();app.MapControllerRoute(name:"default",pattern:"{controller=Home}/{action=Index}/{id?}");app.Run();