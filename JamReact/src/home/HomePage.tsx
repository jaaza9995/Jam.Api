import React, { useEffect, useState } from "react";
import "./HomePage.css";

interface Story {
  storyId: number;
  title: string;
  description: string;
  difficultyLevel: string;
  accessible: boolean;
  questionCount?: number;
}
const API_URL = import.meta.env.VITE_API_URL;

const HomePage = () => {
  const [stories, setStories] = useState<Story[]>([]);
  const [loading, setLoading] = useState(true);


useEffect(() => {
  const fetchStories = async () => {
    try {
      const token = localStorage.getItem("token");

      const response = await fetch(`${API_URL}/api/home/dashboard`, {
        headers: { Authorization: `Bearer ${token}` },
      });

      console.log("Response status:", response.status);

      if (!response.ok) throw new Error("Failed to fetch stories");

      const data = await response.json();
      console.log("Parsed data:", data);

      setStories(data.YourStories || []);
    } catch (error) {
      console.error("Error fetching stories:", error);
    } finally {
      setLoading(false);
    }
  };

  fetchStories();
}, []);

  if (loading)
    return (
      <div className="pixel-bg">
        <p className="loading">Loading...</p>
      </div>
    );

  return (
    <div className="pixel-bg">
      <div className="home-container">
        <h1 className="game-title">WELCOME BACK, PLAYER!</h1>

        <div className="button-row">
          <button className="pixel-btn teal">MAKE NEW GAME</button>
          <button className="pixel-btn pink">ADD NEW GAME</button>
        </div>

        <h2 className="section-title">YOUR GAMES:</h2>
        <div className="story-grid">
          {stories.length === 0 ? (
            <p className="no-stories">No stories found.</p>
          ) : (
            stories.map((story) => (
              <div className="story-card" key={story.storyId}>
                <h3 className="story-title">{story.title}</h3>
                <p className="story-desc">{story.description}</p>
                <p className="story-meta">
                  <strong>Questions:</strong> {story.questionCount ?? 0}
                </p>
                <p className="story-meta">
                  <strong>Difficulty:</strong> {story.difficultyLevel}
                </p>

                <div className="button-row">
                  <button className="pixel-btn brown">EDIT</button>
                  <button className="pixel-btn yellow">PLAY</button>
                </div>
              </div>
            ))
          )}
        </div>
      </div>
    </div>
  );
};

export default HomePage;
