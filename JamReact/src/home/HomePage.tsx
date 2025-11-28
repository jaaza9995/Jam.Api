import React, { useEffect, useState, useRef } from "react";
import { useAuth } from "../auth/AuthContext";
import "./HomePage.css";
import { useNavigate } from "react-router-dom";
import { Story } from "../types/home";
import { fetchHomePageData } from "./homePageService";
import { UnauthorizedError } from "./homePageService";
import StoryStatsModal from "./StoryStatsModal";

const HomePage: React.FC = () => {
	const { token, logout } = useAuth();
	const navigate = useNavigate();
	const hasLoadedRef = useRef(false);

	const [stories, setStories] = useState<Story[]>([]);
	const [recentlyPlayed, setRecentlyPlayed] = useState<Story[]>([]);
	const [showStoryStatsModal, setShowStoryStatsModal] = useState(false);
	const [selectedStory, setSelectedStory] = useState<Story | null>(null);
	const [firstName, setFirstName] = useState<string>("");
	const [loading, setLoading] = useState<boolean>(true);
	const [error, setError] = useState<string | null>(null);

	useEffect(() => {
		if (hasLoadedRef.current) {
			return;
		}

		const load = async () => {
			hasLoadedRef.current = true;
			setLoading(true);
			setError(null);

			try {
				const { data, error } = await fetchHomePageData(token!);

				if (error || !data) {
					setError(error ?? "Failed to load homepage.");
					setLoading(false);
					return;
				}

				setFirstName(data.firstName || "Player");
				setStories(data.yourStories || []);

				setRecentlyPlayed(
					(data.recentlyPlayed || []).sort(
						(a: Story, b: Story) =>
							new Date(b.lastPlayed as any).getTime() -
							new Date(a.lastPlayed as any).getTime()
					)
				);
			} catch (err) {
				if (err instanceof UnauthorizedError) {
					console.warn("UnauthorizedError caught, logging out.");
					logout();
					setError(err.message);
				} else {
					// Håndterer andre ukjente feil
					setError("An unexpected error occurred.");
				}
			} finally {
				setLoading(false);
			}
		};

		load();
	}, [token]);

	return (
		<div className="pixel-bg">
			{showStoryStatsModal && selectedStory && (
				<StoryStatsModal
					storyTitle={selectedStory.title}
					played={selectedStory.played}
					finished={selectedStory.finished}
					failed={selectedStory.failed}
					dnf={selectedStory.dnf}
					onConfirm={() => setShowStoryStatsModal(false)}
					onCancel={() => setShowStoryStatsModal(false)}
				/>
			)}

			<h1 className="homepage-title">WELCOME TO MATH UNIVERSE</h1>
			{error && <p className="error-text">{error}</p>}

			<div className="homepage-buttons">
				<button
					className="btn-bigHome make"
					onClick={() => navigate("/create/intro")}
				>
					MAKE NEW GAME
				</button>
				<button
					className="btn-bigHome add"
					onClick={() => navigate("/browse")}
				>
					BROWSE GAMES
				</button>
			</div>

			{/* ================= YOUR GAMES ================ */}
			<section className="section-block">
				<h2 className="section-title">YOUR GAMES:</h2>

				{stories.length === 0 ? (
					<p className="empty-text">No stories found.</p>
				) : (
					<ul className="story-list">
						{stories.map((s) => (
							<li
								key={s.storyId}
								className="story-card"
								onClick={() => {
									setSelectedStory(s);
									setShowStoryStatsModal(true);
								}}
                                style={{ cursor: "pointer" }}
							>
								<div className="story-content">
									<h3>{s.title}</h3>
									<p>{s.description}</p>
								</div>
								<div className="bottom-info">
									<div className="diff-qc-row">
										<span className="question-count">
											<p>Questions: {s.questionCount}</p>
										</span>

										{/* DIFFICULTY */}
										<span className="difficulty">
											{s.difficultyLevel === 0 && (
												<button className="difficulty easy">
													Easy
												</button>
											)}

											{s.difficultyLevel === 1 && (
												<button className="difficulty medium">
													Medium
												</button>
											)}

											{s.difficultyLevel === 2 && (
												<button className="difficulty hard">
													Hard
												</button>
											)}
										</span>
									</div>

									{/* PRIVATE CODE */}
									{s.accessibility === 1 && (
										<p className="private-code">
											Game Code: {s.code}
										</p>
									)}
								</div>

								{/* Buttons */}
								<div className="story-buttons">
									<button
										className="pixel-btn edit"
										onClick={() =>
											navigate(`/edit/${s.storyId}`)
										}
									>
										EDIT
									</button>

									<button
										className="pixel-btn play"
										onClick={() =>
											navigate(`/play/${s.storyId}`)
										}
									>
										PLAY
									</button>
								</div>
							</li>
						))}
					</ul>
				)}
			</section>

			{/* ================= RECENTLY PLAYED ================ */}
			<section className="section-block">
				<h2 className="section-title">RECENTLY PLAYED:</h2>

				{recentlyPlayed.length === 0 ? (
					<p className="empty-text">No recently played games.</p>
				) : (
					<ul className="story-list">
						{recentlyPlayed.map((s) => (
							<li key={s.storyId} className="story-card">
								<div className="story-content">
									<h3>{s.title}</h3>
									<p>{s.description}</p>
								</div>
								<div className="bottom-info">
									<div className="diff-qc-row">
										<span className="question-count">
											<p>Questions: {s.questionCount}</p>
										</span>
										{/* DIFFICULTY */}
										<span className="difficulty">
											{s.difficultyLevel === 0 && (
												<button className="difficulty easy">
													Easy
												</button>
											)}

											{s.difficultyLevel === 1 && (
												<button className="difficulty medium">
													Medium
												</button>
											)}

											{s.difficultyLevel === 2 && (
												<button className="difficulty hard">
													Hard
												</button>
											)}
										</span>
									</div>
									{/* PRIVATE CODE */}
									{s.accessibility === 1 && (
										<p className="private-code">
											Game Code: {s.code}
										</p>
									)}
								</div>

								{/* Buttons */}
								<div className="story-buttons">
									<button
										className="pixel-btn play"
										onClick={() =>
											navigate(`/play/${s.storyId}`)
										}
									>
										PLAY
									</button>
								</div>
							</li>
						))}
					</ul>
				)}
			</section>
		</div>
	);
};

export default HomePage;
