using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Models;

namespace RaceDay.Api.Data;

public class RaceDayDbContext(
    DbContextOptions<RaceDayDbContext> options)
    : DbContext(options)
{
    // ==========================================
    // DATABASE TABLES
    // ==========================================

    public DbSet<User> Users => Set<User>();

    public DbSet<EventType> EventTypes => Set<EventType>();

    public DbSet<Event> Events => Set<Event>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<Enrolment> Enrolments => Set<Enrolment>();

    public DbSet<Result> Results => Set<Result>();


    // ==========================================
    // DATABASE RELATIONSHIPS AND CONFIGURATION
    // ==========================================

    protected override void OnModelCreating(ModelBuilder b)
    {
        base.OnModelCreating(b);


        // ======================================
        // USER TABLE
        // ======================================

        b.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        b.Entity<User>()
            .Property(x => x.Role)
            .HasDefaultValue("Participant");

        b.Entity<User>()
            .Property(x => x.ProfileImageUrl)
            .HasColumnName("ProfilePictureUrl");


        // ======================================
        // EVENT TABLE
        // ======================================

        b.Entity<Event>()
            .Property(x => x.BannerUrl)
            .HasColumnName("BannerImageUrl");

        b.Entity<Event>()
            .Property(x => x.EventType)
            .HasColumnName("EventType")
            .IsRequired();

        b.Entity<Event>()
            .Property(x => x.EventDate)
            .HasColumnType("date");

        b.Entity<Event>()
            .Property(x => x.CreatedAt)
            .HasColumnType("datetime2");

        b.Entity<Event>()
            .Property(x => x.DistanceKm)
            .HasPrecision(8, 2);

        // One organiser can organise many events.
        b.Entity<Event>()
            .HasOne(x => x.Organiser)
            .WithMany(x => x.OrganisedEvents)
            .HasForeignKey(x => x.OrganiserId)
            .OnDelete(DeleteBehavior.Restrict);


        // ======================================
        // CATEGORY TABLE
        // ======================================

        b.Entity<Category>()
            .Property(x => x.Name)
            .HasColumnName("CategoryName");

        b.Entity<Category>()
            .Property(x => x.MinimumAge)
            .HasColumnName("MinAge");

        b.Entity<Category>()
            .Property(x => x.MaximumAge)
            .HasColumnName("MaxAge");

        b.Entity<Category>()
            .Property(x => x.DistanceKm)
            .HasColumnName("CategoryDistanceKm");

        b.Entity<Category>()
            .Property(x => x.DistanceKm)
            .HasPrecision(8, 2);

        // One event can have many categories.
        b.Entity<Category>()
            .HasOne(x => x.Event)
            .WithMany(x => x.Categories)
            .HasForeignKey(x => x.EventId)
            .OnDelete(DeleteBehavior.Cascade);


        // ======================================
        // ENROLMENT TABLE
        // ======================================

        b.Entity<Enrolment>()
            .HasIndex(x => new
            {
                x.ParticipantId,
                x.EventId
            })
            .IsUnique();

        // One participant can have many enrolments.
        b.Entity<Enrolment>()
            .HasOne(x => x.Participant)
            .WithMany(x => x.Enrolments)
            .HasForeignKey(x => x.ParticipantId)
            .OnDelete(DeleteBehavior.Restrict);

        // One category can have many enrolments.
        b.Entity<Enrolment>()
            .HasOne(x => x.Category)
            .WithMany(x => x.Enrolments)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);


        // ======================================
        // RESULT TABLE
        // ======================================

        b.Entity<Result>()
            .HasIndex(x => x.EnrolmentId)
            .IsUnique();

        // One enrolment can have one result.
        b.Entity<Result>()
            .HasOne(x => x.Enrolment)
            .WithOne(x => x.Result)
            .HasForeignKey<Result>(x => x.EnrolmentId)
            .OnDelete(DeleteBehavior.Cascade);


        // ======================================
        // EVENT TYPES TABLE
        // ======================================

        b.Entity<EventType>()
            .HasData(
                new EventType
                {
                    EventTypeId = 1,
                    Name = "Run"
                },

                new EventType
                {
                    EventTypeId = 2,
                    Name = "Walk"
                },

                new EventType
                {
                    EventTypeId = 3,
                    Name = "Cycle"
                }
            );
    }
}