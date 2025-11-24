import { SceneType } from "./enums";
import { AnswerOption } from "./answerOption";

export interface IAnswerFeedback {
	sceneText: string; // Feedback from chosen AnswerOption

	// Updated Session-data
	newScore: number;
	newLevel: number;

	nextSceneId: number | null; // ID for the next Scene (Question or Ending)
	nextSceneType: SceneType; // Type of Scene for next Scene
}

export interface IPlayScene {
    
	sessionId: number;
	sceneId: number;
	sceneType: SceneType; // Intro || Question || Ending
	sceneText: string;

    // For QuestionScenes
	question: string | null;
	answerOptions: AnswerOption[] | null;

    nextSceneAfterIntroId: number | null; // To navigate from IntroScene to first QuestionScene

    // Playing stats
	currentScore: number;
	maxScore: number;
	currentLevel: number;
}

export interface IStartSessionResponse {
    sessionId: number;
    sceneId: number;
    sceneType: SceneType;
}