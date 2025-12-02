export interface StoryStatsModalProps {
	storyTitle: string;
    played: number;
    finished: number;
    failed: number;
    dnf: number;
	onConfirm: () => void;
	onCancel: () => void;
}