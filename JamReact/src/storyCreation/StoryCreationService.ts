import { QuestionScenesPayload } from "../types/createStory";

const API_URL = import.meta.env.VITE_API_URL;

// ---------------------------
// COMMON AUTH HEADERS
// ---------------------------
const getAuthHeaders = () => {
  const token = localStorage.getItem('token');

  const headers: HeadersInit = {
    "Content-Type": "application/json",
  };

  if (token) {
    headers["Authorization"] = `Bearer ${token}`;
  }

  return headers;
};

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
    headers: getAuthHeaders(),     
    credentials: "include",    
    body: JSON.stringify(payload),
  });
};

// ----------------------------
// STEP 2 – QUESTIONS
// ----------------------------
export const saveQuestions = async (payload: QuestionScenesPayload) => {
  return await fetch(`${API_URL}/api/storycreation/questions`, {
    method: "POST",
    headers: getAuthHeaders(),    
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
    headers: getAuthHeaders(),  
    credentials: "include",      
    body: JSON.stringify(payload),
  });
};

// ----------------------------
// STEP 4 – COMPLETE STORY
// ----------------------------
