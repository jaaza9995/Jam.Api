using Xunit;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;

using Jam.Api.Controllers;
using Jam.DAL.StoryDAL;
using Jam.DAL.SceneDAL;
using Jam.DAL.AnswerOptionDAL;

using Jam.Models;
using Jam.Models.Enums;

using Jam.DTOs;
using Jam.DTOs.StoryEditing;
using Jam.DTOs.IntroScenes;
using Jam.DTOs.QuestionScenes;
using Jam.DTOs.UpdateEndingScenes;

public class StoryControllerTests
{
    // =====================================================================
    // INTRO SCENE TESTS  (5 TESTER)
    // =====================================================================

    [Fact]
    public async Task GetIntro_ReturnsOk_WhenIntroExists()
    {
        var intro = new IntroScene
        {
            StoryId = 1,
            IntroText = "Math Universe"
        };

        var mockScene = new Mock<ISceneRepository>();
        mockScene.Setup(s => s.GetIntroSceneByStoryId(1)).ReturnsAsync(intro);

        var controller = new StoryEditingController(
            new Mock<IStoryRepository>().Object,
            mockScene.Object,
            new Mock<IAnswerOptionRepository>().Object,
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
        mockScene.Setup(s => s.GetIntroSceneByStoryId(1))
                 .ReturnsAsync((IntroScene?)null);

        var controller = new StoryEditingController(
            new Mock<IStoryRepository>().Object,
            mockScene.Object,
            new Mock<IAnswerOptionRepository>().Object,
            new Mock<ILogger<StoryEditingController>>().Object
        );

        var result = await controller.GetIntro(1);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var err = Assert.IsType<ErrorDto>(notFound.Value);

        Assert.Equal("Intro scene not found.", err.ErrorTitle);
    }

    [Fact]
    public async Task UpdateIntro_ReturnsNotFound_WhenIntroDoesNotExist()
    {
        var mockScene = new Mock<ISceneRepository>();
        mockScene.Setup(s => s.GetIntroSceneByStoryId(1))
                 .ReturnsAsync((IntroScene?)null);

        var controller = new StoryEditingController(
            new Mock<IStoryRepository>().Object,
            mockScene.Object,
            new Mock<IAnswerOptionRepository>().Object,
            new Mock<ILogger<StoryEditingController>>().Object
        );

        var model = new EditIntroSceneDto { IntroText = "New intro" };
        var result = await controller.UpdateIntro(1, model);

        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var body = Assert.IsType<ErrorDto>(notFound.Value);

        Assert.Equal("Intro scene not found.", body.ErrorTitle);
    }

    [Fact]
    public async Task UpdateIntro_ReturnsServerError_WhenUpdateFails()
    {
        var intro = new IntroScene { StoryId = 1, IntroText = "Old intro" };

        var mockScene = new Mock<ISceneRepository>();
        mockScene.Setup(s => s.GetIntroSceneByStoryId(1))
                 .ReturnsAsync(intro);

        mockScene.Setup(s => s.UpdateIntroScene(intro))
                 .ReturnsAsync(false);

        var controller = new StoryEditingController(
            new Mock<IStoryRepository>().Object,
            mockScene.Object,
            new Mock<IAnswerOptionRepository>().Object,
            new Mock<ILogger<StoryEditingController>>().Object
        );

        var model = new EditIntroSceneDto { IntroText = "New intro" };
        var result = await controller.UpdateIntro(1, model);

        var err = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, err.StatusCode);
    }

    [Fact]
    public async Task UpdateIntro_ReturnsOk_WhenUpdateSucceeds()
    {
        var intro = new IntroScene { StoryId = 1, IntroText = "Old intro" };

        var mockScene = new Mock<ISceneRepository>();
        mockScene.Setup(s => s.GetIntroSceneByStoryId(1)).ReturnsAsync(intro);
        mockScene.Setup(s => s.UpdateIntroScene(intro)).ReturnsAsync(true);

        var controller = new StoryEditingController(
            new Mock<IStoryRepository>().Object,
            mockScene.Object,
            new Mock<IAnswerOptionRepository>().Object,
            new Mock<ILogger<StoryEditingController>>().Object
        );

        var model = new EditIntroSceneDto { IntroText = "Updated!" };
        var result = await controller.UpdateIntro(1, model);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("Intro updated successfully", ok.Value.ToString());
    }

