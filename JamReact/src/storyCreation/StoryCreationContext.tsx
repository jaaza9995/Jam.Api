import { createContext, useContext, useState } from "react";

export interface AnswerOption {
  answerText: string;
  contextText: string;
}

export interface QuestionScene {
  storyText: string;
  questionText: string;
  answers: AnswerOption[];
  correctAnswerIndex: number;
}

export interface StoryCreationData {
  intro: {
    title: string;
    description: string;
    difficulty: string;
    accessibility: string;
    introText: string;
  };
  questions: QuestionScene[];
  endings: {
    good: string;
    neutral: string;
    bad: string;
  };
  token?: string;
}

interface StoryCreationContextType {
  data: StoryCreationData;
  setData: React.Dispatch<React.SetStateAction<StoryCreationData>>;
  API_URL: string;
}

const defaultData: StoryCreationData = {
  intro: {
    title: "",
    description: "",
    difficulty: "",
    accessibility: "",
    introText: "",
  },
  questions: [],
  endings: {
    good: "",
    neutral: "",
    bad: "",
  },
};

const StoryCreationContext = createContext<StoryCreationContextType | null>(null);

export const StoryCreationProvider = ({ children }: { children: React.ReactNode }) => {
  const [data, setData] = useState<StoryCreationData>(defaultData);

  const API_URL = import.meta.env.VITE_API_URL;

  return (
    <StoryCreationContext.Provider value={{ data, setData, API_URL }}>
      {children}
    </StoryCreationContext.Provider>
  );
};

export const useStoryCreation = () => {
  const ctx = useContext(StoryCreationContext);
  if (!ctx) {
    throw new Error("useStoryCreation must be used inside <StoryCreationProvider>");
  }
  return ctx;
};

export default useStoryCreation;
