import React, { useEffect, useState, useRef } from "react";
import { useAuth } from "../auth/AuthContext";
import { useNavigate } from "react-router-dom";
import { Story } from "../types/home";
import { fetchHomePageData } from "./homePageService";
import { UnauthorizedError } from "./homePageService";
import StoryStatsModal from "./StoryStatsModal";

import StoryCard from "../components/StoryCard";
import "./HomePage.css";
import "../components/StoryCard.css";


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

      <h1 className="title">WELCOME TO MATH UNIVERSE</h1>
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
        <div className="story-card-container">

          {stories.map((story) => (
            <StoryCard key={story.storyId} story={story} showEditButton={true}/>
          ))}
        </div>

        )}
      </section>

			{/* ================= RECENTLY PLAYED ================ */}
			<section className="section-block">
				<h2 className="section-title">RECENTLY PLAYED:</h2>

        {recentlyPlayed.length === 0 ? (
          <p className="empty-text">No recently played games.</p>
        ) : (
          <div className="story-card-container">
            {recentlyPlayed.map((story) => (
              <StoryCard key={story.storyId} story={story}/> /* EDIT-knapp skjult */
            ))}
          </div>
        )}
      </section>
    </div>
  );
};

export default HomePage;