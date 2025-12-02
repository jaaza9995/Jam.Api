import { Story } from "../types/home";

const API_URL = import.meta.env.VITE_API_URL;

export interface HomePageResponse {
	firstName: string;
	yourStories: Story[];
	recentlyPlayed: Story[];
}

export interface HomePageResult {
	data: HomePageResponse | null;
	error: string | null;
}

export class UnauthorizedError extends Error {
	constructor(message: string = "Unauthorized") {
		super(message);
		this.name = "UnauthorizedError";
	}
}

export async function fetchHomePageData(
	token: string
): Promise<HomePageResult> {
	try {
		const res = await fetch(`${API_URL}/api/home/homepage`, {
			headers: {
				"Content-Type": "application/json",
				Authorization: `Bearer ${token}`,
			},
		});

		if (!res.ok) {
			if (res.status === 401) {
				throw new UnauthorizedError(
					"Token is invalid or expired. Please log in again."
				);
			}
			return { data: null, error: "Failed to load homepage." };
		}

		const data = await res.json();
		return { data, error: null };
	} catch (err) {
		if (err instanceof UnauthorizedError) {
			throw err;
		}

		return { data: null, error: "Network error while loading homepage." };
	}
}
