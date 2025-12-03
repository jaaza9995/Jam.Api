import React, { useEffect, useState, useRef, useCallback } from "react";
import { useParams } from "react-router-dom";
import { IPlayScene, ISessionState, IAnswerOption } from "../types/storyPlaying";
import { SceneType } from "../types/enums";
import { useNavigate } from "react-router-dom";
import {
	startSession as startSessionService,
	fetchScene as fetchSceneService,
	submitAnswer as submitAnswerService,
	AnswerResult,
} from "./StoryPlayingService";
import "./StoryPlayer.css";

export const StoryPlayer: React.FC = () => {
	const { storyId } = useParams<{ storyId: string }>();

	// Konvertere streng til tall
	const storyIdNumber = parseInt(storyId || "0", 10);

	// Initial validitetssjekk
	if (!storyIdNumber) {
		return <div>Feil: Story ID missing or invalid.</div>;
	}

	// 1. Game State (keys for API-call)
	const [session, setSession] = useState<ISessionState>({
		sessionId: null,
		currentSceneId: null,
		currentSceneType: null,
		score: 0,
		level: 3,
	});

	// 2. Data for the current scene (the content to be displayed)
	const [currentSceneData, setCurrentSceneData] = useState<IPlayScene | null>(
		null
	);

	// 3. Loading and Error Condition
	const [isLoading, setIsLoading] = useState<boolean>(false);
	const [error, setError] = useState<string | null>(null);

	// 4. User's chosen AnswerOption
	const [selectedOption, setSelectedOption] = useState<IAnswerOption | null>(
		null
	);

	// 5. Temporary state to display the feedback text after user has chosen an AnswerOption
	const [feedbackText, setFeedbackText] = useState<string | null>(null);

	// 6. Condition to mark that the game is finished
	const [isGameOver, setIsGameOver] = useState<boolean>(false);

	// 7. Ref to prevent double calls in Strict Mode
	const sessionStarted = useRef(false);

	const [nextSceneKeys, setNextSceneKeys] = useState<{
		id: number | null;
		type: SceneType | null;
	} | null>(null);

	const navigate = useNavigate();

	// Function to start a new game session
	const startSession = useCallback(async () => {
		// Check if the session has already started
		if (session.sessionId) return;

		setIsLoading(true);
		setError(null);

		try {
			// Call the service layer
			const data = await startSessionService(storyIdNumber);

			setSession((prev: ISessionState) => ({
				...prev,
				sessionId: data.sessionId,
				currentSceneId: data.sceneId,
				currentSceneType: data.sceneType,
			}));
		} catch (e: any) {
			setError(e.message);
		} finally {
			setIsLoading(false);
		}
	}, [storyId, session.sessionId]);

	// Function to start the first QuestionScene after the IntroScene
	const startFirstQuestion = () => {
		// Checks if we have data and are in the IntroScene
		if (!currentSceneData || currentSceneData.sceneType !== SceneType.Intro)
			return;

		// Get the ID provided by the API
		const nextId = currentSceneData.nextSceneAfterIntroId;

		if (!nextId) {
			setError("Unable to start the game, story is incomplete.");
			console.error(
				"DEVELOPER ERROR: First question ID (nextSceneAfterIntroId) was missing after IntroScene."
			);
			return;
		}

		// Updates state: This automatically triggers fetchScene via useEffect
		setSession((prev: ISessionState) => ({
			...prev,
			currentSceneId: nextId,
			currentSceneType: SceneType.Question as SceneType,
		}));
	};

	// Function to navigate to the next scene after feedback
	const goToNextScene = () => {
		if (
			!nextSceneKeys ||
			nextSceneKeys.id === null ||
			nextSceneKeys.type === null
		) {
			setError("Unable to continue the game. Game progress is broken.");
			console.error(
				"DEVELOPER ERROR: Next Scene ID/Type missing after successful response."
			);
			return;
		}

		// Reset feedback and navigation keys
		setFeedbackText(null);
		setSelectedOption(null);
		setNextSceneKeys(null);

		// Set new session to trigger fetchScene
		setSession((prev: ISessionState) => ({
			...prev,
			currentSceneId: nextSceneKeys.id,
			currentSceneType: nextSceneKeys.type,
		}));
	};

	// --- useEffect 1: Starts the game (runs once) ---
	useEffect(() => {
		// Ensures we have a storyId and that we haven't run startSession before
		if (storyId && !sessionStarted.current) {
			sessionStarted.current = true;
			startSession();
		}
	}, [storyId, startSession]);

	// --- useEffect 2: Gets scene content (runs on change in scene keys) ---
	useEffect(() => {
		// Only runs if all keys are set (as they are by startSession)
		if (
			session.sessionId &&
			session.currentSceneId !== null &&
			session.currentSceneType !== null
		) {
			fetchScene(session.currentSceneId, session.currentSceneType);
		}
	}, [session.sessionId, session.currentSceneId, session.currentSceneType]);

	// Function to retrieve scene data (called internally in useEffect)
	const fetchScene = async (
		sceneId: number | null,
		sceneType: SceneType | null
	) => {
		// 1. Input validation checks state
		if (
			sceneId === null ||
			sceneId === undefined ||
			sceneType === null ||
			sceneType === undefined ||
			session.sessionId === null ||
			session.sessionId === undefined
		) {
			return;
		}

		setIsLoading(true);
		setError(null);
		setFeedbackText(null); // Hide feedback when loading new scene

		try {
			// 2. Call the service layer
			const data = await fetchSceneService(
				sceneId,
				sceneType,
				session.sessionId
			);

			// 3. Update the state with scene data
			setCurrentSceneData(data);

			// Check if this is an EndingScene
			if (data.sceneType === SceneType.Ending) {
				setIsGameOver(true);
			}
		} catch (e: any) {
			setError(e.message);
		} finally {
			setIsLoading(false);
		}
	};

	// Function to send selected response to API
	const handleAnswer = async (selectedAnswer: IAnswerOption) => {
		// Check that we have a valid session
		if (!session.sessionId) {
			setError("The game session has not started.");
			return;
		}

		if (
			!selectedAnswer ||
			selectedAnswer.answerOptionId === undefined ||
			selectedAnswer.answerOptionId === null
		) {
			setError("Cannot submit answer: Answer ID is missing.");
			return;
		}
		const answerId = selectedAnswer.answerOptionId;

		setIsLoading(true);
		setError(null);

		try {
			// Call the service layer
			const result: AnswerResult = await submitAnswerService(
				session.sessionId,
				answerId
			);

			// --- Handle Game Over (Loss) ---
			if (result.type === "Game Over") {
				setIsGameOver(true);
				setCurrentSceneData(null);
				const gameOverScore = result.score;

				setSession((prev: ISessionState) => ({
					...prev,
					score: gameOverScore,
					currentSceneId: null,
					currentSceneType: null,
				}));

				// We're done, stop further processing
				setIsLoading(false);
				return;
			}

			// --- Handle standard feedback(result.type === 'Feedback') ---
			const feedback = result.feedback;

			// 1. Update score and level
			setSession((prev: ISessionState) => ({
				...prev,
				score: feedback.newScore,
				level: feedback.newLevel,
			}));

			// 2. Update currentSceneData with the keys for the next scene
			setCurrentSceneData((prevData) => {
				if (!prevData) return null;
				return {
					...prevData,
					nextSceneId: feedback.nextSceneId,
					nextSceneType: feedback.nextSceneType,
				};
			});

			// 3. Save the navigation keys for the next scene
			setNextSceneKeys({
				id: feedback.nextSceneId,
				type: feedback.nextSceneType,
			});

			// 4. Show the feedback text/scene and stop loading
			setFeedbackText(feedback.sceneText);

			// 5. Set timer to see if we need to call handleFinishStory
			setTimeout(() => {}, 2000);
		} catch (e: any) {
			// Catches errors (4xx/5xx) thrown from the service layer
			setError(e.message);
		} finally {
			setIsLoading(false);
		}
	};

	// --- Return ---
	return (
		<div className="pixel-bg">
			<div className="story-player">
		
				{isLoading && <p>Loading...</p>}
				{error && <p className="error">Error: {error}</p>}
		
				{/* =========================
					GAME OVER / ENDING
				========================= */}
				{isGameOver && (
					<div className="end-box">
						{/* LOSS */}
						{!currentSceneData ? (
							<>
								<h2 className="game-over-text">Game Over — You Lost!</h2>
								<p>Your final score: {session.score}</p>
		
								<button className="pixel-btn back" onClick={() => navigate("/")}>
									Return Home
								</button>
							</>
						) : (
							/* WIN / ENDING */
							<>
								<h2>Ending</h2>
								<p className="scene-text">{currentSceneData.sceneText}</p>
		
								<p className="score-text">Your final score: {session.score} of {currentSceneData.maxScore}</p>
		
								<button className="pixel-btn back" onClick={() => navigate("/")}>
									Return Home
								</button>
							</>
						)}
					</div>
				)}
		
				{/* =========================
					MAIN GAME VIEW
				========================= */}
				{!isLoading && !error && !isGameOver && currentSceneData && (
					<div className="scene-box">
		
						{/* Level + Score */}
						<p className="top-info">
							Level: {session.level} | Score: {session.score} / {currentSceneData.maxScore}
						</p>
		
						{/* =========================
							INTRO SCENE
						========================= */}
						{currentSceneData.sceneType === SceneType.Intro && (
							<>
								<h3 className="intro-text">Introduction</h3>
								<p className="scene-text">{currentSceneData.sceneText}</p>
		
								<button
									className="pixel-btn next"
									onClick={startFirstQuestion}
									disabled={isLoading || !currentSceneData.nextSceneAfterIntroId}
								>
									Next
								</button>
							</>
						)}
		
						{/* =========================
							QUESTION SCENE
						========================= */}
						{currentSceneData.sceneType === SceneType.Question && (
							<div className="question-scene-container">
		
								{/* FEEDBACK MODE */}
								{feedbackText ? (
									<>
										<div className="feedback-box">
											<p className="feedback-text">{feedbackText}</p>
										</div>
		
										<button
											className="pixel-btn next"
											onClick={goToNextScene}
											disabled={isLoading}
										>
											Next
										</button>
									</>
								) : (
									/* QUESTION MODE */
									<>
										<p className="scene-text">{currentSceneData.sceneText}</p>
										<p className="question-text">{currentSceneData.question}</p>
		
										{/* ANSWER BUTTONS */}
										<div className="answer-options">
											{currentSceneData.answerOptions?.map((option) => (
												<button
													key={option.answerOptionId}
													onClick={() => setSelectedOption(option)}
													className={`answer-btn ${
														selectedOption?.answerOptionId === option.answerOptionId
															? "selected-answer"
															: ""
													}`}
													disabled={isLoading}
												>
													{option.answer}
												</button>
											))}
										</div>
		
										{/* SUBMIT BUTTON */}
										<button
											className="pixel-btn next"
											onClick={() => {
												if (selectedOption) {
													handleAnswer(selectedOption);
													setSelectedOption(null);
												}
											}}
											disabled={!selectedOption || isLoading}
										>
											Send Answer
										</button>
									</>
								)}
							</div>
						)}
					</div>
				)}
			</div>
	</div>
	);
}
	