    // =====================================================================
    // UPDATE QUESTIONS — (4 TESTER)
    // =====================================================================

    [Fact]
    public async Task UpdateQuestions_ReturnsOk_WhenValid()
    {
        var storyId = 1;

        var payload = new List<UpdateQuestionSceneDto>
        {
            new UpdateQuestionSceneDto
            {
                QuestionSceneId = 10,
                StoryText = "Story text",
                QuestionText = "What is 2+2?",
                CorrectAnswerIndex = 1,
                Answers =
                {
                    new AnswerOptionDto { AnswerOptionId = 1, AnswerText = "1", ContextText = "no" },
                    new AnswerOptionDto { AnswerOptionId = 2, AnswerText = "4", ContextText = "yes" },
                    new AnswerOptionDto { AnswerOptionId = 3, AnswerText = "5", ContextText = "no" },
                    new AnswerOptionDto { AnswerOptionId = 4, AnswerText = "2", ContextText = "no" }
                }
            }
        };

        var mockScene = new Mock<ISceneRepository>();
        mockScene
            .Setup(r => r.UpdateQuestionScenes(It.IsAny<IEnumerable<QuestionScene>>()))
            .ReturnsAsync(true);

        var controller = new StoryEditingController(
            new Mock<IStoryRepository>().Object,
            mockScene.Object,
            new Mock<IAnswerOptionRepository>().Object,
            new Mock<ILogger<StoryEditingController>>().Object
        );

        var result = await controller.UpdateQuestions(storyId, payload);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Contains("Questions updated successfully", ok.Value.ToString());
    }

    [Fact]
    public async Task UpdateQuestions_DeletesScenesMarkedForDeletion()
    {
        var storyId = 1;

        var payload = new List<UpdateQuestionSceneDto>
        {
            new UpdateQuestionSceneDto
            {
                QuestionSceneId = 50,
                MarkedForDeletion = true,
                StoryText = "xxx",
                QuestionText = "xxx",
                CorrectAnswerIndex = 0,
                Answers =
                {
                    new AnswerOptionDto(), new AnswerOptionDto(),
                    new AnswerOptionDto(), new AnswerOptionDto(),
                }
            }
        };

        var mockScene = new Mock<ISceneRepository>();

        mockScene.Setup(r => r.DeleteQuestionScene(50, null)).ReturnsAsync(true);
        mockScene.Setup(r => r.UpdateQuestionScenes(It.IsAny<IEnumerable<QuestionScene>>()))
                 .ReturnsAsync(true);

        var controller = new StoryEditingController(
            new Mock<IStoryRepository>().Object,
            mockScene.Object,
            new Mock<IAnswerOptionRepository>().Object,
            new Mock<ILogger<StoryEditingController>>().Object
        );

        var result = await controller.UpdateQuestions(storyId, payload);

        mockScene.Verify(r => r.DeleteQuestionScene(50, null), Times.Once());
        mockScene.Verify(r => r.UpdateQuestionScenes(It.IsAny<IEnumerable<QuestionScene>>()), Times.Once());
        Assert.IsType<OkObjectResult>(result);
    }

