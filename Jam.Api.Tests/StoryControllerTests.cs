using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

using Jam.Api.Controllers;
using Jam.Api.DAL.StoryDAL;
using Jam.Api.DAL.IntroSceneDAL;
using Jam.Api.DAL.QuestionSceneDAL;
using Jam.Api.DAL.EndingSceneDAL;
using Jam.Api.Services;

using Jam.Api.Models;
using Jam.Api.DTOs;
using Jam.Api.DTOs.IntroScenes;
using Jam.Api.DTOs.QuestionScenes;
using Jam.Api.DTOs.EndingScenes;
using Jam.Api.DTOs.Shared;

public class StoryControllerTests
{
    // =====================================================================
    // INTRO SCENE TESTS  (2 TESTS)
    // =====================================================================

    [Fact]
    public async Task GetIntro_ReturnsOk_WhenIntroExists()
    {
        var intro = new IntroScene { StoryId = 1, IntroText = "Math Universe" };

        // Mocks for Repositories
        var mockIntroSceneRepo = new Mock<IIntroSceneRepository>();
        var mockQuestionSceneRepo = new Mock<IQuestionSceneRepository>();
        var mockEndingSceneRepo = new Mock<IEndingSceneRepository>();
        var mockStoryRepo = new Mock<IStoryRepository>();

        // Mocks for Services 
        var mockStoryCodeService = new Mock<IStoryCodeService>();
        var mockStoryEditingService = new Mock<IStoryEditingService>();
        var mockLogger = new Mock<ILogger<StoryEditingController>>();

        // Setup the one action we care about for this test
        mockIntroSceneRepo.Setup(s => s.GetIntroSceneByStoryId(1)).ReturnsAsync(intro);

        var controller = new StoryEditingController(
            mockStoryRepo.Object,
            mockIntroSceneRepo.Object,
            mockQuestionSceneRepo.Object,
            mockEndingSceneRepo.Object,
            mockStoryCodeService.Object,
            mockStoryEditingService.Object,
            mockLogger.Object
        );

        var result = await controller.GetIntro(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsType<IntroSceneDto>(ok.Value);

        Assert.Equal(1, dto.StoryId);
        Assert.Equal("Math Universe", dto.IntroText);
    }

    [Fact]
    public async Task GetIntro_ReturnsNotFound_WhenIntroDoesNotExist()
    {
        // Mocks for Repositories
        var mockIntroSceneRepo = new Mock<IIntroSceneRepository>();
        var mockQuestionSceneRepo = new Mock<IQuestionSceneRepository>();
        var mockEndingSceneRepo = new Mock<IEndingSceneRepository>();
        var mockStoryRepo = new Mock<IStoryRepository>();

        // Mocks for Services (Best Practice: Mock these too)
        var mockStoryCodeService = new Mock<IStoryCodeService>();
        var mockStoryEditingService = new Mock<IStoryEditingService>();
        var mockLogger = new Mock<ILogger<StoryEditingController>>();

        // Setup the one action we care about for this test
        mockIntroSceneRepo.Setup(s => s.GetIntroSceneByStoryId(1)).ReturnsAsync((IntroScene?)null);

        var controller = new StoryEditingController(
            mockStoryRepo.Object,
            mockIntroSceneRepo.Object,
            mockQuestionSceneRepo.Object,
            mockEndingSceneRepo.Object,
            mockStoryCodeService.Object,
            mockStoryEditingService.Object,
            mockLogger.Object
        );

        var result = await controller.GetIntro(1);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var err = Assert.IsType<ErrorDto>(notFound.Value);

        Assert.Equal("Intro scene not found.", err.ErrorTitle);
    }

    // =====================================================================
    // QUESTIONS TESTS (2 TEST)
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

        // Mocks for Repositories
        var mockIntroSceneRepo = new Mock<IIntroSceneRepository>();
        var mockQuestionSceneRepo = new Mock<IQuestionSceneRepository>();
        var mockEndingSceneRepo = new Mock<IEndingSceneRepository>();
        var mockStoryRepo = new Mock<IStoryRepository>();

        // Mocks for Services (Best Practice: Mock these too)
        var mockStoryCodeService = new Mock<IStoryCodeService>();
        var mockStoryEditingService = new Mock<IStoryEditingService>();
        var mockLogger = new Mock<ILogger<StoryEditingController>>();

        // Setup the one action we care about for this test
        mockStoryEditingService.Setup(s => s.UpdateQuestionScenesAsync(
            It.IsAny<int>(),
            It.IsAny<List<UpdateQuestionSceneDto>>()
        )).ReturnsAsync(true);

        var controller = new StoryEditingController(
            mockStoryRepo.Object,
            mockIntroSceneRepo.Object,
            mockQuestionSceneRepo.Object,
            mockEndingSceneRepo.Object,
            mockStoryCodeService.Object,
            mockStoryEditingService.Object,
            mockLogger.Object
        );

        var result = await controller.UpdateQuestions(1, payload);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task UpdateQuestions_ReturnsBadRequest_WhenModelStateInvalid()
    {
        // Mocks for Repositories
        var mockIntroSceneRepo = new Mock<IIntroSceneRepository>();
        var mockQuestionSceneRepo = new Mock<IQuestionSceneRepository>();
        var mockEndingSceneRepo = new Mock<IEndingSceneRepository>();
        var mockStoryRepo = new Mock<IStoryRepository>();

        // Mocks for Services (Best Practice: Mock these too)
        var mockStoryCodeService = new Mock<IStoryCodeService>();
        var mockStoryEditingService = new Mock<IStoryEditingService>();
        var mockLogger = new Mock<ILogger<StoryEditingController>>();

        var controller = new StoryEditingController(
            mockStoryRepo.Object,
            mockIntroSceneRepo.Object,
            mockQuestionSceneRepo.Object,
            mockEndingSceneRepo.Object,
            mockStoryCodeService.Object,
            mockStoryEditingService.Object,
            mockLogger.Object
        );

        controller.ModelState.AddModelError("StoryText", "Required");

        var result = await controller.UpdateQuestions(1, new List<UpdateQuestionSceneDto>());

        Assert.IsType<BadRequestObjectResult>(result);
    }
}
