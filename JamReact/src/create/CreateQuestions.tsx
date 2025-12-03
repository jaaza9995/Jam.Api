import { useState } from "react";
import { useNavigate } from "react-router-dom";
import useStoryCreation from "./StoryCreationContext";
import { saveQuestions } from "./StoryCreationService";
import { parseBackendErrors } from "../utils/parseBackendErrors";
import "./Create.css";

import {
	QuestionSceneDto,
	QuestionErrors,
	QuestionScenesPayload,
} from "../types/createStory";

// -----------------------------
// HELPERS
// -----------------------------
const emptyQuestion = (): QuestionSceneDto => ({
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

const emptyErrors = (): QuestionErrors => ({
	storyText: "",
	questionText: "",
	answers: "",
	contextTexts: "",
	correct: "",
});

const CreateQuestions = () => {
	const navigate = useNavigate();
	const { data, setData } = useStoryCreation();

	const [questions, setQuestions] = useState<QuestionSceneDto[]>(
		data.questions.length > 0 ? data.questions : [emptyQuestion()]
	);

	// errors is ARRAY: one error object per question
	const [errors, setErrors] = useState<QuestionErrors[]>(() =>
		data.questions.length > 0
			? data.questions.map(() => emptyErrors())
			: [emptyErrors()]
	);

	// -----------------------------
	// VALIDATION
	// -----------------------------
	const validate = () => {
		const errList: QuestionErrors[] = questions.map((q) => {
			const e: QuestionErrors = {};

      if (!q.storyText.trim()) e.storyText = "Option outcome is required.";
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

		return errList.every((e) => Object.keys(e).length === 0);
	};

	// -----------------------------
	// NEXT
	// -----------------------------
	const handleNext = async () => {
		if (!validate()) return;

		const payload: QuestionScenesPayload = {
			questionScenes: questions.map((q) => ({
				storyText: q.storyText,
				questionText: q.questionText,
				correctAnswerIndex: q.correctAnswerIndex,
				answers: q.answers.map((a) => ({
					answerText: a.answerText,
					contextText: a.contextText,
				})),
			})),
		};

		const res = await saveQuestions(payload);

		if (!res.ok) {
			const backendJson = await res.json().catch(() => null);

			// Backend errors = ModelState → MÅ parses
			const parsed = parseBackendErrors(backendJson);

			// apply SAME error to ALL questions (backend gir ikke per index validering)
			const backendErrorObject: QuestionErrors = {
				storyText: parsed.storyText || "",
				questionText: parsed.questionText || "",
				answers: parsed.answers || "",
				contextTexts: parsed.contextTexts || "",
				correct: parsed.correctAnswerIndex || "",
			};

			// store error for every question UI
			setErrors(questions.map(() => backendErrorObject));

			return;
		}

		setData((prev) => ({ ...prev, questions }));
		navigate("/create/endings");
	};

	// -----------------------------
	// UPDATERS
	// -----------------------------
	const update = (index: number, updated: QuestionSceneDto) => {
		const copy = [...questions];
		copy[index] = updated;
		setQuestions(copy);

		const newErrors = [...errors];
		newErrors[index] = {}; // clear errors for that question only
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
			<h1 className="title">CREATE QUESTIONS</h1>

			<div className="questions-wrapper">
				{questions.map((q, i) => (
				<div className="question-box" key={i}>
					
					{/* LEAD-UP TO QUESTION */}
					<h3 className="input-label">LEAD-UP</h3>
					<textarea className="input-area"
						value={q.storyText}
						placeholder={`Write Some Lead-up to Your Question Here...`}
						onChange={(e) => update(i, { ...q, storyText: e.target.value })}
					/>
					{errors[i]?.storyText && <p className="error-msg">{errors[i].storyText}</p>}

					{/* QUESTION TEXT */}
					<h3 className="input-label">QUESTION</h3>
					<textarea className="input-area"
						value={q.questionText}
						placeholder={`Write Your Question here...`}
						onChange={(e) => update(i, { ...q, questionText: e.target.value })}
					/>
					{errors[i]?.questionText && <p className="error-msg">{errors[i].questionText}</p>}

					{/* ANSWERS */}
					<h3 className="input-label">ANSWER OPTIONS</h3>

					{q.answers.map((a, ai) => (
						<div className="answer-row" key={ai}>

							{/* Answer text */}
							<input className="input-area"
								placeholder={`Write Answer ${ai + 1}...`}
								value={a.answerText}
								onChange={(e) => {
									const list = [...q.answers];
									list[ai].answerText = e.target.value;
									update(i, { ...q, answers: list });
								}}
							/>

							{/* Answer outcome */}
							<input className="input-area"
								placeholder={`Outcome of Option ${ai + 1}...`}
								value={a.contextText}
								onChange={(e) => {
									const list = [...q.answers];
									list[ai].contextText = e.target.value;
									update(i, { ...q, answers: list });
								}}
							/>

							{/* Correct button */}
							<button
								className={`correct-btn ${
									q.correctAnswerIndex === ai
										? "selected"
										: ""
								}`}
								onClick={() =>
									update(i, { ...q, correctAnswerIndex: ai,
									})
								}
							>
								✔
							</button>
						</div>
					))}

					{/* ERRORS for answer list */}
					{errors[i]?.answers && (
						<p className="error-msg">{errors[i].answers}</p>
					)}
					{errors[i]?.correct && (
						<p className="error-msg">{errors[i].correct}</p>
					)}

					{/* ERROR for context texts */}
					{errors[i]?.contextTexts && (
						<p className="error-msg">
							{errors[i].contextTexts}
						</p>
					)}

					<button
					className="delete-q-button"
					onClick={() => remove(i)}
					>
						DELETE QUESTION
					</button>
				</div>
				))}
			</div>
			
			<div className="nav-buttons">
				<button className="pixel-btn addQuestion" onClick={add}>
					+ ADD QUESTION
				</button>
			</div>

			<div className="nav-buttons">
				<button className="pixel-btn back"
					onClick={() => {
					setData(prev => ({ ...prev, questions }));
					navigate("/create/intro");
					}}
				>
					BACK
				</button>
			
				<button className="pixel-btn next" onClick={handleNext}>
					NEXT
				</button>

			</div>
    	</div>
  	);
};

export default CreateQuestions;
