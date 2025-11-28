// ---- INTRO CREATION DTO ----
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

export interface QuestionErrors {
	storyText?: string;
	questionText?: string;
	answers?: string;
	contextTexts?: string;
	correct?: string;
}


// ---- ENDING CREATION DTO ----
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


// ---- STORY CREATION DTO ----
export interface StoryCreationData {
    intro: {
        title: string;
        description: string;
        difficulty: string;
        accessibility: string;
        introText: string;
    };
    questions: QuestionSceneDto[];
    endings: {
        good: string;
        neutral: string;
        bad: string;
    };
    token?: string;
}

export interface StoryCreationContextType {
    data: StoryCreationData;
    setData: React.Dispatch<React.SetStateAction<StoryCreationData>>;
    API_URL: string;
}