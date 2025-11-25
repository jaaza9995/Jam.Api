export interface StoryCard {
  storyId: number;
  title: string;
  description: string;
  questionCount: number;
  difficultyLevel: number;   // backend sender string når vi bruker ToString()
  accessibility: number;     // backend sender string når vi bruker ToString()
  code: string;
}
