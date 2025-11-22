const API_URL = import.meta.env.VITE_API_URL;

// ----------------------------
// STEP 1 – INTRO
// ----------------------------
export const saveIntro = async (payload: {
  title: string;
  description: string;
  introText: string;
  difficultyLevel: number;
  accessibility: number;
}) => {
  return await fetch(`${API_URL}/api/storycreation/intro`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(payload),
  });
};

// ----------------------------
// STEP 2 – QUESTIONS
// ----------------------------
export const saveQuestions = async (payload: any) => {
  return await fetch(`${API_URL}/api/storycreation/questions`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(payload),
  });
};

// ----------------------------
// STEP 3 – ENDINGS
// ----------------------------
export const saveEndings = async (payload: {
  goodEnding: string;
  neutralEnding: string;
  badEnding: string;
}) => {
  return await fetch(`${API_URL}/api/storycreation/endings`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    credentials: "include",
    body: JSON.stringify(payload),
  });
};

// ----------------------------
// STEP 4 – COMPLETE STORY
// ----------------------------
export const completeStory = async () => {
  return await fetch(`${API_URL}/api/storycreation/complete`, {
    method: "POST",
    credentials: "include",
  });
};
