import { useState } from "react";
import { useNavigate } from "react-router-dom";
import "./Create.css";
import useStoryCreation from "../storyCreation/StoryCreationContext";
import { saveIntro } from "../storyCreation/StoryCreationService";
import { IntroDto, IntroErrors } from "../types/createStory";

const CreateIntro = () => {
  const navigate = useNavigate();
  const { data, setData } = useStoryCreation();

  const [title, setTitle] = useState(data.intro.title);
  const [description, setDescription] = useState(data.intro.description);
  const [introText, setIntroText] = useState(data.intro.introText);
  const [difficulty, setDifficulty] = useState(data.intro.difficulty);
  const [accessibility, setAccessibility] = useState(data.intro.accessibility);

  const [errors, setErrors] = useState<IntroErrors>({
    title: "",
    description: "",
    introText: "",
    difficulty: "",
    accessibility: "",
  });

  const validate = () => {
    const newErrors: IntroErrors = {
      title: "",
      description: "",
      introText: "",
      difficulty: "",
      accessibility: "",
    };

    if (!title.trim()) newErrors.title = "You must write a Title for your game.";
    if (!description.trim()) newErrors.description = "You must write a Description for your game";
    if (!introText.trim()) newErrors.introText = "You must write a Intro Text for your game";
    if (!difficulty) newErrors.difficulty = "Please choose difficulty.";
    if (!accessibility) newErrors.accessibility = "Please choose accessibility.";

    setErrors(newErrors);
    return Object.values(newErrors).every(v => v === "");
  };

  const handleNext = async () => {
    if (!validate()) return;

    // update context
    setData(prev => ({
      ...prev,
      intro: {
        title,
        description,
        introText,
        difficulty,
        accessibility,
      },
    }));

    const payload: IntroDto = {
      title,
      description,
      introText,
      difficultyLevel: Number(difficulty),
      accessibility: Number(accessibility),
    };

    const res = await saveIntro(payload);

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
        

        <label className="input-label">TITLE</label>
        <input
          className="pixel-input"
          value={title}
          placeholder="Enter the title of your game..."
          onChange={(e) => {
            setTitle(e.target.value)
            setErrors((prev) => ({ ...prev, title: "" }));  
            }}
        />
        {errors.title && <p className="error-msg">{errors.title}</p>}

        <label className="input-label">DESCRIPTION</label>
        <textarea
          className="pixel-textarea"
          value={description}
          placeholder="Write a description..."
          onChange={(e) => {
            setDescription(e.target.value)
            setErrors((prev) => ({ ...prev, description: "" }));
        }}
        />
        {errors.description && <p className="error-msg">{errors.description}</p>}
        <label className="input-label">INTRO TEXT</label>
        <textarea
          className="pixel-textarea"
          value={introText}
          placeholder="Write the intro story the player sees first..."
          onChange={(e) => {
            setIntroText(e.target.value)
            setErrors((prev) => ({ ...prev, introText: "" }));
        }}
        />
        {errors.introText && <p className="error-msg">{errors.introText}</p>}

        <label className="input-label">DIFFICULTY</label>
        <select
          className="pixel-select"
          value={difficulty}
          onChange={(e) => {
            setDifficulty(e.target.value)
            setErrors((prev) => ({ ...prev, difficulty: "" }));
        }}
        >
          <option value="" disabled hidden>Select difficulty...</option>
          <option value="0">Easy</option>
          <option value="1">Medium</option>
          <option value="2">Hard</option>
        </select>
        {errors.difficulty && <p className="error-msg">{errors.difficulty}</p>}


        <label className="input-label">ACCESSIBILITY</label>
        <select
          className="pixel-select"
          value={accessibility}
          onChange={(e) => {
            setAccessibility(e.target.value)
          setErrors((prev) => ({ ...prev, accessibility: "" }));
        }}
        >
          <option value="" disabled hidden>Select Accessibility...</option>
          <option value="0">Public</option>
          <option value="1">Private</option>
        </select>
        {errors.accessibility && <p className="error-msg">{errors.accessibility}</p>}


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
