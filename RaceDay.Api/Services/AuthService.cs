using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data; using RaceDay.Api.DTOs; using RaceDay.Api.Models;
namespace RaceDay.Api.Services;
public class AuthService(RaceDayDbContext db) {
 public async Task<(User? User,string? Error)> Register(RegisterRequest r) {
        var role = r.Role.Trim();

        if (role != Roles.Organiser && role != Roles.Participant)
        {
            return (null, "Role must be Organiser or Participant.");
        }       
  var email=r.Email.Trim().ToLowerInvariant(); if(await db.Users.AnyAsync(u=>u.Email==email)) return (null,"An account with this email already exists.");
  var u=new User{FirstName=r.FirstName.Trim(), LastName = r.LastName.Trim(), Email=email,PasswordHash=BCrypt.Net.BCrypt.HashPassword(r.Password),Role=role,Phone=r.Phone?.Trim()}; db.Users.Add(u); await db.SaveChangesAsync(); return (u,null);
 }
 public async Task<User?> Validate(LoginRequest r) { var u=await db.Users.SingleOrDefaultAsync(x=>x.Email==r.Email.Trim().ToLowerInvariant()); return u!=null && BCrypt.Net.BCrypt.Verify(r.Password,u.PasswordHash)?u:null; }
}
