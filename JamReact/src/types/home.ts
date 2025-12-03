import { Story } from "../types/storyCard";

export interface StoryStatsModalProps {
	storyTitle: string;
    played: number;
    finished: number;
    failed: number;
    dnf: number;
	onConfirm: () => void;
	onCancel: () => void;
}

export interface HomePageResponse {
    firstName: string;
    yourStories: Story[];
    recentlyPlayed: Story[];
}

export interface HomePageResult {
    data: HomePageResponse | null;
    error: string | null;
}