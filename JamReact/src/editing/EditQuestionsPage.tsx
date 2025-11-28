import React, { useEffect, useState, useRef } from "react";
import { useParams, useNavigate } from "react-router-dom";
import "./Edit.css";
import { getQuestions, updateQuestions } from "./storyEditingService";
import ConfirmUndoModal from "../shared/ConfirmUndoModal";
import { parseBackendErrors } from "../utils/parseBackendErrors";
import {
	QuestionSceneDto as QuestionScene,
	AnswerOptionDto as AnswerOption,
} from "../types/editStory";

type QuestionErrors = {
	storyText?: string;
	questionText?: string;
	answers?: string;
	contextTexts?: string;
	correct?: string;
};

const mapParsedErrors = (
	parsed: Record<string, string>,
	existingLength: number
): QuestionErrors[] => {
	let maxIndex = existingLength - 1;
	const entries = Object.entries(parsed);

	entries.forEach(([key]) => {
		const match =
			key.match(/questionScenes\[(\d+)\]/i) || key.match(/^\[(\d+)\]/); // backend uses "[0].field" for List<UpdateQuestionSceneDto>

		if (!match) return;

		const idx = Number(match[1]);
		if (!Number.isNaN(idx)) maxIndex = Math.max(maxIndex, idx);
	});

	const newErrors: QuestionErrors[] = Array.from(
		{ length: Math.max(0, maxIndex + 1) },
		() => ({})
	);

	entries.forEach(([key, msg]) => {
		const match =
			key.match(/questionScenes\[(\d+)\]\.(.*)/i) ||
			key.match(/^\[(\d+)\]\.(.*)/);
		if (!match) return;

		const index = Number(match[1]);
		const field = match[2].toLowerCase();

		if (!newErrors[index]) newErrors[index] = {};

		if (field.includes("storytext")) newErrors[index].storyText = msg;
		if (field.includes("questiontext")) newErrors[index].questionText = msg;
		if (field.includes("answer")) newErrors[index].answers = msg;
		if (field.includes("context")) newErrors[index].contextTexts = msg;
		if (field.includes("correctanswerindex"))
			newErrors[index].correct = msg;
	});

	return newErrors;
};

const normalizeAnswers = (
	answers: AnswerOption[] | null | undefined
): AnswerOption[] => {
	if (!answers || answers.length === 0) {
		return [
			{ answerOptionId: 0, answerText: "", contextText: "" },
			{ answerOptionId: 0, answerText: "", contextText: "" },
			{ answerOptionId: 0, answerText: "", contextText: "" },
			{ answerOptionId: 0, answerText: "", contextText: "" },
		];
	} //returnerer fire answer option?

	const list = answers
		.map((a) => ({
			// Bruk en sjekk for å sikre at selv om AnswerOptionDto i teorien har valgfrie felter,
			// så har vi alltid gyldige standarder i UI-tilstanden
			answerOptionId: a.answerOptionId || 0,
			answerText: a.answerText || "",
			contextText: a.contextText || "",
		}))
		.slice(0, 4); // Trim til maks 4

	while (list.length < 4) {
		list.push({ answerOptionId: 0, answerText: "", contextText: "" });
	}

	return list;
};

const emptyQuestion = (): QuestionScene => ({
	//add question knappen
	questionSceneId: 0,
	storyText: "",
	questionText: "",
	answers: normalizeAnswers([]),
	correctAnswerIndex: -1,
});

// Map UI state to backend DTO shape
const toBackendPayload = (list: QuestionScene[], storyId: number) =>
	list.map((q) => ({
		storyId,
		questionSceneId: q.questionSceneId ?? 0,
		storyText: q.storyText,
		questionText: q.questionText,
		correctAnswerIndex: q.correctAnswerIndex,
		markedForDeletion: false,

		answers: normalizeAnswers(q.answers).map((a) => ({
			answerOptionId: a.answerOptionId ?? 0,
			answerText: a.answerText.trim(),
			contextText: a.contextText.trim(),
		})),
	}));

