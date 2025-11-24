export interface Story {
  storyId: number;
  title: string;
  description: string;
  difficultyLevel: number;
  accessibility: number;
  questionCount: number;
  code?: string;
  lastPlayed: string;
  //userId: string;
}
export interface EndingDto {
  goodEnding: string;
  neutralEnding: string;
  badEnding: string;
}

export interface EndingErrors {
  good: string;
  neutral: string;
  bad: string;
}

export interface IntroDto {
  title: string;
  description: string;
  introText: string;
  difficultyLevel: number;
  accessibility: number;
}

export interface IntroErrors {
  title: string;
  description: string;
  introText: string;
  difficulty: string;
  accessibility: string;
}

// ---- QUESTION CREATION DTO ----
export interface AnswerOptionDto {
  answerText: string;
  contextText: string;
}

export interface QuestionSceneDto {
  storyText: string;
  questionText: string;
  correctAnswerIndex: number;
  answers: AnswerOptionDto[];
}

export interface QuestionScenesPayload {
  questionScenes: QuestionSceneDto[];
}

// ---- ERRORS FOR CREATE QUESTIONS ----
export interface QuestionErrors {
  storyText?: string;
  questionText?: string;
  answers?: string;
  contextTexts?: string;
  correct?: string;
}
