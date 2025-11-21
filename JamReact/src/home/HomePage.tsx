import React, { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import "./HomePage.css";

interface Story {
  storyId: number;
  title: string;
  description: string;
  difficultyLevel: string;
  accessibility: string;
  questionCount: number;
}

const API_URL = import.meta.env.VITE_API_URL;

const HomePage = () => {
  
const navigate = useNavigate();

  const [stories, setStories] = useState<Story[]>([]);
  const [recentlyPlayed, setRecentlyPlayed] = useState<Story[]>([]);
  const [firstName, setFirstName] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchHomePage = async () => {
      try {
        const response = await fetch(`${API_URL}/api/home/homepage`);

        if (!response.ok) throw new Error("Failed to fetch homepage");

        const data = await response.json();
        console.log("homepage:", data);

        setFirstName(data.firstName || "");
        setStories(data.yourStories || []);
        setRecentlyPlayed(data.recentlyPlayed || []);
      } catch (error) {
        console.error("Error fetching homepage:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchHomePage();
  }, []);

  if (loading)
    return (
      <div className="pixel-bg">
        <p className="loading">Loading...</p>
      </div>
    );

  return (
    <div className="pixel-bg">
      <div className="home-wrapper">

        {/* TITLE */}
        <h1 className="game-title">
          WELCOME BACK, {firstName.toUpperCase()}!
        </h1>

        {/* BUTTONS */}
        <div className="button-row">
          <button className="pixel-btn teal" onClick={() => navigate("/create/intro")}>
            MAKE NEW GAME
          </button>
          <button className="pixel-btn pink">ADD NEW GAME</button>
        </div>

        {/* YOUR GAMES */}
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
                  <strong>Questions:</strong> {story.questionCount}
                </p>
                <p className="story-meta">
                  <strong>Difficulty:</strong> {story.difficultyLevel}
                </p>
                <p className="story-meta">
                  <strong>Access:</strong> {story.accessibility}
                </p>

                <div className="button-row card-buttons">
                  <button className="pixel-btn blue">EDIT</button>
                  <button className="pixel-btn yellow">PLAY</button>
                </div>
              </div>
            ))
          )}
        </div>

        {/* RECENTLY PLAYED */}
        <h2 className="section-title">RECENTLY PLAYED:</h2>
        <div className="story-grid">
          {recentlyPlayed.length === 0 ? (
            <p className="no-stories">No recently played games.</p>
          ) : (
            recentlyPlayed.map((story) => (
              <div className="story-card" key={story.storyId}>
                <h3 className="story-title">{story.title}</h3>
                <p className="story-desc">{story.description}</p>

                <p className="story-meta">
                  <strong>Questions:</strong> {story.questionCount}
                </p>
                <div className="button-row card-buttons">
                  <button className="pixel-btn yellow">PLAY AGAIN</button>
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
