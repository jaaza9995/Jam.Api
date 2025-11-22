import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import "./EditStoryPage.css";

import {
  getStoryMetadata,
  getIntro,
  updateIntroScene,
  updateStoryMetadata
} from "./storyEditingService";

import ConfirmUndoModal from "../shared/ConfirmUndoModal";

const API_URL = import.meta.env.VITE_API_URL;

const EditIntroPage = () => {
  const { storyId } = useParams();
  const navigate = useNavigate();
  const { token } = useAuth();

  // metadata fields
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [difficulty, setDifficulty] = useState(1);
  const [accessibility, setAccessibility] = useState(0);

  // intro field
  const [introText, setIntroText] = useState("");

  // original values
  const [original, setOriginal] = useState<any>(null);

  const [loading, setLoading] = useState(true);
  const [showUndoConfirm, setShowUndoConfirm] = useState(false);
  const [showSavedMsg, setShowSavedMsg] = useState(false);

  // 🔥 VALIDATION ERRORS – SAMME SOM CREATE PAGE
  const [errors, setErrors] = useState({
    title: "",
    description: "",
    introText: "",
    difficulty: "",
    accessibility: "",
  });

  // 🔥 VALIDATE FUNCTION – 100% MATCH MED CREATE siden
  const validate = () => {
    const newErrors: any = {};

    if (!title.trim()) newErrors.title = "You must write a Title for your game.";
    if (!description.trim()) newErrors.description = "You must write a Description for your game";
    if (!introText.trim()) newErrors.introText = "You must write an Intro Text for your game";
    if (!difficulty) newErrors.difficulty = "Please choose difficulty.";
    if (!accessibility && accessibility !== 0) newErrors.accessibility = "Please choose accessibility.";

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  useEffect(() => {
    const load = async () => {
      if (!storyId || !token) return;

      const mRes = await getStoryMetadata(Number(storyId));
      const m = await mRes.json();

      const i = await getIntro(Number(storyId), token);

      setTitle(m.title);
      setDescription(m.description);
      setDifficulty(m.difficultyLevel);
      setAccessibility(m.accessibility);
      setIntroText(i.introText);

      setOriginal({
        title: m.title,
        description: m.description,
        difficulty: m.difficultyLevel,
        accessibility: m.accessibility,
        intro: i.introText
      });

      setLoading(false);
    };

    load();
  }, [storyId, token]);

  const hasChanges = () => {
    if (!original) return false;

    return (
      title !== original.title ||
      description !== original.description ||
      difficulty !== original.difficulty ||
      accessibility !== original.accessibility ||
      introText !== original.intro
    );
  };

  // SAVE
  const handleSave = async () => {
    if (!storyId || !token) return;

    // 🔥 RUN VALIDATION
    const isValid = validate();
    if (!isValid) return;

    await updateStoryMetadata(Number(storyId), {
      storyId: Number(storyId),
      title,
      description,
      difficultyLevel: difficulty,
      accessibility
    });

    await updateIntroScene(Number(storyId), introText, token);

    const updated = await getStoryMetadata(Number(storyId)).then(res => res.json());

    setOriginal({
      title: updated.title,
      description: updated.description,
      difficulty: updated.difficultyLevel,
      accessibility: updated.accessibility,
      intro: introText,
      code: updated.code
    });

    setShowSavedMsg(true);
    setTimeout(() => setShowSavedMsg(false), 5000);
  };

  // BACK BUTTON
  const handleBack = () => {
    if (hasChanges()) {
      setShowUndoConfirm(true);
    } else {
      navigate(`/edit/${storyId}`);
    }
  };

  const confirmUndo = () => {
    navigate(`/edit/${storyId}`);
  };

  if (loading) return <div className="pixel-bg">Loading...</div>;

  return (
    <div className="pixel-bg edit-container">

      {showUndoConfirm && (
        <ConfirmUndoModal
          onConfirm={confirmUndo}
          onCancel={() => setShowUndoConfirm(false)}
        />
      )}

      {showSavedMsg && (
        <div className="saved-toast">Saved Changes</div>
      )}

      <h1 className="edit-title">Edit Intro</h1>

      {/* TITLE */}
      <label className="edit-label">Title</label>
      <input
        className="pixel-input"
        value={title}
        onChange={(e) => {
          setTitle(e.target.value);
          setErrors(prev => ({ ...prev, title: "" }));
        }}
      />
      {errors.title && <p className="error-msg">{errors.title}</p>}

      {/* DESCRIPTION */}
      <label className="edit-label">Description</label>
      <textarea
        className="pixel-input"
        value={description}
        onChange={(e) => {
          setDescription(e.target.value);
          setErrors(prev => ({ ...prev, description: "" }));
        }}
      />
      {errors.description && <p className="error-msg">{errors.description}</p>}

      {/* DIFFICULTY */}
      <label className="edit-label">Difficulty</label>
      <select
        className="pixel-input"
        value={difficulty}
        onChange={(e) => {
          setDifficulty(Number(e.target.value));
          setErrors(prev => ({ ...prev, difficulty: "" }));
        }}
      >
        <option value="">Select difficulty...</option>
        <option value={1}>Easy</option>
        <option value={2}>Medium</option>
        <option value={3}>Hard</option>
      </select>
      {errors.difficulty && <p className="error-msg">{errors.difficulty}</p>}

      {/* ACCESSIBILITY */}
      <label className="edit-label">Accessibility</label>
      <select
        className="pixel-input"
        value={accessibility}
        onChange={(e) => {
          setAccessibility(Number(e.target.value));
          setErrors(prev => ({ ...prev, accessibility: "" }));
        }}
      >
        <option value="">Select accessibility...</option>
        <option value={0}>Public</option>
        <option value={1}>Private</option>
      </select>
      {errors.accessibility && <p className="error-msg">{errors.accessibility}</p>}

      {/* INTRO TEXT */}
      <label className="edit-label">Introduction</label>
      <textarea
        className="pixel-input edit-textarea"
        value={introText}
        onChange={(e) => {
          setIntroText(e.target.value);
          setErrors(prev => ({ ...prev, introText: "" }));
        }}
      />
      {errors.introText && <p className="error-msg">{errors.introText}</p>}

      <div className="edit-buttons">
        <button className="pixel-btn teal" onClick={handleSave}>
          Save Changes
        </button>

        <button className="pixel-btn blue" onClick={handleBack}>
          Back
        </button>
      </div>

    </div>
  );
};

export default EditIntroPage;
