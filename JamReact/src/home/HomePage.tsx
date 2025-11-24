import React, { useEffect, useState } from "react";
import { useAuth } from "../auth/AuthContext";
import "./HomePage.css";
import { useNavigate } from "react-router-dom";
import { Story } from "../types/createStory";
import { fetchHomePageData } from "./homePageService";

const API_URL = import.meta.env.VITE_API_URL;

const HomePage: React.FC = () => {
  const { token } = useAuth();
  const navigate = useNavigate();

  const [stories, setStories] = useState<Story[]>([]);
  const [recentlyPlayed, setRecentlyPlayed] = useState<Story[]>([]);
  const [firstName, setFirstName] = useState<string>("");
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

useEffect(() => {
  const load = async () => {
    setLoading(true);
    setError(null);

    const { data, error } = await fetchHomePageData(token!);

    if (error || !data) {
      setError(error ?? "Failed to load homepage.");
      setLoading(false);
      return;
    }

    setFirstName(data.firstName || "Player");
    setStories(data.yourStories || []);

    setRecentlyPlayed(
      (data.recentlyPlayed || []).sort((a: Story, b: Story) =>
        new Date(b.lastPlayed as any).getTime() -
        new Date(a.lastPlayed as any).getTime()
      )
    );

    setLoading(false);
  };

  load();
}, [token]);



  return (
    <div className="homepage-container">

      <h1 className="homepage-title">WELCOME TO MATH UNIVERSE</h1>
      {error && <p className="error-text">{error}</p>}

      <div className="homepage-buttons">
        <button
          className="btn-makeNewGame"
          onClick={() => navigate("/create/intro")}
        >
          MAKE NEW GAME
        </button>
        <button
          className="btn-playNewGame"
          onClick={() => navigate("/browse")}
        >
          PLAY NEW GAME
        </button>
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
                    onClick={() => navigate(`/edit/${s.storyId}`)}
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
