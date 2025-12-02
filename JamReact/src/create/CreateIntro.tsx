import { useState } from "react";
import { useNavigate } from "react-router-dom";
import useStoryCreation from "./StoryCreationContext";
import { saveIntro } from "./StoryCreationService";
import { IntroDto, IntroErrors } from "../types/createStory";
import { parseBackendErrors } from "../utils/parseBackendErrors";
import FormErrorMessage from "../components/FormErrorMessage";
import "./Create.css";

const CreateIntro: React.FC = () => {
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

		if (!title.trim())
			newErrors.title = "You must write a Title for your game.";
		if (!description.trim())
			newErrors.description =
				"You must write a Description for your game.";
		if (!introText.trim())
			newErrors.introText = "You must write an Intro Text for your game.";
		if (!difficulty) newErrors.difficulty = "Please choose difficulty.";
		if (!accessibility)
			newErrors.accessibility = "Please choose accessibility.";

		setErrors(newErrors);
		return Object.values(newErrors).every((v) => v === "");
	};

	const handleNext = async () => {
		if (!validate()) return;

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

		const payload: IntroDto = {
			title,
			description,
			introText,
			difficultyLevel: Number(difficulty),
			accessibility: Number(accessibility),
		};

		const res = await saveIntro(payload);

		if (!res.ok) {
			const parsed = parseBackendErrors(res.data);

			setErrors({
				title: parsed.title || "",
				description: parsed.description || "",
				introText: parsed.introText || "",
				difficulty: parsed.difficultyLevel || "",
				accessibility: parsed.accessibility || "",
			});

			return;
		}

		navigate("/create/questions");
	};

  return (
    <div className="pixel-bg">
      <div className="create-form"> 
        <h1 className="title">CREATE NEW GAME</h1>
        {/* TITLE */}
        <label className="input-label">TITLE</label>
        <input
          className="input-area"
          value={title}
          placeholder="Enter the title of your game..."
          onChange={(e) => {
            setTitle(e.target.value);
            setErrors(prev => ({ ...prev, title: "" }));
          }}
        />
        <FormErrorMessage message={errors.title} />

        {/* DESCRIPTION */}
        <label className="input-label">DESCRIPTION</label>
        <textarea
          className="input-area longer-box"
          value={description}
          placeholder="Write a description..."
          onChange={(e) => {
            setDescription(e.target.value);
            setErrors(prev => ({ ...prev, description: "" }));
          }}
        />
        <FormErrorMessage message={errors.description} />

        {/* INTRO TEXT */}
        <label className="input-label">INTRO TEXT</label>
        <textarea
          className="input-area longer-box"
          value={introText}
          placeholder="Write the intro story..."
          onChange={(e) => {
            setIntroText(e.target.value);
            setErrors(prev => ({ ...prev, introText: "" }));
          }}
        />
        <FormErrorMessage message={errors.introText} />

        {/* DIFFICULTY */}
        <label className="input-label">DIFFICULTY</label>
        <div className="dropdown-wrapper">
          <select
            className="select-dropdown"
            value={difficulty}
            onChange={(e) => {
              setDifficulty(e.target.value);
              setErrors(prev => ({ ...prev, difficulty: "" }));
            }}
          >
            <option value="" disabled hidden>Select difficulty...</option>
            <option value="0">Easy</option>
            <option value="1">Medium</option>
            <option value="2">Hard</option>
          </select>
        </div>
        <FormErrorMessage message={errors.difficulty} />

        {/* ACCESSIBILITY */}
        <label className="input-label">ACCESSIBILITY</label>
        <div className="dropdown-wrapper">
          <select
            className="select-dropdown"
            value={accessibility}
            onChange={(e) => {
              setAccessibility(e.target.value);
              setErrors(prev => ({ ...prev, accessibility: "" }));
            }}
          >
            <option value="" disabled hidden>Select accessibility...</option>
            <option value="0">Public</option>
            <option value="1">Private</option>
          </select>
        </div>
        <FormErrorMessage message={errors.accessibility} />

        {/* BUTTONS */}
        <div className="nav-buttons">
          <button className="pixel-btn back" onClick={() => navigate("/")}>
            BACK
          </button>
          <button className="pixel-btn next" onClick={handleNext}>
            NEXT
          </button>
        </div>
      </div>
    </div>
  );
};

export default CreateIntro;
