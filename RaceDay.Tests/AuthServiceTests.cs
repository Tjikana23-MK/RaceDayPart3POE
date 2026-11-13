using Microsoft.EntityFrameworkCore; using RaceDay.Api.Data; using RaceDay.Api.DTOs; using RaceDay.Api.Models; using RaceDay.Api.Services; using Xunit;
namespace RaceDay.Tests;
public class AuthServiceTests {
 private static (RaceDayDbContext Db,AuthService Service) Setup(){var options=new DbContextOptionsBuilder<RaceDayDbContext>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;var db=new RaceDayDbContext(options);return(db,new AuthService(db));}
 [Fact] public async Task Register_ValidParticipant_StoresHashedPassword(){var(db,s)=Setup();var (u,error)=await s.Register(new RegisterRequest("Test Runner","runner@example.com","StrongPass123!",Roles.Participant,"0710000000"));Assert.Null(error);Assert.NotNull(u);Assert.NotEqual("StrongPass123!",u!.PasswordHash);Assert.True(BCrypt.Net.BCrypt.Verify("StrongPass123!",u.PasswordHash));}
 [Fact] public async Task Register_InvalidRole_IsRejected(){var(_,s)=Setup();var(u,error)=await s.Register(new RegisterRequest("Test User","test@example.com","StrongPass123!","Admin",null));Assert.Null(u);Assert.NotNull(error);}
 [Fact] public async Task Register_DuplicateEmail_IsRejected(){var(_,s)=Setup();var r=new RegisterRequest("Test User","same@example.com","StrongPass123!",Roles.Participant,null);Assert.Null((await s.Register(r)).Error);var second=await s.Register(r);Assert.NotNull(second.Error);}
 [Fact] public async Task Login_ValidCredentials_Succeeds_AndWrongPasswordFails(){var(_,s)=Setup();await s.Register(new RegisterRequest("Test User","login@example.com","StrongPass123!",Roles.Organiser,null));Assert.NotNull(await s.Validate(new LoginRequest("login@example.com","StrongPass123!")));Assert.Null(await s.Validate(new LoginRequest("login@example.com","wrong-password")));}
 [Fact] public void SupportedRoles_AreOnlyOrganiserAndParticipant(){Assert.Equal("Organiser",Roles.Organiser);Assert.Equal("Participant",Roles.Participant);Assert.NotEqual(Roles.Organiser,"Admin");}
}
