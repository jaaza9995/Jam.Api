// src/services/homeService.ts
import { Story } from "../types/createStory";

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

export async function fetchHomePageData(token: string): Promise<HomePageResult> {
  try {
    const res = await fetch(`${API_URL}/api/home/homepage`, {
      headers: {
        "Content-Type": "application/json",
        Authorization: `Bearer ${token}`,
      },
    });

    if (!res.ok) {
      const msg = res.status === 401 ? "Please log in again." : "Failed to load homepage.";
      return { data: null, error: msg };
    }

    const data = await res.json();
    return { data, error: null };
  } catch (err) {
    return { data: null, error: "Network error while loading homepage." };
  }
}
