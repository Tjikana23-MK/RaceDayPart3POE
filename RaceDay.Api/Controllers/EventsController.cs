using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController(RaceDayDbContext db) : ControllerBase
{
    // Gets the logged-in user's ID from their authentication token.
    private int UserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);


    // =========================================================
    // GET: api/events
    // Gets all events.
    // Optional filters: search and from.
    // =========================================================

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get(
        [FromQuery] string? search,
        [FromQuery] DateTime? from)
    {
        var events = await db.Events
            .Include(e => e.Categories)
            .Where(e =>
                (string.IsNullOrWhiteSpace(search) ||
                 e.Name.Contains(search) ||
                 e.Location.Contains(search))
                &&
                (from == null || e.EventDate >= from))
            .OrderBy(e => e.EventDate)
            .Select(e => new
            {
                e.EventId,
                e.Name,
                e.Description,
                e.EventDate,
                e.Location,
                e.DistanceKm,
                e.BannerUrl,
                e.EventType,
                e.OrganiserId,

                Categories = e.Categories.Select(c => new
                {
                    c.CategoryId,
                    c.Name,
                    c.DistanceKm,
                    c.MinimumAge,
                    c.MaximumAge
                })
            })
            .ToListAsync();

        return Ok(events);
    }


    // =========================================================
    // GET: api/events/5
    // Gets one event using its ID.
    // =========================================================

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var eventDetails = await db.Events
            .Include(e => e.Categories)
            .Where(e => e.EventId == id)
            .Select(e => new
            {
                e.EventId,
                e.Name,
                e.Description,
                e.EventDate,
                e.Location,
                e.DistanceKm,
                e.BannerUrl,
                e.EventType,
                e.OrganiserId,

                Categories = e.Categories.Select(c => new
                {
                    c.CategoryId,
                    c.Name,
                    c.DistanceKm,
                    c.MinimumAge,
                    c.MaximumAge
                })
            })
            .FirstOrDefaultAsync();

        if (eventDetails == null)
        {
            return NotFound(new
            {
                message = "Event not found."
            });
        }

        return Ok(eventDetails);
    }


    // =========================================================
    // POST: api/events
    // Creates a new event.
    // Only Organisers can create events.
    // =========================================================

    [HttpPost]
    [Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> Create(EventRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var newEvent = new Event
        {
            Name = request.Name.Trim(),
            Description = request.Description,
            EventDate = request.EventDate,
            Location = request.Location.Trim(),
            DistanceKm = request.DistanceKm,
            EventType = request.EventType.Trim(),
            OrganiserId = UserId
        };

        db.Events.Add(newEvent);

        await db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(GetById),
            new { id = newEvent.EventId },
            newEvent
        );
    }


    // =========================================================
    // PUT: api/events/5
    // Updates an existing event.
    // Only the organiser who created the event can update it.
    // =========================================================

    [HttpPut("{id:int}")]
    [Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> Update(
        int id,
        EventRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingEvent = await db.Events
            .FirstOrDefaultAsync(e => e.EventId == id);

        if (existingEvent == null)
        {
            return NotFound(new
            {
                message = "Event not found."
            });
        }

        // Check whether the logged-in organiser owns this event.
        if (existingEvent.OrganiserId != UserId)
        {
            return Forbid();
        }

        // Update event details.
        existingEvent.Name = request.Name.Trim();
        existingEvent.Description = request.Description;
        existingEvent.EventDate = request.EventDate;
        existingEvent.Location = request.Location.Trim();
        existingEvent.DistanceKm = request.DistanceKm;
        existingEvent.EventType = request.EventType.Trim();

        await db.SaveChangesAsync();

        return Ok(new
        {
            message = "Event updated successfully.",
            eventDetails = existingEvent
        });
    }


    // =========================================================
    // DELETE: api/events/5
    // Deletes an event.
    // Only the organiser who created the event can delete it.
    // Events with enrolments cannot be deleted.
    // =========================================================

    [HttpDelete("{id:int}")]
    [Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> Delete(int id)
    {
        var existingEvent = await db.Events
            .Include(e => e.Categories)
            .ThenInclude(c => c.Enrolments)
            .FirstOrDefaultAsync(e => e.EventId == id);

        if (existingEvent == null)
        {
            return NotFound(new
            {
                message = "Event not found."
            });
        }

        // Check whether the logged-in organiser owns this event.
        if (existingEvent.OrganiserId != UserId)
        {
            return Forbid();
        }

        // Prevent deleting events that already have enrolments.
        if (existingEvent.Categories.Any(c => c.Enrolments.Any()))
        {
            return Conflict(new
            {
                message = "Cannot delete an event that has existing enrolments."
            });
        }

        db.Events.Remove(existingEvent);

        await db.SaveChangesAsync();

        return Ok(new
        {
            message = "Event deleted successfully."
        });
    }
}