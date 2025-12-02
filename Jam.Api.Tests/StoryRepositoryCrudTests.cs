using Jam.Api.DAL;
using Jam.Api.DAL.StoryDAL;
using Jam.Api.Models;
using Jam.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

public class StoryRepositoryCrudTests
{
    private StoryRepository CreateRepository(out StoryDbContext db)
    {
        var options = new DbContextOptionsBuilder<StoryDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        db = new StoryDbContext(options);
        return new StoryRepository(db, NullLogger<StoryRepository>.Instance);
    }

    private static Story BuildStory(string title = "Test Story") =>
        new()
        {
            Title = title,
            Description = "Desc",
            DifficultyLevel = DifficultyLevel.Easy,
            Accessibility = Accessibility.Public,
            IntroScene = new IntroScene { IntroText = "Intro" }
        };

    // CREATE
    [Fact]
    public async Task AddStory_ReturnsTrue_WhenValid()
    {
        var repo = CreateRepository(out var db);
        var story = BuildStory();

        var result = await repo.AddStory(story);

        Assert.True(result);
        Assert.Equal(1, await db.Stories.CountAsync());
    }

    [Fact]
    public async Task AddStory_ReturnsFalse_WhenNull()
    {
        var repo = CreateRepository(out var _);

        var result = await repo.AddStory(null!);

        Assert.False(result);
    }

    // READ
    [Fact]
    public async Task GetStoryById_ReturnsStory_WhenExists()
    {
        var repo = CreateRepository(out var db);
        var story = BuildStory();
        db.Stories.Add(story);
        await db.SaveChangesAsync();

        var found = await repo.GetStoryById(story.StoryId);

        Assert.NotNull(found);
        Assert.Equal(story.Title, found!.Title);
    }

    [Fact]
    public async Task GetStoryById_ReturnsNull_WhenInvalidId()
    {
        var repo = CreateRepository(out var _);

        var found = await repo.GetStoryById(-1);

        Assert.Null(found);
    }

    // UPDATE
    [Fact]
    public async Task UpdateStory_ReturnsTrue_WhenExisting()
    {
        var repo = CreateRepository(out var db);
        var story = BuildStory();
        db.Stories.Add(story);
        await db.SaveChangesAsync();

        story.Title = "Updated";
        var result = await repo.UpdateStory(story);

        Assert.True(result);
        Assert.Equal("Updated", (await db.Stories.FirstAsync()).Title);
    }

    [Fact]
    public async Task UpdateStory_ReturnsFalse_WhenNull()
    {
        var repo = CreateRepository(out var _);

        var result = await repo.UpdateStory(null!);

        Assert.False(result);
    }

    // DELETE
    [Fact]
    public async Task DeleteStory_ReturnsTrue_WhenExisting()
    {
        var repo = CreateRepository(out var db);
        var story = BuildStory();
        db.Stories.Add(story);
        await db.SaveChangesAsync();

        var result = await repo.DeleteStory(story.StoryId);

        Assert.True(result);
        Assert.Empty(db.Stories);
    }

    [Fact]
    public async Task DeleteStory_ReturnsFalse_WhenInvalidId()
    {
        var repo = CreateRepository(out var _);

        var result = await repo.DeleteStory(0);

        Assert.False(result);
    }
}
