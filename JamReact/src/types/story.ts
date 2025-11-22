export interface Story {
  storyId: number;
  title: string;
  description: string;
  difficultyLevel: number | string;
  accessibility: number | string;
  questionCount: number;
  code?: string;
}