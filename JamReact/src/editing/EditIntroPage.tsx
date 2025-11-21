import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import { getStoryMetadata, getIntro, updateIntroScene, deleteStory } from "./storyEditingService";
import DeleteModal from "../shared/DeleteModal";

const EditIntroPage: React.FC = () => {
  const { storyId } = useParams();
  const navigate = useNavigate();
  const { token } = useAuth();

  const [introText, setIntroText] = useState("");
  const [storyTitle, setStoryTitle] = useState("");
  const [showConfirm, setShowConfirm] = useState(false);

  // -------------------------------
  // LOAD STORY TITLE + INTRO TEXT
  // -------------------------------
  useEffect(() => {
    async function loadData() {
      if (!storyId || !token) return;

      try {
        // Hent metadata (inkludert tittel)
        const metaRes = await getStoryMetadata(Number(storyId));
        const meta = await metaRes.json();
        setStoryTitle(meta.title);

        // Hent Intro-tekst
        const intro = await getIntro(Number(storyId), token);
        setIntroText(intro.introText);
      } catch (err) {
        console.error("Failed loading edit intro:", err);
      }
    }

    loadData();
  }, [storyId, token]);

  // -------------------------------
  // SAVE INTRO
  // -------------------------------
  const handleSave = async () => {
    if (!storyId || !token) return;
    await updateIntroScene(Number(storyId), introText, token);
    navigate(`/edit/${storyId}/questions`);
  };

  // -------------------------------
  // DELETE STORY
  // -------------------------------
  const handleDelete = async () => {
    if (!storyId || !token) return;
    await deleteStory(Number(storyId), token);
    navigate("/");
  };

  return (
    <div className="edit-container">
      <h1 className="edit-title">Edit Intro</h1>

      <textarea
        className="edit-textarea"
        value={introText}
        onChange={(e) => setIntroText(e.target.value)}
      />

      <div className="edit-buttons">
        <button className="pixel-btn teal" onClick={handleSave}>
          Next
        </button>

        <button className="pixel-btn red" onClick={() => setShowConfirm(true)}>
          Delete Game
        </button>
      </div>

      {/* POPUP */}
      {showConfirm && (
        <DeleteModal
          storyTitle={storyTitle}
          onConfirm={handleDelete}
          onCancel={() => setShowConfirm(false)}
        />
      )}
    </div>
  );
};

export default EditIntroPage;
