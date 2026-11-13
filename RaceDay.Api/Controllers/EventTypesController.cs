using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore; using RaceDay.Api.Data;
namespace RaceDay.Api.Controllers;
[ApiController,Route("api/event-types")]
public class EventTypesController(RaceDayDbContext db):ControllerBase { [HttpGet,AllowAnonymous] public async Task<IActionResult> Get()=>Ok(await db.EventTypes.OrderBy(x=>x.EventTypeId).ToListAsync()); }
