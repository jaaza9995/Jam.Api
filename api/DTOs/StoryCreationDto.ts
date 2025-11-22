// Enums match backend
export enum DifficultyLevel {
  Easy = 0,
  Medium = 1,
  Hard = 2,
}

export enum Accessibility {
  Public = 0,
  Private = 1,
}

// Matches C# AnswerOptionInput
export interface AnswerOptionInput {
  answerText: string;
  contextText: string;
}

// Matches C# QuestionSceneBaseDto
export interface QuestionSceneBaseDto {
  storyText: string;
  questionText: string;
  answers: AnswerOptionInput[];
  correctAnswerIndex: number;
}

// Matches C# EndingScenesDto
export interface EndingScenesDto {
  goodEnding: string;
  neutralEnding: string;
  badEnding: string;
}

// Matches hele C# StoryCreationDto
export interface StoryCreationDto {
  title: string;
  description: string;
  difficultyLevel: DifficultyLevel | null;
  accessibility: Accessibility | null;
  introText: string;
  questionScenes: QuestionSceneBaseDto[];
  goodEnding: string;
  neutralEnding: string;
  badEnding: string;
}
