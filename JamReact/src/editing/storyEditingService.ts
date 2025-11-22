const API_URL = import.meta.env.VITE_API_URL;

const getAuthHeaders = () => {
  const token = localStorage.getItem("token");
  return {
    "Content-Type": "application/json",
    Authorization: `Bearer ${token}`,
  };
};

// ------------------ STORY METADATA ------------------
export const getStoryMetadata = (storyId: number) =>
  fetch(`${API_URL}/api/storyediting/${storyId}`, {
    headers: getAuthHeaders(),
  });

export const updateStoryMetadata = (storyId: number, payload: any) =>
  fetch(`${API_URL}/api/storyediting/${storyId}`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify(payload),
  });

// ------------------ INTRO ------------------
export async function getIntro(storyId: number, token: string) {
    const res = await fetch(`${API_URL}/api/storyediting/${storyId}/intro`, {
        headers: {
            "Content-Type": "application/json",
            Authorization: `Bearer ${token}`,
        },
    });

    if (!res.ok) throw new Error("Failed to load intro");
    return await res.json();
}
export async function updateIntroScene(
  storyId: number,
  introText: string,
  token: string
) {
  return await fetch(`${API_URL}/api/storyediting/${storyId}/intro`, {
    method: "PUT",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify({ introText }),
  });
}


// ------------------ QUESTIONS ------------------
export const getQuestions = (storyId: number) =>
  fetch(`${API_URL}/api/storyediting/${storyId}/questions`, {
    headers: getAuthHeaders(),
  });

export const updateQuestions = (storyId: number, payload: any) =>
  fetch(`${API_URL}/api/storyediting/${storyId}/questions`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify(payload),
  });

// Delete 1 question
export const deleteQuestion = (questionSceneId: number) =>
  fetch(`${API_URL}/api/storyediting/questions/${questionSceneId}`, {
    method: "DELETE",
    headers: getAuthHeaders(),
  });

// ------------------ ENDINGS ------------------
export const getEndings = (storyId: number) =>
  fetch(`${API_URL}/api/storyediting/${storyId}/endings`, {
    headers: getAuthHeaders(),
  });

export const updateEndings = (storyId: number, payload: any) =>
  fetch(`${API_URL}/api/storyediting/${storyId}/endings`, {
    method: "PUT",
    headers: getAuthHeaders(),
    body: JSON.stringify(payload),
  });

  export async function deleteStory(storyId: number, token: string) {
  const API_URL = import.meta.env.VITE_API_URL;

  const response = await fetch(`${API_URL}/api/storyediting/${storyId}`, {
    method: "DELETE",
    headers: {
      Authorization: `Bearer ${token}`,
    },
  });

  if (!response.ok) throw new Error("Failed to delete story");
  return await response.json();
}