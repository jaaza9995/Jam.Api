import { useState } from "react";
import { useNavigate } from "react-router-dom";
import "./CreateIntro.css";
import useStoryCreation from "../storyCreation/StoryCreationContext";
import { saveIntro } from "../storyCreation/StoryCreationService";

const CreateIntro = () => {
  const navigate = useNavigate();
  const { data, setData } = useStoryCreation();

  const [title, setTitle] = useState(data.intro.title);
  const [description, setDescription] = useState(data.intro.description);
  const [introText, setIntroText] = useState(data.intro.introText);
  const [difficulty, setDifficulty] = useState(data.intro.difficulty);
  const [accessibility, setAccessibility] = useState(data.intro.accessibility);

  const [error, setError] = useState("");

  const validate = () => {
    if (!title.trim()) return "Title is required.";
    if (!description.trim()) return "Description is required.";
    if (!introText.trim()) return "Intro text is required.";
    if (difficulty === "" || difficulty === null) return "Please choose difficulty.";
    if (accessibility === "" || accessibility === null) return "Please choose accessibility.";
    return "";
  };

  const handleNext = async () => {
    const val = validate();
    if (val) {
      setError(val);
      return;
    }

    // Oppdater React Context
    setData((prev) => ({
      ...prev,
      intro: {
        title,
        description,
        introText,
        difficulty,
        accessibility,
      },
    }));

    // Send til backend session
    const res = await saveIntro({
      title,
      description,
      introText,
      difficultyLevel: Number(difficulty),
      accessibility: Number(accessibility),
    });

    if (!res.ok) {
      console.error("Failed to save intro");
      return;
    }

    navigate("/create/questions");
  };

  return (
    <div className="pixel-bg">
      <h1 className="intro-title">CREATE NEW GAME</h1>

      <div className="form-section">
        {error && <p className="error-msg">{error}</p>}

        <label className="input-label">TITLE</label>
        <input
          className="pixel-input"
          value={title}
          onChange={(e) => setTitle(e.target.value)}
        />

        <label className="input-label">DESCRIPTION</label>
        <textarea
          className="pixel-textarea"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
        />

        <label className="input-label">INTRO TEXT</label>
        <textarea
          className="pixel-textarea"
          value={introText}
          onChange={(e) => setIntroText(e.target.value)}
        />

        <label className="input-label">DIFFICULTY</label>
        <select
          className="pixel-select"
          value={difficulty}
          onChange={(e) => setDifficulty(e.target.value)}
        >
          <option value="">Select</option>
          <option value="0">Easy</option>
          <option value="1">Medium</option>
          <option value="2">Hard</option>
        </select>

        <label className="input-label">ACCESSIBILITY</label>
        <select
          className="pixel-select"
          value={accessibility}
          onChange={(e) => setAccessibility(e.target.value)}
        >
          <option value="">Select</option>
          <option value="0">Public</option>
          <option value="1">Private</option>
        </select>

        <div className="button-row-intro">
          <button className="pixel-btn pink" onClick={() => navigate("/")}>
            BACK
          </button>
          <button className="pixel-btn teal" onClick={handleNext}>
            NEXT
          </button>
        </div>
      </div>
    </div>
  );
};

export default CreateIntro;
