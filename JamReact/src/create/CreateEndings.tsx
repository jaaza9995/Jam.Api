import { useState } from "react";
import { useNavigate } from "react-router-dom";
import "./Create.css";
import useStoryCreation from "../storyCreation/StoryCreationContext";
import { saveEndings } from "../storyCreation/StoryCreationService";
import { EndingDto, EndingErrors } from "../types/createStory";

const CreateEndings = () => {
  const navigate = useNavigate();
  const { data, setData } = useStoryCreation();

  const [good, setGood] = useState(data.endings.good);
  const [neutral, setNeutral] = useState(data.endings.neutral);
  const [bad, setBad] = useState(data.endings.bad);

  const [errors, setErrors] = useState<EndingErrors>({
    good: "",
    neutral: "",
    bad: "",
  });

  const validate = () => {
    const newErrors: EndingErrors = { good: "", neutral: "", bad: "" };

    if (!good.trim()) newErrors.good = "Opps, your forgot to write a good ending.";
    if (!neutral.trim()) newErrors.neutral = "Opps, your forgot to write a neutral ending.";
    if (!bad.trim()) newErrors.bad = "Opps, your forgot to write a bad ending.";

    setErrors(newErrors);
    return !newErrors.good && !newErrors.neutral && !newErrors.bad;
  };

  const handleFinish = async () => {
    if (!validate()) return;

    setData((prev) => ({
      ...prev,
      endings: { good, neutral, bad },
    }));

    const payload: EndingDto = {
      goodEnding: good,
      neutralEnding: neutral,
      badEnding: bad,
    };

    const res = await saveEndings(payload);

    if (!res.ok) {
      console.error("Failed to save endings");
      return;
    }

    navigate("/");
  };


  // -----------------------
  // RENDER
  // -----------------------
  return (
    <div className="pixel-bg">
      <h1 className="pixel-title">CREATE ENDINGS</h1>

      <div className="ending-wrapper">

        <div className="ending-block">
          <h3 className="ending-label">GOOD ENDING</h3>
          <textarea
            className="ending-input"
            value={good}
            placeholder="Write the good ending for your story..."
            onChange={(e) => {
              setGood(e.target.value);
              setData((prev) => ({
                ...prev,
                endings: { ...prev.endings, good: e.target.value },
              }));
              setErrors((prev) => ({ ...prev, good: "" }));
            }}
          />
          {errors.good && <p className="error-msg">{errors.good}</p>}
        </div>

        <div className="ending-block">
          <h3 className="ending-label">NEUTRAL ENDING</h3>
          <textarea
            className="ending-input"
            value={neutral}
            placeholder="Write the neutral ending..."
            onChange={(e) => {
              setNeutral(e.target.value);
              setData((prev) => ({
                ...prev,
                endings: { ...prev.endings, neutral: e.target.value },
              }));
              setErrors((prev) => ({ ...prev, neutral: "" }));
            }}
          />
          {errors.neutral && <p className="error-msg">{errors.neutral}</p>}
        </div>

        <div className="ending-block">
          <h3 className="ending-label">BAD ENDING</h3>
          <textarea
            className="ending-input"
            value={bad}
            placeholder="Write the BAD ending..."
            onChange={(e) => {
              setBad(e.target.value);
              setData((prev) => ({
                ...prev,
                endings: { ...prev.endings, bad: e.target.value },
              }));
              setErrors((prev) => ({ ...prev, bad: "" }));
            }}
          />
          {errors.bad && <p className="error-msg">{errors.bad}</p>}
        </div>

        <div className="ending-buttons">
          <button
            className="pixel-btn pink side-btn"
            onClick={() => {
              // persist current endings before going back
              setData((prev) => ({
                ...prev,
                endings: { good, neutral, bad },
              }));
              navigate("/create/questions");
            }}
          >
            BACK
          </button>

          <button className="pixel-btn teal side-btn" onClick={handleFinish}>
            FINISH
          </button>
        </div>
      </div>
    </div>
  );
};

export default CreateEndings;
