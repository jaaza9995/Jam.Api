// src/editing/EditIntroPage.tsx

import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import "./EditStoryPage.css";
import { parseBackendErrors } from "../utils/parseBackendErrors";

import {
  getStoryMetadata,
  getIntro,
  updateIntroScene,
  updateStoryMetadata,
} from "./storyEditingService";

import ConfirmUndoModal from "../shared/ConfirmUndoModal";

import { StoryMetadataDto, IntroDto } from "../types/editStory";

const API_URL = import.meta.env.VITE_API_URL;

const EditIntroPage: React.FC = () => {
  const { storyId } = useParams();
  const navigate = useNavigate();
  const { token } = useAuth();

  // METADATA
  const [title, setTitle] = useState<string>("");
  const [description, setDescription] = useState<string>("");
  const [difficulty, setDifficulty] = useState<number>(1);
  const [accessibility, setAccessibility] = useState<number>(0);

  // INTRO TEXT
  const [introText, setIntroText] = useState<string>("");

  // ORIGINAL VALUES
  const [original, setOriginal] = useState<{
    title: string;
    description: string;
    difficulty: number;
    accessibility: number;
    intro: string;
  } | null>(null);

  const [loading, setLoading] = useState<boolean>(true);
  const [backendError, setBackendError] = useState<string>("");
  const [showUndoConfirm, setShowUndoConfirm] = useState<boolean>(false);
  const [showSavedMsg, setShowSavedMsg] = useState<boolean>(false);

  // 🔥 NEW: "no changes" toast
  const [showNoChangesMsg, setShowNoChangesMsg] = useState<boolean>(false);

  // ERRORS
  const [errors, setErrors] = useState({
    title: "",
    description: "",
    introText: "",
    difficulty: "",
    accessibility: "",
  });

  // ------------------------------------
  // VALIDATION
  // ------------------------------------
  const validate = (): boolean => {
    const newErrors = {
      title: "",
      description: "",
      introText: "",
      difficulty: "",
      accessibility: "",
    };

    if (!title.trim()) newErrors.title = "You must write a Title for your game.";
    if (!description.trim())
      newErrors.description = "You must write a Description for your game";
    if (!introText.trim())
      newErrors.introText = "You must write an Intro Text for your game";

    if (!difficulty) newErrors.difficulty = "Please choose difficulty.";
    if (accessibility !== 0 && accessibility !== 1)
      newErrors.accessibility = "Please choose accessibility.";

    setErrors(newErrors);
    return Object.values(newErrors).every((e) => e === "");
  };

  // ------------------------------------
  // LOAD FROM BACKEND
  // ------------------------------------
  useEffect(() => {
    const load = async () => {
      if (!storyId || !token) {
        setBackendError("Missing story or token");
        setLoading(false);
        return;
      }

      try {
        const metaRes = await getStoryMetadata(Number(storyId));
        if (!metaRes.ok) throw new Error("Failed to load story metadata");
        const meta = (await metaRes.json()) as StoryMetadataDto;

        let introTextValue = "";
        try {
          const intro = await getIntro(Number(storyId), token);
          introTextValue = intro.introText ?? "";
        } catch (introErr) {
          console.warn("Intro missing, starting empty", introErr);
        }

        const parsedDifficulty = Number(meta.difficultyLevel);
        const parsedAccessibility = Number(meta.accessibility);

        setTitle(meta.title);
        setDescription(meta.description);
        setDifficulty(isNaN(parsedDifficulty) ? 1 : parsedDifficulty);
        setAccessibility(isNaN(parsedAccessibility) ? 0 : parsedAccessibility);
        setIntroText(introTextValue);

        setOriginal({
          title: meta.title,
          description: meta.description,
          difficulty: isNaN(parsedDifficulty) ? 1 : parsedDifficulty,
          accessibility: isNaN(parsedAccessibility) ? 0 : parsedAccessibility,
          intro: introTextValue,
        });
      } catch (err) {
        console.error(err);
        setBackendError("Could not load intro. Please try again.");
      } finally {
        setLoading(false);
      }
    };

    load();
  }, [storyId, token]);

  // ------------------------------------
  // CHANGE DETECTION
  // ------------------------------------
  const hasChanges = (): boolean => {
    if (!original) return false;
    return (
      title !== original.title ||
      description !== original.description ||
      difficulty !== original.difficulty ||
      accessibility !== original.accessibility ||
      introText !== original.intro
    );
  };

  // ------------------------------------
  // SAVE
  // ------------------------------------

const handleSave = async () => {
  setBackendError("");

  // 🔥 NEW: show "no changes" toast
  if (!hasChanges()) {
    setShowNoChangesMsg(true);
    setTimeout(() => setShowNoChangesMsg(false), 4000);
    return;
  }

  if (!storyId || !token) {
    setBackendError("Missing story or token");
    return;
  }

  // Local validation
  if (!validate()) return;

  // -----------------------------
  // SAVE METADATA
  // -----------------------------
  const metaRes = await updateStoryMetadata(Number(storyId), {
    storyId: Number(storyId),
    title,
    description,
    difficultyLevel: difficulty,
    accessibility,
  });

  if (!metaRes.ok) {
    let body = null;
    try {
      body = await metaRes.json();
    } catch {}

    const parsed = parseBackendErrors(body);

    setErrors((prev) => ({
      ...prev,
      title: parsed.title || "",
      description: parsed.description || "",
      difficulty: parsed.difficultyLevel || "",
      accessibility: parsed.accessibility || "",
    }));

    if (body?.errorTitle) setBackendError(body.errorTitle);
    return;
  }

  // -----------------------------
  // SAVE INTRO TEXT
  // -----------------------------
  const introRes = await updateIntroScene(Number(storyId), introText);

  if (!introRes.ok) {
    let body = null;
    try {
      body = await introRes.json();
    } catch {}

    const parsed = parseBackendErrors(body);

    setErrors((prev) => ({
      ...prev,
      introText: parsed.introText || "",
    }));

    if (body?.errorTitle) setBackendError(body.errorTitle);

    return;
  }

  // -----------------------------
  // REFRESH ORIGINAL SAVED DATA
  // -----------------------------
  const updatedMeta = await getStoryMetadata(Number(storyId)).then((res) =>
    res.json()
  );

  setOriginal({
    title: updatedMeta.title,
    description: updatedMeta.description,
    difficulty: updatedMeta.difficultyLevel,
    accessibility: Number(updatedMeta.accessibility),
    intro: introText,
  });

  setShowSavedMsg(true);
  setTimeout(() => setShowSavedMsg(false), 5000);
};

  // ------------------------------------
  // BACK BUTTON
  // ------------------------------------
  const handleBack = () => {
    if (hasChanges()) setShowUndoConfirm(true);
    else navigate(`/edit/${storyId}`);
  };

  const confirmUndo = () => navigate(`/edit/${storyId}`);

  if (loading) return <div className="pixel-bg">Loading...</div>;

  // ------------------------------------
  // RENDER
  // ------------------------------------
  return (
    <div className="pixel-bg edit-container">

      {/* Undo modal */}
      {showUndoConfirm && (
        <ConfirmUndoModal
          onConfirm={confirmUndo}
          onCancel={() => setShowUndoConfirm(false)}
        />
      )}

      {/* Saved toast */}
      {showSavedMsg && <div className="saved-toast">Saved Changes</div>}

      {/* 🔥 NEW: No changes toast */}
      {showNoChangesMsg && (
        <div className="nochanges-toast">No changes have been done</div>
      )}

      <h1 className="edit-title">Edit Intro</h1>

      {backendError && <p className="error-msg">{backendError}</p>}

      {/* TITLE */}
      <label className="edit-label">Title</label>
      <input
        className="pixel-input"
        value={title}
        onChange={(e) => {
          setTitle(e.target.value);
          setErrors((prev) => ({ ...prev, title: "" }));
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
          setErrors((prev) => ({ ...prev, description: "" }));
        }}
      />
      {errors.description && <p className="error-msg">{errors.description}</p>}

      {/* DIFFICULTY */}
      <label className="edit-label">Difficulty</label>
      <select
        className="pixel-input"
        value={difficulty}
        onChange={(e) => setDifficulty(Number(e.target.value))}
      >
        <option value={1}>Easy</option>
        <option value={2}>Medium</option>
        <option value={3}>Hard</option>
      </select>

      {/* ACCESSIBILITY */}
      <label className="edit-label">Accessibility</label>
      <select
        className="pixel-input"
        value={accessibility}
        onChange={(e) => setAccessibility(Number(e.target.value))}
      >
        <option value={0}>Public</option>
        <option value={1}>Private</option>
      </select>

      {/* INTRO TEXT */}
      <label className="edit-label">Introduction</label>
      <textarea
        className="pixel-input edit-textarea"
        value={introText}
        onChange={(e) => {
          setIntroText(e.target.value);
          setErrors((prev) => ({ ...prev, introText: "" }));
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
