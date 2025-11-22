// src/editing/EditEndingsPage.tsx

import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getEndings, updateEndings } from "./storyEditingService";
import ConfirmUndoModal from "../shared/ConfirmUndoModal";
import "./EditStoryPage.css";

const EditEndingsPage: React.FC = () => {
  const { storyId } = useParams();
  const navigate = useNavigate();

  const [good, setGood] = useState("");
  const [neutral, setNeutral] = useState("");
  const [bad, setBad] = useState("");

  const [original, setOriginal] = useState({ good: "", neutral: "", bad: "" });

  const [errors, setErrors] = useState({ good: "", neutral: "", bad: "" });
  const [backendError, setBackendError] = useState("");

  const [loading, setLoading] = useState(true);

  const [showUndoConfirm, setShowUndoConfirm] = useState(false);
  const [showSavedMsg, setShowSavedMsg] = useState(false);

  // ---------------------------
  // LOAD
  // ---------------------------
  useEffect(() => {
    const load = async () => {
      if (!storyId) return;

      const res = await getEndings(Number(storyId));
      if (!res.ok) {
        setLoading(false);
        return;
      }

      const data = await res.json();
      setGood(data.goodEnding);
      setNeutral(data.neutralEnding);
      setBad(data.badEnding);

      setOriginal({
        good: data.goodEnding,
        neutral: data.neutralEnding,
        bad: data.badEnding,
      });

      setLoading(false);
    };
    load();
  }, [storyId]);

  const hasChanges = () => {
    return (
      good !== original.good ||
      neutral !== original.neutral ||
      bad !== original.bad
    );
  };

  // ---------------------------
  // VALIDATION
  // ---------------------------
  const validate = () => {
    const newErrors = { good: "", neutral: "", bad: "" };

    if (!good.trim()) newErrors.good = "Good ending is required.";
    if (!neutral.trim()) newErrors.neutral = "Neutral ending is required.";
    if (!bad.trim()) newErrors.bad = "Bad ending is required.";

    setErrors(newErrors);

    return !newErrors.good && !newErrors.neutral && !newErrors.bad;
  };

  // ---------------------------
  // SAVE
  // ---------------------------
  const handleSave = async () => {
    if (!storyId) return;
    setBackendError("");

    if (!validate()) return;

    const res = await updateEndings(Number(storyId), {
      goodEnding: good,
      neutralEnding: neutral,
      badEnding: bad,
    });

   if (!res.ok) {
      const body = (await res.json()) as {
        errors?: Record<string, string[]>;
        [key: string]: any;  // fallback
      };

      if (body.errors) {
        const first = Object.values(body.errors)[0][0];
        setBackendError(first);
      } else {
        setBackendError("Something went wrong.");
      }
      return;
    }

    setShowSavedMsg(true);
    setTimeout(() => setShowSavedMsg(false), 5000);

    setOriginal({ good, neutral, bad });
  };

  // ---------------------------
  // BACK
  // ---------------------------
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

      {/* Undo modal */}
      {showUndoConfirm && (
        <ConfirmUndoModal
          onConfirm={confirmUndo}
          onCancel={() => setShowUndoConfirm(false)}
        />
      )}

      {/* Saved toast */}
      {showSavedMsg && <div className="saved-toast">Saved Changes</div>}

      <h1 className="edit-title">Edit Endings</h1>

      {backendError && <p className="error-msg">{backendError}</p>}

      <div className="ending-block">
        <h3 className="ending-label">GOOD ENDING</h3>
        <textarea
          className="pixel-input ending-input"
          value={good}
          placeholder="Write the good ending..."
          onChange={(e) => setGood(e.target.value)}
        />
        {errors.good && <p className="error-msg">{errors.good}</p>}
      </div>

      <div className="ending-block">
        <h3 className="ending-label">NEUTRAL ENDING</h3>
        <textarea
          className="pixel-input ending-input"
          value={neutral}
          placeholder="Write the neutral ending..."
          onChange={(e) => setNeutral(e.target.value)}
        />
        {errors.neutral && <p className="error-msg">{errors.neutral}</p>}
      </div>

      <div className="ending-block">
        <h3 className="ending-label">BAD ENDING</h3>
        <textarea
          className="pixel-input ending-input"
          value={bad}
          placeholder="Write the bad ending..."
          onChange={(e) => setBad(e.target.value)}
        />
        {errors.bad && <p className="error-msg">{errors.bad}</p>}
      </div>

      <div className="edit-buttons">
        <button className="pixel-btn blue" onClick={handleBack}>
          Back
        </button>

        <button className="pixel-btn teal" onClick={handleSave}>
          Save Changes
        </button>
      </div>
    </div>
  );
};

export default EditEndingsPage;
