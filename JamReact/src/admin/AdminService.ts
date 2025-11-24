import { IErrorDto } from "../types/ErrorDto";
import { UserDataDto } from "../types/admin";
import { StoryDataDto } from "../types/admin";

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
        return;
    }

    let errorText = "An unknown error occurred during network call.";
    
    // Specific handling for 403 Forbidden
    if (response.status === 403) {
        errorText = "You do not have access to this resource (Admin required).";
        throw new Error(errorText);
    }
    
    // Assume that the Backend sends errors in JSON format on other 4xx/5xx
    try {
        const errorBody: IErrorDto = await response.json();
        errorText = errorBody.errorTitle || errorText;
    } catch {
        // If the response is not JSON (e.g. plain text 500 error), use the text message
        try {
            errorText = await response.text();
        } catch {
            // If the response is empty
            errorText = `Error with status code: ${response.status}`;
        }
    }

    throw new Error(errorText);
};

export const getAdminUsers = async (): Promise<UserDataDto[]> => {
    const response = await fetch(`${API_URL}/api/Admin/users`, {
        method: "GET",
        headers: getAuthHeaders(),
    });

    await handleApiError(response);
    
    return response.json();
};

export const getAdminStories = async (): Promise<StoryDataDto[]> => {
    const response = await fetch(`${API_URL}/api/Admin/stories`, {
        method: "GET",
        headers: getAuthHeaders(),
    });

    await handleApiError(response);

    return response.json();
};