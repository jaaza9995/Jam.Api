import { useState } from "react";
import { useNavigate } from "react-router-dom";
import useStoryCreation from "./StoryCreationContext";
import { saveEndings, finishCreation } from "./StoryCreationService";
import { EndingDto, EndingErrors } from "../types/createStory";
import { useToast } from "../shared/ToastContext";
import { parseBackendErrors } from "../utils/parseBackendErrors";
import FormErrorMessage from "../components/FormErrorMessage";
import "./Create.css";

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

		if (!good.trim())
			newErrors.good = "Oops, your forgot to write a good ending.";
		if (!neutral.trim())
			newErrors.neutral = "Oops, your forgot to write a neutral ending.";
		if (!bad.trim())
			newErrors.bad = "Oops, your forgot to write a bad ending.";

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

			{/* GOOD ENDING */}
			<div className="create-form">
				<h3 className="input-label">GOOD ENDING</h3>
				<textarea
					className="input-area longer-box"
					value={good}
					placeholder="Write the good ending..."
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
				<h3 className="input-label">NEUTRAL ENDING</h3>
				<textarea
					className="input-area longer-box"
					value={neutral}
					placeholder="Write the neutral ending..."
					onChange={(e) => {
						setNeutral(e.target.value);
						setErrors((prev) => ({ ...prev, neutral: "" }));
						setData((prev) => ({
							...prev,
							endings: {
								...prev.endings,
								neutral: e.target.value,
							},
						}));
					}}
				/>
				<FormErrorMessage message={errors.neutral} />

				{/* BAD ENDING */}
				<h3 className="input-label">BAD ENDING</h3>
				<textarea
					className="input-area longer-box"
					value={bad}
					placeholder="Write the bad ending..."
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

				{/* BUTTONS */}
				<div className="nav-buttons">
					<button
						className="pixel-btn back"
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

					<button className="pixel-btn edit" onClick={handleFinish}>
						FINISH
					</button>
				</div>
			</div>
		</div>
	);
};

export default CreateEndings;
