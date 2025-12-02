import React, { useEffect, useState, useRef } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import { useAuth } from "../auth/AuthContext";
import "./Edit.css";
import { parseBackendErrors } from "../utils/parseBackendErrors";
import {
	getStoryMetadata,
	getIntro,
	updateIntroScene,
	updateStoryMetadata,
} from "./storyEditingService";
import ConfirmUndoModal from "../shared/ConfirmUndoModal";
import { StoryMetadataDto } from "../types/editStory";

const EditIntroPage: React.FC = () => {
	const { storyId } = useParams();
	const navigate = useNavigate();
	const location = useLocation();
	const { token } = useAuth();
	const hasLoadedRef = useRef(false);

	// METADATA
	const [title, setTitle] = useState<string>("");
	const [description, setDescription] = useState<string>("");
	const [difficulty, setDifficulty] = useState<number>(1);
	const [accessibility, setAccessibility] = useState<number>(0);

	// INTRO TEXT
	const [introText, setIntroText] = useState<string>("");

	// ORIGINAL VALUES
	const [original, setOriginal] = useState<{
		title: string;
		description: string;
		difficulty: number;
		accessibility: number;
		intro: string;
	} | null>(null);

	const [loading, setLoading] = useState<boolean>(true);
	const [backendError, setBackendError] = useState<string>("");
	const [showUndoConfirm, setShowUndoConfirm] = useState<boolean>(false);
	const [showSavedMsg, setShowSavedMsg] = useState<boolean>(false);

	// "No changes" toast
	const [showNoChangesMsg, setShowNoChangesMsg] = useState<boolean>(false);

	// State to hold the last loaded/saved metadata
	const [currentMetadata, setCurrentMetadata] =
		useState<StoryMetadataDto | null>(null);

	// ERRORS
	const [errors, setErrors] = useState({
		title: "",
		description: "",
		introText: "",
		difficulty: "",
		accessibility: "",
	});

	// ------------------------------------
	// VALIDATION
	// ------------------------------------
	const validate = (): boolean => {
		const newErrors = {
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
				"You must write a Description for your game";
		if (!introText.trim())
			newErrors.introText = "You must write an Intro Text for your game";

		if (difficulty !== 0 && difficulty !== 1 && difficulty !== 2)
			newErrors.difficulty = "Please choose difficulty.";
		if (accessibility !== 0 && accessibility !== 1)
			newErrors.accessibility = "Please choose accessibility.";

		setErrors(newErrors);
		return Object.values(newErrors).every((e) => e === "");
	};

	// ------------------------------------
	// LOAD FROM BACKEND
	// ------------------------------------
	useEffect(() => {
		if (hasLoadedRef.current) {
			return;
		}

		const load = async () => {
			hasLoadedRef.current = true;

			if (!storyId || !token) {
				setBackendError("Missing story or token");
				setLoading(false);
				return;
			}

			try {
				let meta: StoryMetadataDto | null = null;
				let metaLoadedFromState = false;

				// 1. Check nagivation state for meta (StoryMetadataDto)
				const state = location.state as
					| { meta: StoryMetadataDto }
					| undefined;
				if (state?.meta) {
					meta = state.meta;
					metaLoadedFromState = true;
				} else {
					// 2. Fallback if state is missing: fetch from backend
					try {
						const metaRes = await getStoryMetadata(Number(storyId));
						if (!metaRes.ok)
							throw new Error("Failed to load story metadata");
						meta = (await metaRes.json()) as StoryMetadataDto;
					} catch (err) {
						setBackendError("Failed to load story metadata.");
						setLoading(false);
						return;
					}
				}

				let introTextValue = "";
				try {
					const intro = await getIntro(Number(storyId), token);
					introTextValue = intro.introText ?? "";
				} catch (introErr) {
					console.warn("Intro missing, starting empty", introErr);
				}

				const parsedDifficulty = Number(meta.difficultyLevel);
				const parsedAccessibility = Number(meta.accessibility);

				setTitle(meta.title);
				setDescription(meta.description);
				setDifficulty(isNaN(parsedDifficulty) ? 1 : parsedDifficulty);
				setAccessibility(
					isNaN(parsedAccessibility) ? 0 : parsedAccessibility
				);
				setIntroText(introTextValue);

				setOriginal({
					title: meta.title,
					description: meta.description,
					difficulty: isNaN(parsedDifficulty) ? 1 : parsedDifficulty,
					accessibility: isNaN(parsedAccessibility)
						? 0
						: parsedAccessibility,
					intro: introTextValue,
				});
				setCurrentMetadata(meta!);
			} catch (err) {
				console.error(err);
				setBackendError("Could not load intro. Please try again.");
			} finally {
				setLoading(false);
			}
		};

		load();
	}, [storyId, token, location.state]);

	// ------------------------------------
	// CHANGE DETECTION
	// ------------------------------------
	const hasChanges = (): boolean => {
		if (!original) return false;
		return (
			title !== original.title ||
			description !== original.description ||
			difficulty !== original.difficulty ||
			accessibility !== original.accessibility ||
			introText !== original.intro
		);
	};

	// ------------------------------------
	// SAVE
	// ------------------------------------

	const handleSave = async () => {
		setBackendError("");

		// NEW: no change toast
		if (!hasChanges()) {
			setShowNoChangesMsg(true);
			setTimeout(() => setShowNoChangesMsg(false), 4000);
			return;
		}

		if (!storyId || !token) {
			setBackendError("Missing story or token");
			return;
		}

		if (!validate()) return;

		// -----------------------------
		// SAVE METADATA
		// -----------------------------
		const metaRes = await updateStoryMetadata(Number(storyId), {
			storyId: Number(storyId),
			title,
			description,
			difficultyLevel: difficulty,
			accessibility,
			questionCount: 0, // Not updating questions here
		});

		if (!metaRes.ok) {
			const body = await metaRes.json().catch(() => null);
			const parsed = parseBackendErrors(body);

			setErrors((prev) => ({
				...prev,
				title: parsed.title || "",
				description: parsed.description || "",
				introText: parsed.introText || "",
				difficulty: parsed.difficultyLevel || "",
				accessibility: parsed.accessibility || "",
			}));

			// Global error message only if no field error
			if (body?.errorTitle) setBackendError(body.errorTitle);
			else if (Object.keys(parsed).length === 0)
				setBackendError("Failed to save metadata");
			return;
		}

		// -----------------------------
		// SAVE INTRO
		// -----------------------------
		const introRes = await updateIntroScene(Number(storyId), introText);

		if (!introRes.ok) {
			const body = await introRes.json().catch(() => null);
			const parsed = parseBackendErrors(body);

			setErrors((prev) => ({
				...prev,
				introText: parsed.introText || "",
			}));

			if (body?.errorTitle) setBackendError(body.errorTitle);
			else if (Object.keys(parsed).length === 0)
				setBackendError("Failed to save intro");
			return;
		}

		// -----------------------------
		// SUCCESS
		// -----------------------------
		const updatedMeta = await getStoryMetadata(Number(storyId)).then((r) =>
			r.json()
		);

		// Set the updated meta data
		setCurrentMetadata(updatedMeta);

		setOriginal({
			title: updatedMeta.title,
			description: updatedMeta.description,
			difficulty: updatedMeta.difficultyLevel,
			accessibility: Number(updatedMeta.accessibility),
			intro: introText,
		});

		setShowSavedMsg(true);
		setTimeout(() => setShowSavedMsg(false), 5000);
	};

	// ------------------------------------
	// BACK BUTTON
	// ------------------------------------
	const handleBack = () => {
		if (hasChanges()) setShowUndoConfirm(true);
		else navigate(`/edit/${storyId}`, { state: { meta: currentMetadata } });
	};

	const confirmUndo = () => navigate(`/edit/${storyId}`);

	if (loading) return <div className="pixel-bg">Loading...</div>;

	// ------------------------------------
	// RENDER
	// ------------------------------------
	return (
		<div className="pixel-bg edit-container">
			{/* Undo modal */}
			{showUndoConfirm && (
				<ConfirmUndoModal
					onConfirm={confirmUndo}
					onCancel={() => setShowUndoConfirm(false)}
				/>
			)}

			{/* Saved toast */}
			{showSavedMsg && <div className="saved-toast">Saved Changes</div>}

			{/* No changes toast */}
			{showNoChangesMsg && (
				<div className="nochanges-toast">No changes have been done</div>
			)}

			<h1 className="title">Edit Story & Intro</h1>

			{backendError && <p className="error-msg">{backendError}</p>}

			{/* TITLE */}
			<label className="input-label">Title</label>
			<input
				className="pixel-textarea"
				value={title}
				onChange={(e) => {
					setTitle(e.target.value);
					setErrors((prev) => ({ ...prev, title: "" }));
				}}
			/>
			{errors.title && <p className="error-msg">{errors.title}</p>}

			{/* DESCRIPTION */}
			<label className="input-label">Description</label>
			<textarea
				className="pixel-textarea"
				value={description}
				onChange={(e) => {
					setDescription(e.target.value);
					setErrors((prev) => ({ ...prev, description: "" }));
				}}
			/>
			{errors.description && (
				<p className="error-msg">{errors.description}</p>
			)}

			{/* INTRO TEXT */}
			<label className="input-label">Introduction</label>
			<textarea
				className="pixel-textarea"
				value={introText}
				onChange={(e) => {
					setIntroText(e.target.value);
					setErrors((prev) => ({ ...prev, introText: "" }));
				}}
			/>
			{errors.introText && (
				<p className="error-msg">{errors.introText}</p>
			)}

			{/* DIFFICULTY */}
			<label className="input-label">Difficulty</label>
			<select
				className="pixel-textarea"
				value={difficulty}
				onChange={(e) => setDifficulty(Number(e.target.value))}
			>
				<option value={0}>Easy</option>
				<option value={1}>Medium</option>
				<option value={2}>Hard</option>
			</select>
			{errors.difficulty && (
				<p className="error-msg">{errors.difficulty}</p>
			)}

			{/* ACCESSIBILITY */}
			<label className="input-label">Accessibility</label>
			<select
				className="pixel-textarea"
				value={accessibility}
				onChange={(e) => setAccessibility(Number(e.target.value))}
			>
				<option value={0}>Public</option>
				<option value={1}>Private</option>
			</select>
			{errors.accessibility && (
				<p className="error-msg">{errors.accessibility}</p>
			)}

			<div className="edit-buttons">
				<button
					className="pixel-btn pixel-btn-back"
					onClick={handleBack}
				>
					Back
				</button>

				<button
					className="pixel-btn pixel-btn-saveChanges"
					onClick={handleSave}
				>
					Save Changes
				</button>
			</div>
		</div>
	);
};

export default EditIntroPage;
