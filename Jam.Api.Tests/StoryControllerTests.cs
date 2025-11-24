using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

using Jam.Api.Controllers;
using Jam.DAL.StoryDAL;
using Jam.DAL.SceneDAL;
using Jam.Api.Services;

using Jam.Models;
using Jam.DTOs;
using Jam.DTOs.IntroScenes;
using Jam.DTOs.QuestionScenes;
using Jam.DTOs.UpdateEndingScenes;

public class StoryControllerTests
{
    // =====================================================================
    // INTRO SCENE TESTS  (2 TESTER)
    // =====================================================================

    [Fact]
    public async Task GetIntro_ReturnsOk_WhenIntroExists()
    {
        var intro = new IntroScene { StoryId = 1, IntroText = "Math Universe" };

        var mockScene = new Mock<ISceneRepository>();
        mockScene.Setup(s => s.GetIntroSceneByStoryId(1)).ReturnsAsync(intro);

        var mockStoryRepo = new Mock<IStoryRepository>();

        var controller = new StoryEditingController(
            mockStoryRepo.Object,
            mockScene.Object,
            new StoryCodeService(mockStoryRepo.Object),
            new Mock<ILogger<StoryEditingController>>().Object);

        var result = await controller.GetIntro(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<IntroSceneDto>(ok.Value);

        Assert.Equal(1, dto.StoryId);
        Assert.Equal("Math Universe", dto.IntroText);
    }

    [Fact]
    public async Task GetIntro_ReturnsNotFound_WhenIntroDoesNotExist()
    {
        var mockScene = new Mock<ISceneRepository>();
        mockScene.Setup(s => s.GetIntroSceneByStoryId(1)).ReturnsAsync((IntroScene?)null);

        var mockStoryRepo = new Mock<IStoryRepository>();
        var controller = new StoryEditingController(
            mockStoryRepo.Object,
            mockScene.Object,
            new StoryCodeService(mockStoryRepo.Object),
            new Mock<ILogger<StoryEditingController>>().Object);

        var result = await controller.GetIntro(1);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var err = Assert.IsType<ErrorDto>(notFound.Value);

        Assert.Equal("Intro scene not found.", err.ErrorTitle);
    }

    // =====================================================================
    // QUESTIONS TESTS (2 TESTER)
    // =====================================================================

    [Fact]
    public async Task UpdateQuestions_ReturnsOk_WhenValid()
    {
        var payload = new List<UpdateQuestionSceneDto>
        {
            new UpdateQuestionSceneDto
            {
                QuestionSceneId = 10,
                StoryText = "Story",
                QuestionText = "2+2?",
                CorrectAnswerIndex = 1,
                Answers =
                {
                    new AnswerOptionDto(), new AnswerOptionDto(),
                    new AnswerOptionDto(), new AnswerOptionDto(),
                }
            }
        };

        var mockScene = new Mock<ISceneRepository>();
        mockScene.Setup(r => r.UpdateQuestionScenes(It.IsAny<IEnumerable<QuestionScene>>()))
                 .ReturnsAsync(true);

        var mockStoryRepo = new Mock<IStoryRepository>();
        var controller = new StoryEditingController(
            mockStoryRepo.Object,
            mockScene.Object,
            new StoryCodeService(mockStoryRepo.Object),
            new Mock<ILogger<StoryEditingController>>().Object);

        var result = await controller.UpdateQuestions(1, payload);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateQuestions_ReturnsBadRequest_WhenModelStateInvalid()
    {
        var mockStoryRepo = new Mock<IStoryRepository>();
        var controller = new StoryEditingController(
            mockStoryRepo.Object,
            new Mock<ISceneRepository>().Object,
            new StoryCodeService(mockStoryRepo.Object),
            new Mock<ILogger<StoryEditingController>>().Object);

        controller.ModelState.AddModelError("StoryText", "Required");

        var result = await controller.UpdateQuestions(1, new List<UpdateQuestionSceneDto>());

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
