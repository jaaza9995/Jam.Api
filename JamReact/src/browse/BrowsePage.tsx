import React, { useEffect, useState } from "react";
import { useAuth } from "../auth/AuthContext";
import { useNavigate } from "react-router-dom";
import PlayConfirmModal from "../shared/PlayConfirmModal";
import "./BrowsePage.css";

const API_URL = import.meta.env.VITE_API_URL;

const BrowsePage: React.FC = () => {
  const { token } = useAuth();
  const navigate = useNavigate();

  const [publicGames, setPublicGames] = useState([]);
  const [publicSearch, setPublicSearch] = useState("");

  const [privateCode, setPrivateCode] = useState("");
  const [privateMatch, setPrivateMatch] = useState<any>(null);

  // Modal state
  const [showModal, setShowModal] = useState(false);
  const [selectedGame, setSelectedGame] = useState<any>(null);

  const openModal = (game: any) => {
    setSelectedGame(game);
    setShowModal(true);
  };

  const closeModal = () => {
    setSelectedGame(null);
    setShowModal(false);
  };

  const confirmPlay = () => {
    navigate(`/play/${selectedGame.storyId}`);
  };

  // Fetch all public games
  useEffect(() => {
    const fetchPublic = async () => {
      try {
        const res = await fetch(`${API_URL}/api/browse/public`, {
          headers: { Authorization: `Bearer ${token}` },
        });
        if (!res.ok) return;

        const data = await res.json();
        setPublicGames(data);
      } catch (e) {
        console.error("Error loading public games", e);
      }
    };

    fetchPublic();
  }, [token]);

  const filtered = publicGames.filter((g: any) =>
    g.title.toLowerCase().includes(publicSearch.toLowerCase())
  );

  // Private game search
  const handlePrivateSearch = async () => {
    if (!privateCode.trim()) return;

    const res = await fetch(`${API_URL}/api/browse/private/${privateCode}`, {
      headers: { Authorization: `Bearer ${token}` },
    });

    if (res.ok) {
      const game = await res.json();
      setPrivateMatch(game);
      openModal(game);
    } else {
      alert("No game found with this code.");
    }
  };

  return (
    <div className="browse-container">

      <h1 className="browse-title">Find a Game</h1>

      <div className="browse-sections">

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
            {filtered.map((g: any) => (
              <li
                key={g.storyId}
                className="browse-card"
                onClick={() => openModal(g)}
              >
                <h3>{g.title}</h3>
                <p>{g.description}</p>
              </li>
            ))}
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
    </div>
  );
};

export default BrowsePage;
