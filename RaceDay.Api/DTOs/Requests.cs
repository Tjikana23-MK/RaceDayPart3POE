
using System.ComponentModel.DataAnnotations;

namespace RaceDay.Api.DTOs;

public record RegisterRequest(
    [param: Required(ErrorMessage = "First name is required.")]
    string FirstName,

    [param: Required(ErrorMessage = "Last name is required.")]
    string LastName,

    [param: Required(ErrorMessage = "Email is required.")]
    [param: EmailAddress(ErrorMessage = "Enter a valid email address.")]
    string Email,

    [param: Required(ErrorMessage = "Password is required.")]
    string Password,

    [param: Required(ErrorMessage = "Role is required.")]
    [param: RegularExpression(
        "^(Organiser|Participant)$",
        ErrorMessage = "Role must be Organiser or Participant."
    )]
    string Role,

    string? Phone = null
);

public record LoginRequest(
    [param: Required]
    [param: EmailAddress]
    string Email,

    [param: Required]
    string Password
);

public record ProfileUpdateRequest(
    string FirstName,
    string LastName,

    [param: EmailAddress]
    string? Email,

    string? Phone
);

public record EventRequest(
    [param: Required]
    [param: MaxLength(150)]
    string Name,

    string Description,

    DateTime EventDate,

    [param: Required]
    string Location,

    [param: Range(0.1, 1000)]
    decimal DistanceKm,

    [param: Required]
    [param: RegularExpression(
        "^(Run|Walk|Cycle)$",
        ErrorMessage = "Event type must be Run, Walk or Cycle."
    )]
    string EventType
);

public record CategoryRequest(
    [param: Required]
    [param: MaxLength(80)]
    string Name,

    int? MinimumAge,

    int? MaximumAge,

    decimal? DistanceKm
);
public record EnrolmentRequest(
    int EventId,
    int CategoryId
);

public record ResultRequest(
    TimeSpan FinishTime,

    [param: Range(1, int.MaxValue)]
    int FinishingPosition
);
