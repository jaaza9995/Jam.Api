import React, { useEffect, useState } from "react";
import { useAuth } from "../auth/AuthContext";
import "./HomePage.css";
import { useNavigate } from "react-router-dom";
import { Story } from "../types/story";

const API_URL = import.meta.env.VITE_API_URL;

const HomePage: React.FC = () => {
  const { token } = useAuth();
  const navigate = useNavigate();

  const [stories, setStories] = useState<Story[]>([]);
  const [recentlyPlayed, setRecentlyPlayed] = useState<Story[]>([]);
  const [firstName, setFirstName] = useState<string>("");
  const [loading, setLoading] = useState<boolean>(true);

useEffect(() => {
  const fetchHomePage = async () => {
    try {
      const response = await fetch(`${API_URL}/api/home/homepage`, {
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`,
        },
      });

      if (!response.ok) {
        console.error("Home API error:", response.status);
        return;
      }

      const data = await response.json();
      console.log("Home API data:", data);

      setFirstName(data.firstName || "Player");
      setStories(data.yourStories || []);
      setRecentlyPlayed(data.recentlyPlayed || []);

    } catch (error) {
      console.error("Error loading homepage:", error);
    } finally {
      setLoading(false);
    }
  };

  fetchHomePage();
}, [token]);


  return (
    <div className="homepage-container">

      <h1 className="homepage-title">WELCOME TO MATH UNIVERSE</h1>

      <div className="homepage-buttons">
        <button
          className="btn-green"
          onClick={() => navigate("/create/intro")}
        >
          MAKE NEW GAME
        </button>
        <button className="btn-pink">ADD NEW GAME</button>
      </div>

      {/* ================= YOUR GAMES ================ */}
      <section className="section-block">
        <h2 className="section-title">YOUR GAMES:</h2>

        {stories.length === 0 ? (
          <p className="empty-text">No stories found.</p>
        ) : (
          <ul className="story-list">
            {stories.map((s) => (
              <li key={s.storyId} className="story-card">

                <h3>{s.title}</h3>
                <p>{s.description}</p>
                <p>Questions: {s.questionCount}</p>

                {/* PRIVATE CODE */}
                {(s.accessibility === "Private" || s.accessibility === 1) && (
                  <p className="private-code">Game Code: {s.code}</p>
                )}

                {/* Buttons */}
                <div className="story-buttons">
                  <button
                    className="pixel-btn teal"
                    onClick={() => navigate(`/play/${s.storyId}`)}
                  >
                    PLAY
                  </button>

                  <button
                    className="pixel-btn pink"
                    onClick={() => navigate(`/edit/${s.storyId}/intro`)}
                  >
                    EDIT
                  </button>
                </div>

              </li>
            ))}
          </ul>
        )}
      </section>

      {/* ================= RECENTLY PLAYED ================ */}
      <section className="section-block">
        <h2 className="section-title">RECENTLY PLAYED:</h2>

        {recentlyPlayed.length === 0 ? (
          <p className="empty-text">No recently played games.</p>
        ) : (
          <ul className="story-list">
            {recentlyPlayed.map((s) => (
              <li key={s.storyId} className="story-card">
                <h3>{s.title}</h3>
                <p>{s.description}</p>
                <p>Questions: {s.questionCount}</p>
              </li>
            ))}
          </ul>
        )}
      </section>
    </div>
  );
};

export default HomePage;
