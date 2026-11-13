using System.ComponentModel.DataAnnotations;
namespace RaceDay.Api.Models;
public static class Roles { public const string Organiser = "Organiser"; public const string Participant = "Participant"; }
public class User { public int UserId { get; set; } [Required, MaxLength(100)] public string FirstName { get; set; } = ""; public string LastName { get; set; } = ""; [Required, EmailAddress, MaxLength(256)] public string Email { get; set; } = ""; [Required] public string PasswordHash { get; set; } = ""; [Required, MaxLength(20)] public string Role { get; set; } = Roles.Participant; [MaxLength(20)] public string? Phone { get; set; } public DateTime CreatedAt { get; set; } = DateTime.UtcNow; public string? ProfileImageUrl { get; set; } public ICollection<Event> OrganisedEvents { get; set; } = new List<Event>(); public ICollection<Enrolment> Enrolments { get; set; } = new List<Enrolment>(); }
public class EventType
{
    public int EventTypeId { get; set; }

    public string Name { get; set; } = "";
}

public class Event
{
    public int EventId { get; set; }

    [Required, MaxLength(150)]
    public string Name { get; set; } = "";

    [MaxLength(2000)]
    public string Description { get; set; } = "";

    public DateTime EventDate { get; set; }

    public string Location { get; set; } = "";

    public decimal DistanceKm { get; set; }

    public string? BannerUrl { get; set; }

    // Event type is stored as text in the database.
    [Required]
    public string EventType { get; set; } = "";

    public int OrganiserId { get; set; }

    public User? Organiser { get; set; }

    // Matches the CreatedAt column in SQL Server.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Category> Categories { get; set; }
        = new List<Category>();
}
public class Category
{
    public int CategoryId { get; set; }

    public int EventId { get; set; }

    public Event? Event { get; set; }

    [Required, MaxLength(80)]
    public string Name { get; set; } = "";

    public int? MinimumAge { get; set; }

    public int? MaximumAge { get; set; }

    public decimal? DistanceKm { get; set; }

    public ICollection<Enrolment> Enrolments { get; set; }
        = new List<Enrolment>();
}
public class Enrolment { public int EnrolmentId { get; set; } public int ParticipantId { get; set; } public User? Participant { get; set; } public int EventId { get; set; } public Event? Event { get; set; } public int CategoryId { get; set; } public Category? Category { get; set; } public DateTime EnrolledAt { get; set; } = DateTime.UtcNow; [MaxLength(20)] public string Status { get; set; } = "Confirmed"; public Result? Result { get; set; } }
public class Result { public int ResultId { get; set; } public int EnrolmentId { get; set; } public Enrolment? Enrolment { get; set; } public TimeSpan FinishTime { get; set; } public int FinishingPosition { get; set; } public DateTime RecordedAt { get; set; } = DateTime.UtcNow; }
