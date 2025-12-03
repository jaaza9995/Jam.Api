import React, { useEffect, useState, useRef } from "react";
import { useAuth } from "../auth/AuthContext";
import { useNavigate } from "react-router-dom";
import PlayConfirmModal from "../shared/PlayConfirmModal";
import "./BrowsePage.css";

import { StoryCard as BrowseStoryCard } from "../types/storyCard";
import { fetchPublicStories, fetchPrivateStory } from "./BrowsePageService";
import StoryCard from "../components/StoryCard";
import "../components/StoryCard.css";

const BrowsePage: React.FC = () => {
	const { token } = useAuth();
	const navigate = useNavigate();

	const hasFetchedPublicGames = useRef(false);

	const [publicGames, setPublicGames] = useState<BrowseStoryCard[]>([]);
	const [publicSearch, setPublicSearch] = useState("");

	const [privateCode, setPrivateCode] = useState("");
	const [privateMatch, setPrivateMatch] = useState<BrowseStoryCard | null>(
		null
	);
	const [error, setError] = useState<string | null>(null);

	const [showModal, setShowModal] = useState(false);
	const [selectedGame, setSelectedGame] = useState<BrowseStoryCard | null>(
		null
	);

	const openModal = (game: any) => {
		setSelectedGame(game);
		setShowModal(true);
	};

	const closeModal = () => {
		setSelectedGame(null);
		setShowModal(false);
	};

	const confirmPlay = () => {
		navigate(`/play/${selectedGame!.storyId}`);
	};

	// ---------------------------
	// FETCH PUBLIC GAMES
	// ---------------------------
	useEffect(() => {
		if (hasFetchedPublicGames.current) {
			return;
		}

		const load = async () => {
			hasFetchedPublicGames.current = true;
			setError(null);

			const { data, error } = await fetchPublicStories(token!);
			if (error) {
				setError(error);
			}
			setPublicGames(data ?? []);
		};
		load();
	}, [token]);

	// ---------------------------
	// PUBLIC SEARCH
	// ---------------------------
	const filtered = publicGames.filter((g: BrowseStoryCard) => {
		const title = g.title.toLowerCase();
		return title.includes(publicSearch.toLowerCase());
	});

	// ---------------------------
	// PRIVATE CODE SEARCH
	// ---------------------------
	const handlePrivateSearch = async () => {
		if (!privateCode.trim()) return;

		setError(null);
		const { data, error } = await fetchPrivateStory(token!, privateCode);

		if (data) {
			setPrivateMatch(data);
			openModal(data);
		} else {
			setError(error ?? "No game found with this code.");
			setPrivateMatch(null);
		}
	};

	return (
		<div className="pixel-bg">
			<h1 className="title">Find a Game</h1>

			<div className="search-wrapper">
				{error && <p className="error-msg">{error}</p>}

				<div className="header-row">
					<h3 className="input-label">Public Games</h3>
					<h3 className="input-label">Private Games</h3>
				</div>

				<div className="browse-block">
					{/* PUBLIC SEARCH */}
					<input
						className="input-area"
						placeholder="Search by title..."
						value={publicSearch}
						onChange={(e) => setPublicSearch(e.target.value)}
					/>
					<div className="private-search">
						{/* PRIVATE GAMES */}
						<input
							className="input-area"
							placeholder="Enter game code..."
							value={privateCode}
							onChange={(e) => setPrivateCode(e.target.value)}
						/>
						<button
							className="search-btn"
							onClick={handlePrivateSearch}
						>
							Search
						</button>
					</div>
				</div>
			</div>

			{/* PUBLIC GAMES LIST */}
			<section className="section-block">
				{filtered.length === 0 ? (
					<p className="empty-text">No stories found.</p>
				) : (
					<div className="story-card-container">
						{filtered.map((story) => (
							<StoryCard
								key={story.storyId}
								story={story}
								showPlayButton={true}
							/>
						))}
					</div>
				)}
			</section>

			{/* MODAL */}
			<PlayConfirmModal
				title={selectedGame?.title || ""}
				show={showModal}
				onConfirm={confirmPlay}
				onCancel={closeModal}
			/>

			{/* BACK BUTTON AT THE VERY BOTTOM LEFT */}
			<div className="browse-back-btn">
				<button
					className="pixel-btn back"
					onClick={() => navigate("/")}
				>
					Back to Home
				</button>
			</div>
		</div>
	);
};

export default BrowsePage;
