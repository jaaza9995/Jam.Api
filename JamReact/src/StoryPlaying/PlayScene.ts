import { SceneType } from "./enums";
import { AnswerOption } from "./AnswerOption";

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
