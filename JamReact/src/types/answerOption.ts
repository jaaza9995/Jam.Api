export interface AnswerOption {
    answerOptionId?: number;
    answer: string;
    feedbackText: string;
    isCorrect: boolean;
    questionSceneId: number;
}