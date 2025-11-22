import { useState } from "react";
import { useNavigate } from "react-router-dom";
import "./Create.css";
import useStoryCreation from "../storyCreation/StoryCreationContext";
import { saveEndings, completeStory } from "../storyCreation/StoryCreationApi";

const CreateEndings = () => {
  const navigate = useNavigate();
  const { data, setData } = useStoryCreation();

  const [good, setGood] = useState(data.endings.good);
  const [neutral, setNeutral] = useState(data.endings.neutral);
  const [bad, setBad] = useState(data.endings.bad);

  const [error, setError] = useState("");

  const validate = () => {
    if (!good.trim()) return "Good ending is required.";
    if (!neutral.trim()) return "Neutral ending is required.";
    if (!bad.trim()) return "Bad ending is required.";
    return "";
  };

  const handleFinish = async () => {
    const val = validate();
    if (val) {
      setError(val);
      return;
    }

    // Oppdater React context (lokal state)
    setData((prev) => ({
      ...prev,
      endings: { good, neutral, bad },
    }));

    // 1. Lagre endings i backend-session
    const res1 = await saveEndings({
      goodEnding: good,
      neutralEnding: neutral,
      badEnding: bad,
    });

    if (!res1.ok) {
      console.error("Failed to save endings");
      return;
    }

    // 2. Fullfør story (lagre i databasen)
    const res2 = await completeStory();

    if (!res2.ok) {
      console.error("Failed to complete story");
      return;
    }

    const json = await res2.json();
    console.log("Story created:", json);

    // 3. Naviger hjem etter fullført lagring
    navigate("/");
  };

  return (
    <div className="pixel-bg">
      <h1 className="pixel-title">CREATE ENDINGS</h1>

      {error && <p className="error-msg">{error}</p>}

      <div className="ending-wrapper">

        <div className="ending-block">
          <h3 className="ending-label">GOOD ENDING</h3>
          <textarea
            className="ending-input"
            value={good}
            onChange={(e) => setGood(e.target.value)}
          />
        </div>

        <div className="ending-block">
          <h3 className="ending-label">NEUTRAL ENDING</h3>
          <textarea
            className="ending-input"
            value={neutral}
            onChange={(e) => setNeutral(e.target.value)}
          />
        </div>

        <div className="ending-block">
          <h3 className="ending-label">BAD ENDING</h3>
          <textarea
            className="ending-input"
            value={bad}
            onChange={(e) => setBad(e.target.value)}
          />
        </div>

        <div className="ending-buttons">
          <button
            className="pixel-btn pink side-btn"
            onClick={() => navigate("/create/questions")}
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