    // ❗ NEW TEST #1 — UpdateQuestions returns SERVER ERROR
    [Fact]
    public async Task UpdateQuestions_ReturnsServerError_WhenRepositoryFails()
    {
        var storyId = 1;

        var payload = new List<UpdateQuestionSceneDto>
        {
            new UpdateQuestionSceneDto
            {
                QuestionSceneId = 10,
                StoryText = "Test",
                QuestionText = "Test?",
                CorrectAnswerIndex = 0,
                Answers =
                {
                    new AnswerOptionDto(), new AnswerOptionDto(),
                    new AnswerOptionDto(), new AnswerOptionDto(),
                }
            }
        };

        var mockScene = new Mock<ISceneRepository>();
        mockScene.Setup(r => r.UpdateQuestionScenes(It.IsAny<IEnumerable<QuestionScene>>()))
                 .ReturnsAsync(false);

        var controller = new StoryEditingController(
            new Mock<IStoryRepository>().Object,
            mockScene.Object,
            new Mock<IAnswerOptionRepository>().Object,
            new Mock<ILogger<StoryEditingController>>().Object
        );

        var result = await controller.UpdateQuestions(storyId, payload);

        var error = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, error.StatusCode);
    }

    // ❗ NEW TEST #2 — UpdateQuestions returns BadRequest when ModelState invalid
    [Fact]
    public async Task UpdateQuestions_ReturnsBadRequest_WhenModelStateInvalid()
    {
        var controller = new StoryEditingController(
            new Mock<IStoryRepository>().Object,
            new Mock<ISceneRepository>().Object,
            new Mock<IAnswerOptionRepository>().Object,
            new Mock<ILogger<StoryEditingController>>().Object
        );

        controller.ModelState.AddModelError("StoryText", "Required");

        var result = await controller.UpdateQuestions(1, new List<UpdateQuestionSceneDto>());

        Assert.IsType<BadRequestObjectResult>(result);
    }

    // =====================================================================
    // ENDINGS TESTS — (3 TESTER)
    // =====================================================================

    [Fact]
    public async Task GetEndings_ReturnsOk_WhenEndingsExist()
    {
        var endings = new List<EndingScene>
        {
            new EndingScene { StoryId = 1, EndingType = EndingType.Good, EndingText = "Good" },
            new EndingScene { StoryId = 1, EndingType = EndingType.Neutral, EndingText = "Neutral" },
            new EndingScene { StoryId = 1, EndingType = EndingType.Bad, EndingText = "Bad" },
        };

        var mockScene = new Mock<ISceneRepository>();
        mockScene.Setup(r => r.GetEndingScenesByStoryId(1)).ReturnsAsync(endings);

        var controller = new StoryEditingController(
            new Mock<IStoryRepository>().Object,
            mockScene.Object,
            new Mock<IAnswerOptionRepository>().Object,
            new Mock<ILogger<StoryEditingController>>().Object
        );

        var result = await controller.GetEndings(1);

        var ok = Assert.IsType<OkObjectResult>(result);
        var dto = Assert.IsAssignableFrom<UpdateEndingSceneDto>(ok.Value);

        Assert.Equal("Good", dto.GoodEnding);
        Assert.Equal("Neutral", dto.NeutralEnding);
        Assert.Equal("Bad", dto.BadEnding);
    }

    [Fact]
    public async Task GetEndings_ReturnsNotFound_WhenNoneExist()
    {
        var mockScene = new Mock<ISceneRepository>();
        mockScene.Setup(r => r.GetEndingScenesByStoryId(1))
                 .ReturnsAsync(new List<EndingScene>());

        var controller = new StoryEditingController(
            new Mock<IStoryRepository>().Object,
            mockScene.Object,
            new Mock<IAnswerOptionRepository>().Object,
            new Mock<ILogger<StoryEditingController>>().Object
        );

        var result = await controller.GetEndings(1);

        var nf = Assert.IsType<NotFoundObjectResult>(result);
        var err = Assert.IsType<ErrorDto>(nf.Value);

        Assert.Equal("Ending scenes not found.", err.ErrorTitle);
    }

    [Fact]
    public async Task UpdateEndings_ReturnsServerError_WhenUpdateFails()
    {
        var endings = new List<EndingScene>
        {
            new EndingScene { StoryId = 1, EndingType = EndingType.Good },
            new EndingScene { StoryId = 1, EndingType = EndingType.Neutral },
            new EndingScene { StoryId = 1, EndingType = EndingType.Bad },
        };

        var mockScene = new Mock<ISceneRepository>();
        mockScene.Setup(r => r.GetEndingScenesByStoryId(1))
                 .ReturnsAsync(endings);

        mockScene.Setup(r => r.UpdateEndingScenes(endings))
                 .ReturnsAsync(false);

        var controller = new StoryEditingController(
            new Mock<IStoryRepository>().Object,
            mockScene.Object,
            new Mock<IAnswerOptionRepository>().Object,
            new Mock<ILogger<StoryEditingController>>().Object
        );

        var model = new UpdateEndingSceneDto
        {
            GoodEnding = "x",
            NeutralEnding = "y",
            BadEnding = "z"
        };

        var result = await controller.UpdateEndings(1, model);

        var err = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, err.StatusCode);
    }
}
