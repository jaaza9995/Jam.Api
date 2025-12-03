import React, { useEffect, useState, useRef } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getEndings, updateEndings } from "./storyEditingService";
import ConfirmUndoModal from "../shared/ConfirmUndoModal";
import "./Edit.css";
import "../App.css";
import { EndingsDto } from "../types/editStory";
import { parseBackendErrors } from "../utils/parseBackendErrors";

const EditEndingsPage: React.FC = () => {
	const { storyId } = useParams();
	const navigate = useNavigate();
	const hasLoadedRef = useRef(false);

	// TEXT FIELDS
	const [good, setGood] = useState("");
	const [neutral, setNeutral] = useState("");
	const [bad, setBad] = useState("");

	// ORIGINAL VALUES
	const [original, setOriginal] = useState<EndingsDto>({
		goodEnding: "",
		neutralEnding: "",
		badEnding: "",
	});

	const [errors, setErrors] = useState({
		good: "",
		neutral: "",
		bad: "",
	});

	const [backendError, setBackendError] = useState("");
	const [loading, setLoading] = useState(true);

	const [showUndoConfirm, setShowUndoConfirm] = useState(false);
	const [showSavedMsg, setShowSavedMsg] = useState(false);

	// "No changes" toast
	const [showNoChangesMsg, setShowNoChangesMsg] = useState(false);

	// ---------------------------
	// LOAD
	// ---------------------------
	useEffect(() => {
		if (hasLoadedRef.current) {
			return;
		}

		const load = async () => {
			hasLoadedRef.current = true;
			if (!storyId) return;

			const res = await getEndings(Number(storyId));
			if (!res.ok) {
				const parsed = parseBackendErrors(
					await res.json().catch(() => null)
				);

				setErrors({
					good: parsed.goodEnding || "",
					neutral: parsed.neutralEnding || "",
					bad: parsed.badEnding || "",
				});

				return;
			}

			const data = (await res.json()) as EndingsDto;

			setGood(data.goodEnding);
			setNeutral(data.neutralEnding);
			setBad(data.badEnding);

			setOriginal(data);
			setLoading(false);
		};

		load();
	}, [storyId]);

	// ---------------------------
	// CHANGES CHECK
	// ---------------------------
	const hasChanges = () =>
		good !== original.goodEnding ||
		neutral !== original.neutralEnding ||
		bad !== original.badEnding;

	// ---------------------------
	// VALIDATION
	// ---------------------------
	const validate = () => {
		const newErrors = { good: "", neutral: "", bad: "" };

		if (!good.trim()) newErrors.good = "Good ending is required.";
		if (!neutral.trim()) newErrors.neutral = "Neutral ending is required.";
		if (!bad.trim()) newErrors.bad = "Bad ending is required.";

		setErrors(newErrors);
		return !newErrors.good && !newErrors.neutral && !newErrors.bad;
	};

	// ---------------------------
	// SAVE
	// ---------------------------
	const handleSave = async () => {
		setBackendError("");

		if (!storyId) return;

		// 🔥 NEW: No changes toast
		if (!hasChanges()) {
			setShowNoChangesMsg(true);
			setTimeout(() => setShowNoChangesMsg(false), 4000);
			return;
		}

		if (!validate()) return;

		const payload: EndingsDto = {
			goodEnding: good,
			neutralEnding: neutral,
			badEnding: bad,
		};
		const res = await updateEndings(Number(storyId), payload);

		if (!res.ok) {
			const body = await res.json().catch(() => null);
			const parsed = parseBackendErrors(body);

			setErrors({
				good: parsed.goodEnding || "",
				neutral: parsed.neutralEnding || "",
				bad: parsed.badEnding || "",
			});

			// Global feilmelding (frivillig)
			setBackendError(Object.values(parsed)[0] || "");

			return;
		}

		// Update original
		setOriginal(payload);

		// Show success toast
		setShowSavedMsg(true);
		setTimeout(() => setShowSavedMsg(false), 4000);
	};

	// ---------------------------
	// BACK
	// ---------------------------
	const handleBack = () => {
		if (hasChanges()) setShowUndoConfirm(true);
		else navigate(`/edit/${storyId}`);
	};

	const confirmUndo = () => navigate(`/edit/${storyId}`);

	if (loading) return <div className="pixel-bg">Loading...</div>;

	// ---------------------------
	// RENDER
	// ---------------------------
	return (
		<div className="pixel-bg">
			{showUndoConfirm && (
				<ConfirmUndoModal
					onConfirm={confirmUndo}
					onCancel={() => setShowUndoConfirm(false)}
				/>
			)}

			{/* SUCCESS TOAST */}
			{showSavedMsg && <div className="saved-toast">Saved Changes</div>}

			{/*  NO CHANGES TOAST */}
			{showNoChangesMsg && (
				<div className="nochanges-toast">No changes have been done</div>
			)}

			<h1 className="title">EDIT ENDINGS</h1>

			{/* GOOD */}
			<div className="create-form"> 
					<h3 className="input-label">GOOD ENDING</h3>
					<textarea
						className="input-area longer-box"
						value={good}
						onChange={(e) => setGood(e.target.value)}
					/>
					{errors.good && <p className="error-msg">{errors.good}</p>}

				{/* NEUTRAL */}
				<h3 className="input-label">NEUTRAL ENDING</h3>
				<textarea
					className="input-area longer-box"
					value={neutral}
					onChange={(e) => setNeutral(e.target.value)}
				/>
				{errors.neutral && (
					<p className="error-msg">{errors.neutral}</p>
				)}
			

				{/* BAD */}
				<h3 className="input-label">BAD ENDING</h3>
				<textarea
					className="input-area longer-box"
					value={bad}
					onChange={(e) => setBad(e.target.value)}
				/>
				{errors.bad && <p className="error-msg">{errors.bad}</p>}
			

				<div className="nav-buttons">
					<button className="pixel-btn back" onClick={handleBack}>
						Back
					</button>
					<button className="pixel-btn save" onClick={handleSave}>
						Save Changes
					</button>
				</div>
			</div>
		</div>
	);
};

export default EditEndingsPage;
