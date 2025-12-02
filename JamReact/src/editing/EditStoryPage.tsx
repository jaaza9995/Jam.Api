import React, { useEffect, useState, useRef } from "react";
import { useParams, useNavigate, useLocation } from "react-router-dom";
import { getStoryMetadata, deleteStory } from "./storyEditingService";
import { useAuth } from "../auth/AuthContext";
import DeleteModal from "../shared/DeleteModal";
import { parseBackendErrors } from "../utils/parseBackendErrors";
import { getDifficultyLevelString } from "../utils/enumHelpers";

import { StoryMetadataDto } from "../types/editStory";



import StoryCard from "../components/StoryCard";
import "../components/StoryCard.css";
import "./Edit.css";
import "../Create/Create.css";


const EditStoryPage: React.FC = () => {
	const { storyId } = useParams();
	const navigate = useNavigate();
	const location = useLocation();
	const { token } = useAuth();

	const hasLoadedRef = useRef(false);

	const [meta, setMeta] = useState<StoryMetadataDto | null>(null);
	const [metaErrors, setMetaErrors] = useState<{
		title?: string;
		description?: string;
	}>({});

	const [showDeleteModal, setShowDeleteModal] = useState(false);

	useEffect(() => {
		const state = location.state as { meta: StoryMetadataDto } | undefined;

		// 1. Check if we have fresh data from navigation state
		if (state?.meta) {
			hasLoadedRef.current = true;
			setMeta(state.meta);
			return; // Done, avoiding load() and API calls
		}

		// 2. If we DO NOT have data from state, check if we have already run load
		if (hasLoadedRef.current) {
			return;
		}

		// 3. Fallback/First Mount (API call required)
		const load = async () => {
			hasLoadedRef.current = true;

			const res = await getStoryMetadata(Number(storyId));
			if (!res.ok) {
				const backend = await res.json().catch(() => null);
				const parsed = parseBackendErrors(backend);

				setMetaErrors({
					title: parsed.Title,
					description: parsed.Description,
				});

				return;
			}
			const data = await res.json();
			setMeta(data);
		};

		load();
	}, [storyId, token, location.key, location.state]);

	if (!meta) return <div className="pixel-bg">Loading...</div>;

	const isPrivate = meta.accessibility === 1;
	const code = meta.code ?? null;
	const questionCount = meta.questionCount;

	const handleDelete = async () => {
		const res = await deleteStory(Number(storyId));

		if (res.ok) {
			navigate("/");
		} else {
			alert("Failed to delete story.");
		}
	};

	const navigateToIntro = () => {
		// Send meta (StoryMetadataDto) as state to EditIntro page
		navigate(`/edit/${storyId}/intro`, { state: { meta: meta } });
	};

	return (
		<div className="pixel-bg">
			{showDeleteModal && (
				<DeleteModal
					title={meta.title}
					onConfirm={handleDelete}
					onCancel={() => setShowDeleteModal(false)}
				/>
			)}
			<h2 className="title">EDIT GAME</h2>

			<div className="edit-story-card-wrapper">
				{/* LEFT - THE METADATA CARD */}
				<div className="story-card-container">
					<StoryCard
						key={meta.storyId}
						story={meta}
						showPlayButton={false}
					/>
				</div>

				{/* RIGHT – BUTTONS UNDER EACH OTHER */}
				<div className="right-buttons">
					<button
						className="edit-btn"
						onClick={() => navigateToIntro()}
					>
						Edit Intro
					</button>

					<button
						className="edit-btn"
						onClick={() => navigate(`/edit/${storyId}/questions`)}
					>
						Edit Questions
					</button>

					<button
						className="edit-btn"
						onClick={() => navigate(`/edit/${storyId}/endings`)}
					>
						Edit Endings
					</button>

					<button
						className="pixel-btn delete"
						onClick={() => setShowDeleteModal(true)}
					>
						DELETE GAME
					</button>
				</div>
			</div>

			{/* BACK BUTTON AT THE VERY BOTTOM LEFT */}
			<button
				className="pixel-btn back"
				onClick={() => navigate("/")}
			>
				BACK TO HOME
			</button>
		</div>
	);
};

export default EditStoryPage;
