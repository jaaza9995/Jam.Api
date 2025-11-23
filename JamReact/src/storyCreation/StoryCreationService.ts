import { StoryMetadataDto, EndingsDto } from "../types/editStory";
import { IntroDto, QuestionScenesPayload } from "../types/createStory";

const API_URL = import.meta.env.VITE_API_URL;

const getAuthHeaders = () => {
  const token = localStorage.getItem("token");
  return {
    "Content-Type": "application/json",
    Authorization: `Bearer ${token}`,
  };
};

// ------------------ STORY METADATA ------------------
export const getStoryMetadata = (storyId: number): Promise<Response> =>
  fetch(`${API_URL}/api/storyediting/${storyId}`, {
    headers: getAuthHeaders(),
  });

export const updateStoryMetadata = (storyId: number, payload: StoryMetadataDto) =>
  fetch(`${API_URL}/api/storyediting/${storyId}`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify(payload),
  });

// ------------------ INTRO ------------------
export async function getIntro(storyId: number) {
  const res = await fetch(`${API_URL}/api/storyediting/${storyId}/intro`, {
    headers: getAuthHeaders(),
  });

  if (!res.ok) throw new Error("Failed to load intro");
  return await res.json();
}

export async function updateIntroScene(storyId: number, introText: string) {
  return await fetch(`${API_URL}/api/storyediting/${storyId}/intro`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify({ introText }),
  });
}

// ------------------ QUESTIONS ------------------
export const getQuestions = (storyId: number): Promise<Response> =>
  fetch(`${API_URL}/api/storyediting/${storyId}/questions`, {
    headers: getAuthHeaders(),
  });

export const updateQuestions = (storyId: number, payload: unknown) =>
  fetch(`${API_URL}/api/storyediting/${storyId}/questions`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify(payload),
  });

export const deleteQuestion = (questionSceneId: number) =>
  fetch(`${API_URL}/api/storyediting/questions/${questionSceneId}`, {
    method: "DELETE",
    headers: getAuthHeaders(),
  });

// ------------------ ENDINGS ------------------
export const getEndings = (storyId: number): Promise<Response> =>
  fetch(`${API_URL}/api/storyediting/${storyId}/endings`, {
    headers: getAuthHeaders(),
  });

export const updateEndings = (storyId: number, payload: EndingsDto) =>
  fetch(`${API_URL}/api/storyediting/${storyId}/endings`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify(payload),
  });

export async function deleteStory(storyId: number) {
  const response = await fetch(`${API_URL}/api/storyediting/${storyId}`, {
    method: "DELETE",
    headers: getAuthHeaders(),
  });

  if (!response.ok) throw new Error("Failed to delete story");
  return await response.json();
}

// ------------------ CREATION FLOW ------------------
export async function saveIntro(payload: IntroDto) {
  const res = await fetch(`${API_URL}/api/storycreation/intro`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify(payload)
  });

  let data = null;
  try {
    data = await res.json();
  } catch (e) {}

  return { ok: res.ok, status: res.status, data };
}

export const saveQuestions = (payload: QuestionScenesPayload) =>
  fetch(`${API_URL}/api/storycreation/questions`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(payload),
    credentials: "include",
  });

export const saveEndings = (payload: EndingsDto) =>
  fetch(`${API_URL}/api/storycreation/endings`, {
    method: "POST",
    headers: getAuthHeaders(),
    body: JSON.stringify(payload),
    credentials: "include",
  });

export const finishCreation = () =>
  fetch(`${API_URL}/api/storycreation/create`, {
    method: "POST",
    headers: getAuthHeaders(),
    credentials: "include",
  });
