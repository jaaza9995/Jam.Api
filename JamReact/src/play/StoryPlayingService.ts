import {
	IStartSessionResponse,
	IPlayScene,
	IAnswerFeedback,
} from "../types/storyPlaying";
import { SceneType } from "../types/enums";
import { IErrorDto } from "../types/errorDto";

const API_URL = import.meta.env.VITE_API_URL;

const getAuthHeaders = () => {
	const token = localStorage.getItem("token");
	const headers: HeadersInit = {
		"Content-Type": "application/json",
	};
	if (token) {
		headers["Authorization"] = `Bearer ${token}`;
	}
	return headers;
};

// Helper function to handle non-OK responses
const handleApiError = async (response: Response): Promise<void> => {
	if (response.ok) {
		return; // No errors to handle
	}

	// Assume that the Backend sends errors in JSON format at 4xx/5xx
	let errorText = "Network response was not ok.";
	try {
		const errorBody: IErrorDto = await response.json();
		errorText = errorBody.errorTitle || errorText;
	} catch {
		// If the response is not JSON (e.g. plain text 500 error), use the default message
		errorText = await response.text();
	}

	throw new Error(errorText);
};

/**
 * Starts a new game session for a given story.
 * @param storyId The ID of the story to play.
 * @returns The response from the backend containing the session and scene IDs.
 */
export async function startSession(
	storyId: number
): Promise<IStartSessionResponse> {
	// 1. API-call
	const response = await fetch(
		`${API_URL}/api/StoryPlaying/start/${storyId}`,
		{
			method: "POST",
			headers: getAuthHeaders(),
		}
	);

	// 2. Handle errors (4xx/5xx) centrally
	await handleApiError(response);

	// 3. If OK, return data
	const data: IStartSessionResponse = await response.json();
	return data;
}

/**
 * Gets data for a specific scene.
 * @param sceneId The ID of the scene.
 * @param sceneType The type of scene (Intro, Question, Ending).
 * @param sessionId The ID of the active game session.
 * @returns The scene content (IPlayScene).
 */
export async function fetchScene(
	sceneId: number,
	sceneType: SceneType,
	sessionId: number
): Promise<IPlayScene> {
	// 1. API-call
	const response = await fetch(
		`${API_URL}/api/StoryPlaying/scene?sceneId=${sceneId}&sceneType=${sceneType}&sessionId=${sessionId}`,
		{
			method: "GET",
			headers: getAuthHeaders(),
		}
	);

	// 2. Handle errors (4xx/5xx) centrally
	await handleApiError(response);

	// 3. If OK, return data
	const data: IPlayScene = await response.json();
	return data;
}

// Define a type to handle the two different 200 OK responses
export type AnswerResult =
	| { type: "Game Over"; score: number }
	| { type: "Feedback"; feedback: IAnswerFeedback };

/**
 * Sends the user's response to the API and returns the result (Game Over or Feedback).
 * @param sessionId The ID of the game session.
 * @param selectedAnswerId The ID of the selected answer.
 * @returns An object that is either the Game Over result or IAnswerFeedback.
 * @throws Errors on 4xx/5xx status codes.
 */
export async function submitAnswer(
	sessionId: number,
	selectedAnswerId: number
): Promise<AnswerResult> {
	// 1. API-call
	const response = await fetch(`${API_URL}/api/StoryPlaying/answer`, {
		method: "POST",
		headers: getAuthHeaders(),
		body: JSON.stringify({
			sessionId: sessionId,
			selectedAnswerId: selectedAnswerId,
		}),
	});

	// 2. Check for successful responses (HTTP 200 OK)
	if (response.ok) {
		// Use .clone() to read the body once without consuming the stream
		const potentialResponse = await response.clone().json();

		if (potentialResponse?.message === "Game over") {
			// Case 1: Game Over (Level 1 Fail)
			return {
				type: "Game Over",
				score: potentialResponse.score,
			};
		}

		// Case 2: Standard Feedback (Read the body again for the actual DTO)
		const feedback: IAnswerFeedback = await response.json();
		return {
			type: "Feedback",
			feedback: feedback,
		};
	}

	// 3. Handle error responses (4xx/5xx)
	const errorBody: IErrorDto = await response.json();
	throw new Error(
		errorBody.errorTitle ||
			`Error sending response. Status: ${response.status}`
	);
}
