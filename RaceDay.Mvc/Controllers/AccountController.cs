using Microsoft.AspNetCore.Mvc;using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
public class AccountController(RaceDayApiClient api):Controller{
 [HttpGet]public IActionResult Login(string? returnUrl=null){ViewBag.ReturnUrl=returnUrl;return View();}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Login(string email,string password,string? returnUrl){var r=await api.Login(email,password);if(!r.Ok){ViewBag.Error=r.Message;ViewBag.ReturnUrl=returnUrl;return View();}return LocalRedirect(Url.IsLocalUrl(returnUrl)?returnUrl!:(HttpContext.Session.GetString("Role")=="Organiser"?"/Organiser":"/Participant"));}
 [HttpGet]public IActionResult Register()=>View();
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Register(string firstName,string lastName,string email,string password,string role,string? phone){if(password.Length<8){ViewBag.Error="Password must contain at least 8 characters.";return View();}var r=await api.Register(new{firstName,lastName,email,password,role,phone});if(!r.Ok){ViewBag.Error=r.Message;return View();}TempData["Success"]=r.Message;return RedirectToAction(nameof(Login));}
 public async Task<IActionResult> Logout(){await api.Logout();return RedirectToAction(nameof(Index),"Home");}
}