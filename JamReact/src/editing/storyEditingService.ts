import { StoryMetadataDto, EndingsDto } from "../types/editStory";

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

export const updateStoryMetadata = (
	storyId: number,
	payload: StoryMetadataDto
) =>
	fetch(`${API_URL}/api/storyediting/${storyId}`, {
		method: "PUT",
		headers: getAuthHeaders(),
		body: JSON.stringify(payload),
	});

//------------------ STORY DELETION ------------------
export const deleteStory = (storyId: number) =>
	fetch(`${API_URL}/api/storyediting/${storyId}`, {
		method: "DELETE",
		headers: getAuthHeaders(),
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

// Delete one QuestionScene
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