const EditQuestionsPage: React.FC = () => {
	const { storyId } = useParams();
	const navigate = useNavigate();
	const hasLoadedRef = useRef(false);

	const [questions, setQuestions] = useState<QuestionScene[]>([]);
	const [originalQuestions, setOriginalQuestions] = useState<QuestionScene[]>(
		[]
	);
	const [errors, setErrors] = useState<QuestionErrors[]>([]);
	const [backendError, setBackendError] = useState("");

	const [loading, setLoading] = useState(true);
	const [showUndoConfirm, setShowUndoConfirm] = useState(false);
	const [showSavedMsg, setShowSavedMsg] = useState(false);
	const [showNoChangesMsg, setShowNoChangesMsg] = useState(false);

	// -------------------------------
	// LOAD
	// -------------------------------
	useEffect(() => {
		if (hasLoadedRef.current) {
			return;
		}

		const load = async () => {
			hasLoadedRef.current = true;
			if (!storyId) return;

			const res = await getQuestions(Number(storyId));
			if (!res.ok) {
				let body: any = null;

				try {
					body = await res.json();
				} catch {
					setBackendError("Unexpected server error.");
					setLoading(false);
					return;
				}

				const parsed = parseBackendErrors(body);
				const newErrors = mapParsedErrors(parsed, questions.length);

				if (body?.errorTitle) {
					setBackendError(body.errorTitle);
				} else if (Object.keys(parsed).length > 0) {
					setBackendError(Object.values(parsed)[0]);
				} else {
					setBackendError("Failed to load questions.");
				}

				setErrors(newErrors);
				setLoading(false);
				return;
			}

			const data = (await res.json()) as QuestionScene[];

			const normalized = data.map((q) => ({
				...q,
				answers: normalizeAnswers(q.answers),
			}));

			// 1. Dyp kloning for å sette original tilstand
			const deepClone = JSON.parse(JSON.stringify(normalized));

			// 2. Initialiser tilstandene
			setQuestions(normalized);
			setOriginalQuestions(deepClone);
			setErrors(normalized.map(() => ({})));

			// 3. Avslutt lasting
			setLoading(false); // <--- Denne manglet også ved suksess
		};

		load();
	}, [storyId]);

	const hasChanges = () =>
		JSON.stringify(questions) !== JSON.stringify(originalQuestions);

	// -------------------------------
	// VALIDATION
	// -------------------------------
	const validate = (list: QuestionScene[]) => {
		const newErrors: QuestionErrors[] = [];

		list.forEach((q) => {
			const err: QuestionErrors = {};

			// --- STORY TEXT ---
			if (!q.storyText.trim()) {
				err.storyText = "Story context is required.";
			}

			// --- QUESTION TEXT ---
			if (!q.questionText.trim()) {
				err.questionText = "Question text is required.";
			}

			// --- ANSWERS (4 EXACT) ---
			if (!q.answers || q.answers.length !== 4) {
				err.answers = "Each question must have exactly 4 answers.";
			} else {
				// --- ANSWER TEXTS ---
				if (q.answers.some((a) => !a.answerText.trim())) {
					err.answers = "All 4 answer options must be filled.";
				}

				// --- CONTEXT TEXTS ---
				if (q.answers.some((a) => !a.contextText.trim())) {
					err.contextTexts = "All 4 context texts must be filled.";
				}
			}

			// --- CORRECT ANSWER ---
			if (q.correctAnswerIndex < 0 || q.correctAnswerIndex > 3) {
				err.correct = "Please choose a correct answer.";
			}

			newErrors.push(err);
		});

		setErrors(newErrors);
		return newErrors.every((e) => Object.keys(e).length === 0);
	};

	// -------------------------------
	// SAVE
	// -------------------------------

	const handleSave = async () => {
		setBackendError("");

		if (!storyId) return;

		if (!hasChanges()) {
			setShowNoChangesMsg(true);
			setTimeout(() => setShowNoChangesMsg(false), 3500);
			return;
		}

		// Normalize before validating
		const normalized: QuestionScene[] = questions.map((q) => ({
			...q,
			answers: normalizeAnswers(q.answers),
		}));

		if (!validate(normalized)) return;

		// Build payload
		const payload = toBackendPayload(normalized, Number(storyId));

		const res = await updateQuestions(Number(storyId), payload as any);

		// ----------------------------
		// BACKEND ERROR HANDLING
		// ----------------------------
		if (!res.ok) {
			let body: any = null;

			try {
				body = await res.json();
			} catch {
				setBackendError("Unexpected server error.");
				return;
			}

			const parsed = parseBackendErrors(body);

			// ModelState errors per spørsmål
			if (Object.keys(parsed).length > 0) {
				const newErrors = mapParsedErrors(parsed, questions.length);
				setErrors(newErrors);

				if (body?.errorTitle) setBackendError(body.errorTitle);
				else setBackendError(Object.values(parsed)[0]);
				return;
			}

			// Custom backend error
			if (body?.errorMessage) {
				setBackendError(body.errorMessage);
				return;
			}

			if (body?.errorTitle) setBackendError(body.errorTitle);
			else setBackendError("Something went wrong.");
			return;
		}

		// ----------------------------
		// SUCCESS → reload
		// ----------------------------
		const reloadRes = await getQuestions(Number(storyId));
		if (!reloadRes.ok) {
			setBackendError("Saved, but failed to reload.");
			return;
		}

		const reloaded = (await reloadRes.json()) as QuestionScene[];

		const normalizedReload = reloaded.map((q) => ({
			...q,
			answers: normalizeAnswers(q.answers),
		}));

		setQuestions(normalizedReload);
		setOriginalQuestions(JSON.parse(JSON.stringify(normalizedReload)));

		setShowSavedMsg(true);
		setTimeout(() => setShowSavedMsg(false), 3500);
	};

	// -------------------------------
	// ADD / DELETE
	// -------------------------------
	const handleAdd = () => {
		setQuestions((prev) => [...prev, emptyQuestion()]);
		setErrors((prev) => [...prev, {}]);
	};

	const handleDeleteScene = (sceneId: number, index: number) => {
		if (questions.length === 1) {
			alert("You must have at least one question.");
			return;
		}

		// Bare fjern fra frontend-state – slettingen skjer når vi trykker Save
		setQuestions((prev) => prev.filter((_, i) => i !== index));
		setErrors((prev) => prev.filter((_, i) => i !== index));
	};

	// -------------------------------
	// BACK
	// -------------------------------
	const handleBack = () => {
		if (hasChanges()) setShowUndoConfirm(true);
		else navigate(`/edit/${storyId}`);
	};

	const confirmUndo = () => navigate(`/edit/${storyId}`);

	if (loading) return <div className="pixel-bg">Loading...</div>;

	// -------------------------------
	// RENDER
	// -------------------------------
	return (
		<div className="pixel-bg edit-container">
			{showUndoConfirm && (
				<ConfirmUndoModal
					onConfirm={confirmUndo}
					onCancel={() => setShowUndoConfirm(false)}
				/>
			)}

			{backendError && <p className="error-msg">{backendError}</p>}

			{showSavedMsg && <div className="saved-toast">Saved Changes</div>}
			{showNoChangesMsg && (
				<div className="nochanges-toast">No changes have been done</div>
			)}

			<h1 className="edit-title">Edit Questions</h1>

			{questions.map((q, i) => (
				// if QuestionSceneId is 0 (new question) use the index so keys are unique
				<div
					key={q.questionSceneId !== 0 ? q.questionSceneId : i}
					className="question-scene-card"
				>
					{/* STORY CONTEXT */}
					<h3 className="question-label">STORY CONTEXT</h3>
					<textarea
						className="pixel-input"
						value={q.storyText}
						onChange={(e) => {
							const updated = [...questions];
							updated[i] = {
								...updated[i],
								storyText: e.target.value,
							};
							setQuestions(updated);

							const copy = [...errors];
							copy[i] = { ...copy[i], storyText: "" };
							setErrors(copy);
						}}
					/>
					{errors[i]?.storyText && (
						<p className="error-msg">{errors[i]?.storyText}</p>
					)}

					{/* QUESTION */}
					<h3 className="question-label">QUESTION</h3>
					<textarea
						className="pixel-input"
						value={q.questionText}
						onChange={(e) => {
							const updated = [...questions];
							updated[i] = {
								...updated[i],
								questionText: e.target.value,
							};
							setQuestions(updated);

							const copy = [...errors];
							copy[i] = { ...copy[i], questionText: "" };
							setErrors(copy);
						}}
					/>
					{errors[i]?.questionText && (
						<p className="error-msg">{errors[i]?.questionText}</p>
					)}

					{/* ANSWERS */}
					<h3 className="question-label">ANSWER OPTIONS</h3>

					{q.answers.map((a, idx) => (
						<div className="answer-row" key={idx}>
							<input
								className="pixel-input"
								value={a.answerText}
								onChange={(e) => {
									const updated = [...questions];
									const answers = [...updated[i].answers];
									answers[idx] = {
										...answers[idx],
										answerText: e.target.value,
									};
									updated[i] = { ...updated[i], answers };
									setQuestions(updated);

									const copy = [...errors];
									copy[i] = { ...copy[i], answers: "" };
									setErrors(copy);
								}}
							/>

							<button
								className={
									q.correctAnswerIndex === idx
										? "correct-toggle correct-toggle--active"
										: "correct-toggle"
								}
								onClick={() => {
									const updated = [...questions];
									updated[i] = {
										...updated[i],
										correctAnswerIndex: idx,
									};
									setQuestions(updated);

									const copy = [...errors];
									copy[i] = { ...copy[i], correct: "" };
									setErrors(copy);
								}}
							>
								✓
							</button>
						</div>
					))}

					{errors[i]?.answers && (
						<p className="error-msg">{errors[i]?.answers}</p>
					)}
					{errors[i]?.correct && (
						<p className="error-msg">{errors[i]?.correct}</p>
					)}

					{/* CONTEXT TEXTS */}
					<h3 className="question-label">CONTEXT TEXTS</h3>
					{q.answers.map((a, idx) => (
						<textarea
							key={idx}
							className="pixel-input"
							value={a.contextText}
							onChange={(e) => {
								const updated = [...questions];
								const answers = [...updated[i].answers];
								answers[idx] = {
									...answers[idx],
									contextText: e.target.value,
								};
								updated[i] = { ...updated[i], answers };
								setQuestions(updated);

								const copy = [...errors];
								copy[i] = { ...copy[i], contextTexts: "" };
								setErrors(copy);
							}}
						/>
					))}

					{errors[i]?.contextTexts && (
						<p className="error-msg">{errors[i]?.contextTexts}</p>
					)}

					<button
						className="pixel-btn pink delete-scene-btn"
						onClick={() => handleDeleteScene(q.questionSceneId, i)}
					>
						DELETE QUESTION
					</button>
				</div>
			))}

			<div className="edit-buttons">
				<button className="pixel-btn teal" onClick={handleAdd}>
					Add Question
				</button>
				<button className="pixel-btn teal" onClick={handleSave}>
					Save Changes
				</button>
				<button className="pixel-btn blue" onClick={handleBack}>
					Back
				</button>
			</div>
		</div>
	);
};

export default EditQuestionsPage;
