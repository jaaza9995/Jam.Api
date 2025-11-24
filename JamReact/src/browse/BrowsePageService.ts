import { StoryCard } from "../types/storyCard";

const API_URL = import.meta.env.VITE_API_URL;

export interface FetchResult<T> {
  data: T | null;
  error: string | null;
}

export async function fetchPublicStories(token: string): Promise<FetchResult<StoryCard[]>> {
  try {
    const res = await fetch(`${API_URL}/api/browse/public`, {
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
    });

    if (!res.ok) {
      return { data: null, error: "Failed to load public stories." };
    }

    const data = await res.json();
    return { data, error: null };
  } catch (err) {
    return { data: null, error: "Network error while loading stories." };
  }
}

export async function fetchPrivateStory(token: string, code: string): Promise<FetchResult<StoryCard>> {
  try {
    const res = await fetch(`${API_URL}/api/browse/private/${code}`, {
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
    });

    if (!res.ok) {
      return { data: null, error: res.status === 404 ? "No game found with this code." : "Failed to load private game." };
    }

    const data = await res.json();
    return { data, error: null };
  } catch (err) {
    return { data: null, error: "Network error while searching for code." };
  }
}
