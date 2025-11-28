using Jam.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Jam.Api.DAL;

public class StoryDbContext : DbContext
{
    public StoryDbContext(DbContextOptions<StoryDbContext> options) : base(options)
    {
        // Database.EnsureCreated();
    }

    public DbSet<Story> Stories { get; set; }
    public DbSet<IntroScene> IntroScenes { get; set; }
    public DbSet<QuestionScene> QuestionScenes { get; set; }
    public DbSet<AnswerOption> AnswerOptions { get; set; }
    public DbSet<EndingScene> EndingScenes { get; set; }
    public DbSet<PlayingSession> PlayingSessions { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==========================================================================
        // 1. STORY STRUCTURE RELATIONSHIPS (Cascade Delete for Dependent Content)
        //    If the principal entity is deleted, all dependent entities are deleted
        // ==========================================================================

        // Make owner FK optional (allow UserId = null when user is deleted by admin)
        modelBuilder.Entity<Story>()
            .Property(s => s.UserId)
            .IsRequired(false);

        // Story -> IntroScene (1-to-1): 
        // Deleting a Story automatically deletes its IntroScene
        modelBuilder.Entity<Story>()
            .HasOne(s => s.IntroScene)
            .WithOne(i => i.Story)
            .HasForeignKey<IntroScene>(i => i.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Story -> QuestionScene (1-to-many): 
        // Deleting a Story automatically deletes all its QuestionScenes
        modelBuilder.Entity<Story>()
            .HasMany(s => s.QuestionScenes)
            .WithOne(qs => qs.Story)
            .HasForeignKey(qs => qs.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Story -> EndingScene (1-to-many): 
        // Deleting a Story automatically deletes all its EndingScenes
        modelBuilder.Entity<Story>()
            .HasMany(s => s.EndingScenes)
            .WithOne(es => es.Story)
            .HasForeignKey(es => es.StoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Story -> PlayingSession (1-to-many): 
        // Deleting a Story automatically deletes all associated PlayingSession records (as they are now meaningless)
        modelBuilder.Entity<Story>()
            .HasMany(s => s.PlayingSessions)
            .WithOne(ps => ps.Story)
            .HasForeignKey(ps => ps.StoryId)
            .OnDelete(DeleteBehavior.Cascade);



        // ==========================================================================
        // 2. Question SCENE NAVIGATION RELATIONSHIPS (Restrict Deletion to Preserve Integrity)
        //    If a QuestionScene being referenced is deleted, the referencing Scene's 
        //    NextQuestionSceneId must be manually set to NULL beforehand
        // ==========================================================================

        // QuestionScene -> NextQuestionScene (Self-referencing One-to-One/Zero Relationship):
        modelBuilder.Entity<QuestionScene>()
            .HasOne(qs => qs.NextQuestionScene)
            .WithMany() // no inverse navigation
            .HasForeignKey(qs => qs.NextQuestionSceneId)
            .OnDelete(DeleteBehavior.SetNull);

        // Enforce that NextQuestionSceneId is unique across all Scenes
        modelBuilder.Entity<QuestionScene>()
            .HasIndex(s => s.NextQuestionSceneId)
            .IsUnique();

        // QuestionScene -> AnswerOptions (1-to-many)
        // Deleting a QuestionScene automatically deletes all associated AnswerOptions
        modelBuilder.Entity<QuestionScene>()
            .HasMany(qs => qs.AnswerOptions)
            .WithOne(ao => ao.QuestionScene)
            .HasForeignKey(ao => ao.QuestionSceneId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}