using System.Security.Claims;using Microsoft.AspNetCore.Authorization;using Microsoft.AspNetCore.Mvc;using Microsoft.EntityFrameworkCore;using RaceDay.Api.Data;using RaceDay.Api.DTOs;
namespace RaceDay.Api.Controllers;
[ApiController,Authorize,Route("api/profile")]
public class ProfileController(RaceDayDbContext db):ControllerBase{
 int UserId=>int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
 [HttpGet]public async Task<IActionResult> Get(){var u=await db.Users.Where(x=>x.UserId==UserId).Select(x=>new{x.UserId,x.FirstName,x.LastName,x.Email,x.Phone,x.Role,x.ProfileImageUrl,x.CreatedAt}).FirstOrDefaultAsync();return u==null?NotFound():Ok(u);}
 [HttpPut]public async Task<IActionResult> Update(ProfileUpdateRequest r){var u=await db.Users.FindAsync(UserId);if(u==null)return NotFound();var email=(r.Email??u.Email).Trim().ToLowerInvariant();if(await db.Users.AnyAsync(x=>x.Email==email&&x.UserId!=UserId))return Conflict(new{message="Email is already in use."});u.FirstName=r.FirstName.Trim();u.LastName=r.LastName.Trim();u.Email=email;u.Phone=r.Phone?.Trim();await db.SaveChangesAsync();return Ok(new{u.UserId,u.FirstName,u.LastName,u.Email,u.Phone,u.Role,u.ProfileImageUrl});}
}