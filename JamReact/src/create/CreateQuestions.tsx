import { useState } from "react";
import { useNavigate } from "react-router-dom";
import "./Create.css";
import useStoryCreation from "../storyCreation/StoryCreationContext";
import { saveQuestions } from "../storyCreation/StoryCreationService";

const emptyQuestion = () => ({
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

const emptyErrors = () => ({
  storyText: "",
  questionText: "",
  answers: "",
  contextTexts: "",
  correct: "",
});

const CreateQuestions = () => {
  const navigate = useNavigate();
  const { data, setData } = useStoryCreation();

  const [questions, setQuestions] = useState(
    data.questions.length > 0 ? data.questions : [emptyQuestion()]
  );

  // errors[i] = { storyText?, questionText?, answers?, contextTexts?, correct? }
  const [errors, setErrors] = useState<any[]>([]);

  // -----------------------------
  // VALIDATION
  // -----------------------------
  const validate = () => {
    const errList = questions.map((q) => {
      const e: any = {};

      if (!q.storyText.trim()) e.storyText = "Story context is required.";
      if (!q.questionText.trim()) e.questionText = "Question text is required.";

      if (q.answers.some((a) => !a.answerText.trim()))
        e.answers = "All 4 answer options must be filled.";

      if (q.answers.some((a) => !a.contextText.trim()))
        e.contextTexts = "All 4 context texts must be filled.";

      if (q.correctAnswerIndex < 0)
        e.correct = "Please choose a correct answer.";

      return e;
    });

    setErrors(errList);

    // return TRUE only if all questions have NO errors
    return errList.every((e) => Object.keys(e).length === 0);
  };

  // -----------------------------
  // HANDLERS
  // -----------------------------
  const handleNext = async () => {
    if (!validate()) return;

    const mapped = questions.map((q) => ({
      storyText: q.storyText,
      questionText: q.questionText,
      correctAnswerIndex: q.correctAnswerIndex,
      answers: q.answers.map((a) => ({
        answerText: a.answerText,
        contextText: a.contextText,
      })),
    }));

    const payload = { questionScenes: mapped };
    console.log("payload", payload);

    const res = await saveQuestions(payload);
    if (!res.ok) {
      const body = await res.text();
      console.error("Failed to save questions:", body);
      return;
    }

    setData((prev) => ({ ...prev, questions }));
    navigate("/create/endings");
  };

  const update = (index: number, updated: any) => {
    const copy = [...questions];
    copy[index] = updated;
    setQuestions(copy);

    // Clear errors for that field dynamically
    const newErrors = [...errors];
    newErrors[index] = {};
    setErrors(newErrors);
  };

  const remove = (index: number) => {
    if (questions.length === 1) return;
    setQuestions(questions.filter((_, i) => i !== index));
    setErrors(errors.filter((_, i) => i !== index));
  };

  const add = () => {
    setQuestions([...questions, emptyQuestion()]);
    setErrors([...errors, emptyErrors()]);
  };

  // -----------------------------
  // RENDER
  // -----------------------------
  return (
    <div className="pixel-bg">
      <h1 className="pixel-title">CREATE QUESTIONS</h1>

      <div className="questions-wrapper">
        {questions.map((q, i) => (
          <div className="question-box" key={i}>
            <h3 className="question-label">STORY CONTEXT</h3>
            <textarea
              className="pixel-input"
              value={q.storyText}
              placeholder="Write the story text shown before the question..."
              onChange={(e) =>
                update(i, { ...q, storyText: e.target.value })
              }
            />
            {errors[i]?.storyText && (
              <p className="error-msg">{errors[i].storyText}</p>
            )}

            <h3 className="question-label">QUESTION</h3>
            <textarea
              className="pixel-input"
              value={q.questionText}
              placeholder="Write the question the player must answer..."
              onChange={(e) =>
                update(i, { ...q, questionText: e.target.value })
              }
            />
            {errors[i]?.questionText && (
              <p className="error-msg">{errors[i].questionText}</p>
            )}

            <h3 className="question-label">ANSWER OPTIONS</h3>
            {q.answers.map((a, ai) => (
              <div className="answer-row" key={ai}>
                <input
                  className="pixel-input-small"
                  value={a.answerText}
                  placeholder={`Answer option ${ai + 1}`}
                  onChange={(e) => {
                    const list = [...q.answers];
                    list[ai].answerText = e.target.value;
                    update(i, { ...q, answers: list });
                  }}
                />

                <button
                  className={`correct-btn ${
                    q.correctAnswerIndex === ai ? "selected" : ""
                  }`}
                  onClick={() =>
                    update(i, { ...q, correctAnswerIndex: ai })
                  }
                >
                  ✔
                </button>
              </div>
            ))}
            {errors[i]?.answers && (
              <p className="error-msg">{errors[i].answers}</p>
            )}
            {errors[i]?.correct && (
              <p className="error-msg">{errors[i].correct}</p>
            )}

            <h3 className="question-label">CONTEXT TEXTS</h3>
            {q.answers.map((a, ai) => (
              <textarea
                key={ai}
                className="pixel-input"
                value={a.contextText}
                placeholder="Story text that appears after choosing this answer..."
                onChange={(e) => {
                  const list = [...q.answers];
                  list[ai].contextText = e.target.value;
                  update(i, { ...q, answers: list });
                }}
              />
            ))}
            {errors[i]?.contextTexts && (
              <p className="error-msg">{errors[i].contextTexts}</p>
            )}

            <button
              className="pixel-btn pink small-remove"
              onClick={() => remove(i)}
            >
              DELETE QUESTION
            </button>
          </div>
        ))}

        <button className="pixel-btn blue wide-btn" onClick={add}>
          + ADD QUESTION
        </button>
        <button className="pixel-btn teal wide-btn" onClick={handleNext}>
          NEXT
        </button>
        <button
          className="pixel-btn pink small-btn"
          onClick={() => navigate("/create/intro")}
        >
          BACK
        </button>
      </div>
    </div>
  );
};

export default CreateQuestions;
