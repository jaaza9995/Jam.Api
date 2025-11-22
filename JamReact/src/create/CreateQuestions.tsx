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

const CreateQuestions = () => {
  const navigate = useNavigate();
  const { data, setData } = useStoryCreation();

  const [questions, setQuestions] = useState(
    data.questions.length > 0 ? data.questions : [emptyQuestion()]
  );

  const [errors, setErrors] = useState<{ [key: number]: string }>({});

  const validate = () => {
  const err: { [key: number]: string } = {};

  questions.forEach((q, i) => {
    // Valider én og én, og stopp når du finner en feil
      if (!q.storyText.trim()) {
        err[i] = "Story context is required.";
        return;
      }

      if (!q.questionText.trim()) {
        err[i] = "Question text is required.";
        return;
      }

      if (q.answers.some(a => !a.answerText.trim())) {
        err[i] = "All 4 answer options must be filled.";
        return;
      }

      if (q.answers.some(a => !a.contextText.trim())) {
        err[i] = "All 4 context texts must be filled.";
        return;
      }

      if (q.correctAnswerIndex < 0) {
        err[i] = "Please choose a correct answer.";
        return;
      }
    });

    setErrors(err);
    return Object.keys(err).length === 0;
  };

  const handleNext = async () => {
    if (!validate()) return;

    // mappe til formatet backend forventer (QuestionSceneBaseDto)
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

    // lagre i context slik at brukeren kan gå tilbake
    setData((prev) => ({ ...prev, questions }));

    navigate("/create/endings");
  };

  const update = (index: number, updated: any) => {
    const copy = [...questions];
    copy[index] = updated;
    setQuestions(copy);
  };

  const remove = (index: number) => {
    if (questions.length === 1) return;
    setQuestions(questions.filter((_, i) => i !== index));
  };

  const add = () => {
    setQuestions([...questions, emptyQuestion()]);
  };

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
              onChange={(e) => update(i, { ...q, storyText: e.target.value })}
            />
            {errors[i] && errors[i].includes("Story") && (
              <p className="error-msg">{errors[i]}</p>
            )}

            <h3 className="question-label">QUESTION</h3>
            <textarea
              className="pixel-input"
              value={q.questionText}
              onChange={(e) =>
                update(i, { ...q, questionText: e.target.value })
              }
            />
            {errors[i] && errors[i].includes("Question") && (
              <p className="error-msg">{errors[i]}</p>
            )}

            <h3 className="question-label">ANSWER OPTIONS</h3>
            {q.answers.map((a, ai) => (
              <div className="answer-row" key={ai}>
                <input
                  className="pixel-input-small"
                  value={a.answerText}
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

            {errors[i] && errors[i].includes("answer options") && (
              <p className="error-msg">{errors[i]}</p>
            )}
            {errors[i] && errors[i].includes("correct") && (
              <p className="error-msg">{errors[i]}</p>
            )}

            <h3 className="question-label">CONTEXT TEXTS</h3>
            {q.answers.map((a, ai) => (
              <textarea
                key={ai}
                className="pixel-input"
                value={a.contextText}
                onChange={(e) => {
                  const list = [...q.answers];
                  list[ai].contextText = e.target.value;
                  update(i, { ...q, answers: list });
                }}
              />
            ))}

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
