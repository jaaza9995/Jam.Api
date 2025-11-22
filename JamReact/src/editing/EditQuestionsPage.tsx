// src/editing/EditQuestionsPage.tsx
import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import "./EditStoryPage.css";
import {
  getQuestions,
  updateQuestions,
  deleteQuestion,
} from "./storyEditingService";
import ConfirmUndoModal from "../shared/ConfirmUndoModal";

interface AnswerOption {
  answerOptionId?: number;   // i tilfelle backend sender id
  answerText: string;
  contextText: string;
}

interface QuestionScene {
  questionSceneId: number;
  storyText: string;
  questionText: string;
  answers: AnswerOption[];
  correctAnswerIndex: number;   // 0–3
}

const emptyQuestion = (): QuestionScene => ({
  questionSceneId: 0,
  storyText: "",
  questionText: "",
  answers: [
    { answerText: "", contextText: "" },
    { answerText: "", contextText: "" },
    { answerText: "", contextText: "" },
    { answerText: "", contextText: "" },
  ],
  correctAnswerIndex: -1,
});

const EditQuestionsPage: React.FC = () => {
  const { storyId } = useParams();
  const navigate = useNavigate();

  const [questions, setQuestions] = useState<QuestionScene[]>([]);
  const [originalQuestions, setOriginalQuestions] = useState<QuestionScene[]>(
    []
  );
  const [errors, setErrors] = useState<any[]>([]);
  const [backendError, setBackendError] = useState("");
  const [loading, setLoading] = useState(true);

  const [showUndoConfirm, setShowUndoConfirm] = useState(false);
  const [showSavedMsg, setShowSavedMsg] = useState(false);

  // -------------------------------
  // LOAD QUESTIONS
  // -------------------------------
  useEffect(() => {
    const load = async () => {
      if (!storyId) return;

      const res = await getQuestions(Number(storyId));
      if (!res.ok) {
        setLoading(false);
        return;
      }

      const data = (await res.json()) as QuestionScene[];

      // sørg for at alle har 4 svar
      const normalized = data.map((q) => ({
        ...q,
        answers:
          q.answers && q.answers.length === 4
            ? q.answers
            : [
                { answerText: "", contextText: "" },
                { answerText: "", contextText: "" },
                { answerText: "", contextText: "" },
                { answerText: "", contextText: "" },
              ],
      }));

      setQuestions(normalized);
      setOriginalQuestions(JSON.parse(JSON.stringify(normalized)));
      setLoading(false);
    };

    load();
  }, [storyId]);

  const hasChanges = () =>
    JSON.stringify(questions) !== JSON.stringify(originalQuestions);

  // -------------------------------
  // VALIDATION
  // -------------------------------
 // -------------------------------
// VALIDATION — MATCHER CREATE 1:1
// -------------------------------
const validate = () => {
  const newErrors: any[] = [];

    questions.forEach((q, i) => {
      const err: any = {};

      if (!q.storyText.trim())
        err.storyText = "Story context is required.";

      if (!q.questionText.trim())
        err.questionText = "Question text is required.";

      if (q.answers.some(a => !a.answerText.trim()))
        err.answers = "All 4 answer options must be filled.";

      if (q.answers.some(a => !a.contextText.trim()))
        err.contextTexts = "All 4 context texts must be filled.";

      if (q.correctAnswerIndex < 0)
        err.correct = "Please choose a correct answer.";

      newErrors[i] = err;
    });

    setErrors(newErrors);

    // Return TRUE if no errors anywhere
    return newErrors.every(e => Object.keys(e).length === 0);
  };


  // -------------------------------
  // SAVE
  // -------------------------------
  const handleSave = async () => {
    setBackendError("");

    if (!storyId) return;
    if (!validate()) return;

    const res = await updateQuestions(Number(storyId), questions);

    if (!res.ok) {
      try {
        const body = (await res.json()) as {
          errors?: Record<string, string[]>;
        };

        if (body.errors) {
          const first = Object.values(body.errors)[0][0];
          setBackendError(first);
        } else {
          setBackendError("Something went wrong.");
        }
      } catch {
        setBackendError("Unexpected server error.");
      }
      return;
    }

    setShowSavedMsg(true);
    setTimeout(() => setShowSavedMsg(false), 5000);

    setOriginalQuestions(JSON.parse(JSON.stringify(questions)));
  };

  // -------------------------------
  // ADD / DELETE
  // -------------------------------
  const handleAdd = () => {
    setQuestions((prev) => [...prev, emptyQuestion()]);
  };

  const handleDeleteScene = async (sceneId: number, index: number) => {
    if (questions.length === 1) {
      alert("You must have at least one question.");
      return;
    }

    if (sceneId !== 0) {
      // backend delete av hele questionScene
      await deleteQuestion(sceneId);
    }

    setQuestions((prev) => prev.filter((_, i) => i !== index));
  };

  // -------------------------------
  // BACK
  // -------------------------------
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
      {/* Undo-modal */}
      {showUndoConfirm && (
        <ConfirmUndoModal
          onConfirm={confirmUndo}
          onCancel={() => setShowUndoConfirm(false)}
        />
      )}

      {/* Saved toast */}
      {showSavedMsg && <div className="saved-toast">Saved Changes</div>}

      <h1 className="edit-title">Edit Questions</h1>

      {backendError && <p className="error-msg">{backendError}</p>}

      {questions.map((q, i) => (
        <div key={i} className="question-scene-card">
          {/* STORY CONTEXT (valgfritt å vise – du kan fjerne denne seksjonen om du ikke vil ha den) */}

          
         <h3 className="question-label">STORY CONTEXT</h3>
          <textarea
            className="pixel-input"
            value={q.storyText}
            onChange={(e) => {
              const updated = [...questions];
              updated[i] = { ...updated[i], storyText: e.target.value };

              setQuestions(updated);

              // clear error dynamically
              const copy = [...errors];
              copy[i] = { ...copy[i], storyText: "" };
              setErrors(copy);
            }}
          />
          {errors[i]?.storyText && (
            <p className="error-msg">{errors[i].storyText}</p>
          )}

          

          {/* QUESTION */}
       <h3 className="question-label">QUESTION</h3>
      <textarea
        className="pixel-input"
        value={q.questionText}
        onChange={(e) => {
          const updated = [...questions];
          updated[i] = { ...updated[i], questionText: e.target.value };
          setQuestions(updated);

          const copy = [...errors];
          copy[i] = { ...copy[i], questionText: "" };
          setErrors(copy);
        }}
      />
      {errors[i]?.questionText && (
        <p className="error-msg">{errors[i].questionText}</p>
      )}


          {/* ANSWER OPTIONS */}
          {q.answers.map((a, idx) => (
          <div className="answer-row" key={idx}>
            <input
              className="pixel-input"
              value={a.answerText}
              onChange={(e) => {
                const newQ = [...questions];
                const answers = [...newQ[i].answers];
                answers[idx].answerText = e.target.value;
                newQ[i].answers = answers;
                setQuestions(newQ);

                const copy = [...errors];
                copy[i] = { ...copy[i], answers: "" };
                setErrors(copy);
              }}
            />

            <button
              className={
                q.correctAnswerIndex === idx
                  ? "correct-toggle correct-toggle--active"
                  : "correct-toggle"
              }
              onClick={() => {
                const updated = [...questions];
                updated[i] = { ...updated[i], correctAnswerIndex: idx };
                setQuestions(updated);

                const copy = [...errors];
                copy[i] = { ...copy[i], correct: "" };
                setErrors(copy);
              }}
            >
              ✓
            </button>
          </div>
        ))}

        {errors[i]?.answers && (
          <p className="error-msg">{errors[i].answers}</p>
        )}

        {errors[i]?.correct && (
          <p className="error-msg">{errors[i].correct}</p>
        )}

          {q.answers.map((a, idx) => (
            <textarea
              key={idx}
              className="pixel-input"
              value={a.contextText}
              onChange={(e) => {
                const updated = [...questions];
                const answers = [...updated[i].answers];
                answers[idx].contextText = e.target.value;
                updated[i].answers = answers;
                setQuestions(updated);

                const copy = [...errors];
                copy[i] = { ...copy[i], contextTexts: "" };
                setErrors(copy);
              }}
            />
          ))}

          {errors[i]?.contextTexts && (
            <p className="error-msg">{errors[i].contextTexts}</p>
          )}


          {/* DELETE WHOLE QUESTION SCENE */}
          <button
            className="pixel-btn pink delete-scene-btn"
            onClick={() => handleDeleteScene(q.questionSceneId, i)}
          >
            DELETE QUESTION
          </button>
        </div>
      ))}

      {/* BOTTOM BUTTONS */}
      <div className="edit-buttons">
        <button className="pixel-btn teal" onClick={handleAdd}>
          Add Question
        </button>
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

export default EditQuestionsPage;
