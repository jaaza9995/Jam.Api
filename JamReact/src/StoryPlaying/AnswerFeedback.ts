import { SceneType } from "./enums";

export interface IAnswerFeedback {
  sceneText: string; // Feedback from chosen AnswerOption
  
  // Updated Session-data
  newScore: number;
  newLevel: number;
  
  nextSceneId: number | null; // ID for the next Scene (Question or Ending)
  nextSceneType: SceneType;   // Type of Scene for next Scene
}