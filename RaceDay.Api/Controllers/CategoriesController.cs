using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.DTOs;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events/{eventId:int}/categories")]
public class CategoriesController(RaceDayDbContext db) : ControllerBase
{
    // Gets the logged-in user's ID.
    private int UserId =>
        int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);


    // =====================================================
    // GET: api/events/1/categories
    // Gets all categories belonging to an event.
    // =====================================================

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Get(int eventId)
    {
        var eventExists = await db.Events
            .AnyAsync(e => e.EventId == eventId);

        if (!eventExists)
        {
            return NotFound(new
            {
                message = "Event not found."
            });
        }

        var categories = await db.Categories
            .Where(c => c.EventId == eventId)
            .Select(c => new
            {
                c.CategoryId,
                c.EventId,
                c.Name,
                c.MinimumAge,
                c.MaximumAge,
                c.DistanceKm
            })
            .ToListAsync();

        return Ok(categories);
    }


    // =====================================================
    // POST: api/events/1/categories
    // Creates a category for an event.
    // Only the event organiser can create categories.
    // =====================================================

    [HttpPost]
    [Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> Create(
        int eventId,
        CategoryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existingEvent = await db.Events
            .FirstOrDefaultAsync(e => e.EventId == eventId);

        if (existingEvent == null)
        {
            return NotFound(new
            {
                message = "Event not found."
            });
        }

        // Only the organiser who created the event
        // is allowed to add categories.
        if (existingEvent.OrganiserId != UserId)
        {
            return Forbid();
        }

        // Check that the age range is valid.
        if (request.MinimumAge.HasValue &&
            request.MaximumAge.HasValue &&
            request.MinimumAge > request.MaximumAge)
        {
            return BadRequest(new
            {
                message = "Minimum age cannot be greater than maximum age."
            });
        }

        var category = new Category
        {
            EventId = eventId,
            Name = request.Name.Trim(),
            MinimumAge = request.MinimumAge,
            MaximumAge = request.MaximumAge,
            DistanceKm = request.DistanceKm
        };

        db.Categories.Add(category);

        await db.SaveChangesAsync();

        return Created(
            $"/api/events/{eventId}/categories/{category.CategoryId}",
            new
            {
                message = "Category created successfully.",
                category.CategoryId,
                category.EventId,
                category.Name,
                category.MinimumAge,
                category.MaximumAge,
                category.DistanceKm
            }
        );
    }


    // =====================================================
    // PUT: api/events/1/categories/2
    // Updates an existing category.
    // Only the event organiser can update it.
    // =====================================================

    [HttpPut("{categoryId:int}")]
    [Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> Update(
        int eventId,
        int categoryId,
        CategoryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var category = await db.Categories
            .Include(c => c.Event)
            .FirstOrDefaultAsync(c =>
                c.EventId == eventId &&
                c.CategoryId == categoryId);

        if (category == null)
        {
            return NotFound(new
            {
                message = "Category not found."
            });
        }

        // Check that the logged-in user owns the event.
        if (category.Event == null ||
            category.Event.OrganiserId != UserId)
        {
            return Forbid();
        }

        // Check that the age range is valid.
        if (request.MinimumAge.HasValue &&
            request.MaximumAge.HasValue &&
            request.MinimumAge > request.MaximumAge)
        {
            return BadRequest(new
            {
                message = "Minimum age cannot be greater than maximum age."
            });
        }

        category.Name = request.Name.Trim();
        category.MinimumAge = request.MinimumAge;
        category.MaximumAge = request.MaximumAge;
        category.DistanceKm = request.DistanceKm;

        await db.SaveChangesAsync();

        return Ok(new
        {
            message = "Category updated successfully.",
            category.CategoryId,
            category.EventId,
            category.Name,
            category.MinimumAge,
            category.MaximumAge,
            category.DistanceKm
        });
    }


    // =====================================================
    // DELETE: api/events/1/categories/2
    // Deletes a category.
    // Categories with enrolments cannot be deleted.
    // =====================================================

    [HttpDelete("{categoryId:int}")]
    [Authorize(Roles = Roles.Organiser)]
    public async Task<IActionResult> Delete(
        int eventId,
        int categoryId)
    {
        var category = await db.Categories
            .Include(c => c.Event)
            .Include(c => c.Enrolments)
            .FirstOrDefaultAsync(c =>
                c.EventId == eventId &&
                c.CategoryId == categoryId);

        if (category == null)
        {
            return NotFound(new
            {
                message = "Category not found."
            });
        }

        // Only the event organiser can delete the category.
        if (category.Event == null ||
            category.Event.OrganiserId != UserId)
        {
            return Forbid();
        }

        // Prevent deleting categories that have enrolments.
        if (category.Enrolments.Any())
        {
            return Conflict(new
            {
                message = "Cannot delete a category with existing enrolments."
            });
        }

        db.Categories.Remove(category);

        await db.SaveChangesAsync();

        return Ok(new
        {
            message = "Category deleted successfully."
        });
    }
}