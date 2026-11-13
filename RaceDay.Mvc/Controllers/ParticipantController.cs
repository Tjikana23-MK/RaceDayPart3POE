using Microsoft.AspNetCore.Mvc;using RaceDay.Mvc.Filters;using RaceDay.Mvc.Services;
namespace RaceDay.Mvc.Controllers;
[RequireRole("Participant")]public class ParticipantController(RaceDayApiClient api):Controller{
 public async Task<IActionResult> Index(){ViewBag.Events=await api.Get("api/events?from="+DateTime.UtcNow.ToString("O"));return View();}
 public async Task<IActionResult> Details(int id){ViewBag.Event=await api.Get($"api/events/{id}");return View();}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> Enrol(int eventId,int categoryId){var r=await api.Post("api/enrolments",new{eventId,categoryId});TempData[r.Ok?"Success":"Error"]=r.Message;return RedirectToAction(nameof(Details),new{id=eventId});}
 public async Task<IActionResult> MyEvents(){ViewBag.Items=await api.Get("api/enrolments/mine");return View();}
 public async Task<IActionResult> Results(){ViewBag.Items=await api.Get("api/results/mine");return View();}
 public async Task<IActionResult> Profile(){ViewBag.Profile=await api.Get("api/profile");return View();}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> UpdateProfile(string firstName,string lastName,string? email,string? phone){var r=await api.Put("api/profile",new{firstName,lastName,email,phone});TempData[r.Ok?"Success":"Error"]=r.Message;return RedirectToAction(nameof(Profile));}
 [HttpPost,ValidateAntiForgeryToken]public async Task<IActionResult> UploadProfile(IFormFile file){if(file==null||file.Length==0){TempData["Error"]="Choose an image first.";return RedirectToAction(nameof(Profile));}var r=await api.Upload("api/profile/image",file);TempData[r.Ok?"Success":"Error"]=r.Message;return RedirectToAction(nameof(Profile));}
}