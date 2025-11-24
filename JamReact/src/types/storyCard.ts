export interface StoryCard {
  storyId: number;
  title: string;
  description: string;
  questionCount: number;
  difficultyLevel: string;   // backend sender string når vi bruker ToString()
  accessibility: string;     // backend sender string når vi bruker ToString()
  code: string;
}
