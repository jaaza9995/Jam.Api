import React, { useEffect, useState } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import { getStoryMetadata } from "./storyEditingService";
import { useAuth } from "../auth/AuthContext";
import DeleteModal from "../shared/DeleteModal";
import "./EditStoryPage.css";
import { StoryMetadataDto } from "../types/editStory";

const API_URL = import.meta.env.VITE_API_URL;

const EditStoryPage = () => {
  const { storyId } = useParams();
  const navigate = useNavigate();
  const location = useLocation();
  const { token } = useAuth();

  const [meta, setMeta] = useState<StoryMetadataDto | null>(null);
  const [questionCount, setQuestionCount] = useState<number | null>(null);

  const [showDeleteModal, setShowDeleteModal] = useState(false);

  useEffect(() => {
    const load = async () => {
      const res = await getStoryMetadata(Number(storyId));
      if (!res.ok) return;

      const data = await res.json();
      setMeta(data);

      const qRes = await fetch(`${API_URL}/api/storyediting/${storyId}/questions`, {
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`
        }
      });

      if (qRes.ok) {
        const questions = await qRes.json();
        setQuestionCount(questions.length);
      }
    };

    load();
  }, [storyId, token, location.key]);

  if (!meta) return <div className="pixel-bg">Loading...</div>;

  const isPrivate =
    meta.accessibility === 1 ||
    meta.accessibility === "Private" ||
    meta.accessibility === "private";

  const code = meta.code ?? null;

  const handleDelete = async () => {
    const res = await fetch(`${API_URL}/api/storyediting/${storyId}`, {
      method: "DELETE",
      headers: { Authorization: `Bearer ${token}` }
    });

    if (res.ok) {
      navigate("/");
    } else {
      alert("Failed to delete story.");
    }
  };

  return (
    <div className="pixel-bg edit-dashboard">

      {showDeleteModal && (
        <DeleteModal
          storyTitle={meta.title}
          onConfirm={handleDelete}
          onCancel={() => setShowDeleteModal(false)}
        />
      )}

      <div className="story-card">
        <h1 className="story-title">{meta.title}</h1>

        <p className="story-description">{meta.description}</p>

        <div className="story-meta-grid">
          <p><strong>Difficulty:</strong> {difficultyLabel(meta.difficultyLevel)}</p>
          <p><strong>Accessibility:</strong> {isPrivate ? "Private" : "Public"}</p>

          {isPrivate && (
            <p className="story-meta">
              <strong>Code:</strong> {code || "No code generated yet"}
            </p>
          )}

          <p><strong>Questions:</strong> {questionCount ?? "Loading..."}</p>
        </div>
      </div>

      <div className="edit-dashboard-buttons">
        <button className="pixel-btn teal" onClick={() => navigate(`/edit/${storyId}/intro`)}>Edit Intro</button>
        <button className="pixel-btn teal" onClick={() => navigate(`/edit/${storyId}/questions`)}>Edit Questions</button>
        <button className="pixel-btn teal" onClick={() => navigate(`/edit/${storyId}/endings`)}>Edit Endings</button>

        <button className="pixel-btn pink" onClick={() => setShowDeleteModal(true)}>
          Delete Game
        </button>

        <button className="pixel-btn blue" onClick={() => navigate("/")}>Back to Home</button>
      </div>

    </div>
  );
};

export default EditStoryPage;

// HELPER
const difficultyLabel = (level: number) => {
  switch (level) {
    case 1: return "Easy";
    case 2: return "Medium";
    case 3: return "Hard";
    default: return "Unknown";
  }
};
