import { useState } from "react";
import { useNavigate } from "react-router-dom";
import useStoryCreation from "./StoryCreationContext";
import { saveEndings, finishCreation } from "./StoryCreationService";
import { EndingDto, EndingErrors } from "../types/createStory";
import { useToast } from "../shared/ToastContext";
import { parseBackendErrors } from "../utils/parseBackendErrors";
import FormErrorMessage from "../components/FormErrorMessage";
import "../App.css";

const CreateEndings: React.FC = () => {
  
  const navigate = useNavigate();
  const { data, setData } = useStoryCreation();
  const { showToast } = useToast();

  const [good, setGood] = useState(data.endings?.good ?? "");
  const [neutral, setNeutral] = useState(data.endings?.neutral ?? "");
  const [bad, setBad] = useState(data.endings?.bad ?? "");

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
    return Object.values(newErrors).every((e) => e === "");
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

    // SAVE ENDINGS (Backend validation)
    const res = await saveEndings(payload);

    if (!res.ok) {
    const body = await res.json().catch(() => null);
    const parsed = parseBackendErrors(body);

    setErrors({
      good: parsed.goodEnding || "",
      neutral: parsed.neutralEnding || "",
      bad: parsed.badEnding || "",
    });

    return;
  }

    // COMPLETE CREATION
    const createRes = await finishCreation();

    if (!createRes.ok) {
      console.error("Failed to create story:", await createRes.text());
      return;
    }

    showToast("Story created!");
    navigate("/");
  };

  return (
    <div className="pixel-bg">
      <h1 className="title">CREATE ENDINGS</h1>

      <div className="ending-wrapper">

        {/* GOOD ENDING */}
        <div className="ending-block">
          <h3 className="ending-label">GOOD ENDING</h3>
          <textarea
            className="ending-input"
            value={good}
            placeholder="Write the good ending for your story..."
            onChange={(e) => {
              setGood(e.target.value);
              setErrors((prev) => ({ ...prev, good: "" }));
              setData((prev) => ({
                ...prev,
                endings: { ...prev.endings, good: e.target.value },
              }));
            }}
          />
          <FormErrorMessage message={errors.good} />


        {/* NEUTRAL ENDING */}

          <h3 className="ending-label">NEUTRAL ENDING</h3>
          <textarea
            className="ending-input"
            value={neutral}
            placeholder="Write the neutral ending..."
            onChange={(e) => {
              setNeutral(e.target.value);
              setErrors((prev) => ({ ...prev, neutral: "" }));
              setData((prev) => ({
                ...prev,
                endings: { ...prev.endings, neutral: e.target.value },
              }));
            }}
          />
          <FormErrorMessage message={errors.neutral} />


        {/* BAD ENDING */}

          <h3 className="ending-label">BAD ENDING</h3>
          <textarea
            className="ending-input"
            value={bad}
            placeholder="Write the BAD ending..."
            onChange={(e) => {
              setBad(e.target.value);
              setErrors((prev) => ({ ...prev, bad: "" }));
              setData((prev) => ({
                ...prev,
                endings: { ...prev.endings, bad: e.target.value },
              }));
            }}
          />
          <FormErrorMessage message={errors.bad} />
        </div>

        {/* BUTTONS */}
        <div className="button-row">
          <button
            className="pixel-btn pixel-btn-back"
            onClick={() => {
              setData((prev) => ({
                ...prev,
                endings: { good, neutral, bad },
              }));
              navigate("/create/questions");
            }}
          >
            BACK
          </button>

          <button className="pixel-btn pixel-btn-finish" onClick={handleFinish}>
            FINISH
          </button>
        </div>
      </div>
    </div>
  );
};

export default CreateEndings;
