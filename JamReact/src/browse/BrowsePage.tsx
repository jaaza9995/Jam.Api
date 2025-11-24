import React, { useEffect, useState } from "react";
import { useAuth } from "../auth/AuthContext";
import { useNavigate } from "react-router-dom";
import PlayConfirmModal from "../shared/PlayConfirmModal";
import "./BrowsePage.css";
import "../App.css"
import { StoryCard } from "../types/storyCard";
import { fetchPublicStories, fetchPrivateStory } from "./BrowsePageService";


const API_URL = import.meta.env.VITE_API_URL;

const BrowsePage: React.FC = () => {
  const { token } = useAuth();
  const navigate = useNavigate();

  const [publicGames, setPublicGames] = useState<StoryCard[]>([]);
  const [publicSearch, setPublicSearch] = useState("");

  const [privateCode, setPrivateCode] = useState("");
  const [privateMatch, setPrivateMatch] = useState<StoryCard | null>(null);
  const [error, setError] = useState<string | null>(null);

  const [showModal, setShowModal] = useState(false);
  const [selectedGame, setSelectedGame] = useState<StoryCard | null>(null);

  const openModal = (game: any) => {
    setSelectedGame(game);
    setShowModal(true);
  };

  const closeModal = () => {
    setSelectedGame(null);
    setShowModal(false);
  };

  const confirmPlay = () => {
    navigate(`/play/${selectedGame!.storyId}`);
  };

  // ---------------------------
  // FETCH PUBLIC GAMES
  // ---------------------------
useEffect(() => {
  const load = async () => {
    setError(null);
    const { data, error } = await fetchPublicStories(token!);
    if (error) {
      setError(error);
    }
    setPublicGames(data ?? []);
  };
  load();
}, [token]);

  // ---------------------------
  // PUBLIC SEARCH
  // ---------------------------
  const filtered = publicGames.filter((g: StoryCard) => {
  const title = g.title.toLowerCase();
  return title.includes(publicSearch.toLowerCase());
});

  // ---------------------------
  // PRIVATE CODE SEARCH
  // ---------------------------
const handlePrivateSearch = async () => {
  if (!privateCode.trim()) return;

  setError(null);
  const { data, error } = await fetchPrivateStory(token!, privateCode);
  
  if (data) {
    setPrivateMatch(data);
    openModal(data);
  } else {
    setError(error ?? "No game found with this code.");
    setPrivateMatch(null);
  }
};


  return (
    <div className="browse-container">
      <h1 className="browse-title">Find a Game</h1>

      <div className="browse-sections">
        {error && <p className="error-text">{error}</p>}

        {/* PUBLIC GAMES */}
        <div className="browse-block">
          <h2>Public Games</h2>

          <input
            className="search-input"
            placeholder="Search by title..."
            value={publicSearch}
            onChange={(e) => setPublicSearch(e.target.value)}
          />

          <ul className="browse-list">
            {filtered.length > 0 ? (
              filtered.map((g) => (
               <li
                key={g.storyId}
                className="browse-card"
                onClick={() => openModal(g)}
              >
                <h3>{g.title}</h3>
                <p>{g.description}</p>

                {/* NEW: show difficulty, accessibility, question count */}
                <div className="browse-meta">
                  <p><strong>Questions:</strong> {g.questionCount}</p>
                  <p><strong>Difficulty:</strong> {g.difficultyLevel}</p>
                  <p><strong>Accessibility:</strong> {g.accessibility}</p>
                </div>
              </li>
              ))
            ) : (
              <p className="empty-msg">No games match your search.</p>
            )}
          </ul>
        </div>

        {/* PRIVATE GAMES */}
        <div className="browse-block">
          <h2>Private Game Code</h2>

          <input
            className="search-input"
            placeholder="Enter game code..."
            value={privateCode}
            onChange={(e) => setPrivateCode(e.target.value)}
          />

          <button className="pixel-btn pink" onClick={handlePrivateSearch}>
            Search
          </button>
        </div>
        
      </div>
      

      {/* MODAL */}
      <PlayConfirmModal
        title={selectedGame?.title || ""}
        show={showModal}
        onConfirm={confirmPlay}
        onCancel={closeModal}
      />
     {/* BACK KNAPP HELT NEDERST VENSTRE */}
    <button className="pixel-btn pixel-btn-back"
      onClick={() => navigate("/")}
    >
      Back to Home
    </button>

  </div>

    
  );
};

export default BrowsePage;
