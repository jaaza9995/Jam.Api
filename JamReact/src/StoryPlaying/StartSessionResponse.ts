import { SceneType } from "./enums";

export interface IStartSessionResponse {
  sessionId: number;
  sceneId: number;
  sceneType: SceneType;
}