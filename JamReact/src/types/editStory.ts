// ---------- Story Metadata ----------
export interface StoryMetadataDto {
  storyId: number;
  title: string;
  description: string;
  difficultyLevel: number; // 1–3
  accessibility: number | string;   // 0/1 or "Public"/"Private"
  code?: string | null;
}

// ---------- Intro ----------
export interface IntroDto {
  introText: string;
}

// ---------- Answer Option ----------
export interface AnswerOptionDto {
  answerOptionId?: number;
  answerText: string;
  contextText: string;
}

// ---------- Question Scene ----------
export interface QuestionSceneDto {
  questionSceneId: number;
  storyText: string;
  questionText: string;
  answers: AnswerOptionDto[];
  correctAnswerIndex: number;
}

// ---------- Endings ----------
export interface EndingsDto {
  goodEnding: string;
  neutralEnding: string;
  badEnding: string;
}
