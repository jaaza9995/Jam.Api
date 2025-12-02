export interface Story {
	storyId: number;
	title: string;
	description: string;
	difficultyLevel: number;
	accessibility: number;
	questionCount: number;
	code?: string;
	lastPlayed: string;
	played: number;
	finished: number;
	failed: number;
	dnf: number;
}

export interface StoryCard {
	storyId: number;
	title: string;
	description: string;
	questionCount: number;
	difficultyLevel: number;
	accessibility: number;
	code: string;
}
