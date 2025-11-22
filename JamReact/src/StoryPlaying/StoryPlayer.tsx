import React, { useEffect, useState, useRef, useCallback } from "react";
import { IPlayScene } from "./PlayScene";
import { IAnswerFeedback } from "./AnswerFeedback";
import { IStartSessionResponse } from "./StartSessionResponse";
import { ICalculateEndingResponse } from "./CalculateEndingResponse";
import { SceneType } from "./enums";
import { AnswerOption } from "./AnswerOption";
import { IErrorDto } from "./ErrorDto";

const API_URL = import.meta.env.VITE_API_URL;

const getAuthHeaders = () => {
	const token = localStorage.getItem("token");
	const headers: HeadersInit = {
		"Content-Type": "application/json",
	};
	if (token) {
		headers["Authorization"] = `Bearer ${token}`;
	}
	return headers;
};

interface StoryPlayerProps {
	storyId: number;
}

interface SessionState {
	sessionId: number | null;
	currentSceneId: number | null;
	currentSceneType: SceneType | null;
	score: number;
	level: number;
}

export const StoryPlayer: React.FC<StoryPlayerProps> = ({
	storyId,
}: StoryPlayerProps) => {
	// 1. Game State (keys for API-call)
	const [session, setSession] = useState<SessionState>({
		sessionId: null,
		currentSceneId: null,
		currentSceneType: null,
		score: 0,
		level: 3, // user always starts at level 3
	});

	// 2. Data for the current scene (the content to be displayed)
	const [currentSceneData, setCurrentSceneData] = useState<IPlayScene | null>(
		null
	);

	// 3. Loading and Error Condition
	const [isLoading, setIsLoading] = useState<boolean>(false);
	const [error, setError] = useState<string | null>(null);

	// 4. Temporary state to display the feedback text after user has chosen an AnswerOption
	const [feedbackText, setFeedbackText] = useState<string | null>(null);

	// 5. Condition to mark that the game is finished
	const [isGameOver, setIsGameOver] = useState<boolean>(false);

	// 6. Ref to prevent double calls in Strict Mode (NEW)
	const sessionStarted = useRef(false);

	// Denne funksjonen må nå ta inn state-verdier som avhengigheter hvis de brukes.
	const startSession = useCallback(async () => {
		// Sjekk om økten allerede har startet (ekstra sikkerhet, men useRef fikser problemet i useEffect)
		if (session.sessionId) return;

		setIsLoading(true);
		setError(null);

		try {
			const response = await fetch(
				`${API_URL}/api/StoryPlaying/start/${storyId}`,
				{
					method: "POST",
					headers: getAuthHeaders(),
				}
			);
			// ... (resten av logikken er den samme) ...

			const data: IStartSessionResponse = await response.json();

			setSession((prev: SessionState) => ({
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
	}, [storyId, session.sessionId]); // Avhengigheter: storyId og session.sessionId (for sjekken)

	// --- useEffect 1: Starter spillet (Kjører én gang) ---
	useEffect(() => {
		// Sikrer at vi har en storyId og at vi ikke har kjørt startSession før
		if (storyId && !sessionStarted.current) {
			sessionStarted.current = true; // Setter flagget til true FØR vi kaller funksjonen
			startSession(); // Kaller funksjonen som setter sessionId i state
		}

		// Denne avhenger av storyId, session.sessionId (hvis den skal sjekkes i useCallback) og startSession
	}, [storyId, startSession]);

	// --- useEffect 2: Henter sceneinnhold (Kjører ved endring i scenenøkler) ---
	useEffect(() => {
		// Kjører kun hvis ALLE nøklene er satt (som de blir av startSession)
		if (
			session.sessionId &&
			session.currentSceneId !== null &&
			session.currentSceneType !== null
		) {
			fetchScene(session.currentSceneId, session.currentSceneType);
		}

		// Avhengigheter: Dette er nøklene som trigger henting av ny scene
	}, [session.sessionId, session.currentSceneId, session.currentSceneType]);

	// Function to retrieve scene data (called internally in useEffect)
	const fetchScene = async (
		sceneId: number | null,
		sceneType: SceneType | null
	) => {
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
			// API-call: /api/StoryPlaying/scene?sceneId=X&sceneType=Y&sessionId=Z
			const response = await fetch(
				`${API_URL}/api/StoryPlaying/scene?sceneId=${sceneId}&sceneType=${sceneType}&sessionId=${session.sessionId}`,
				{
					method: "GET",
					headers: getAuthHeaders(),
				}
			);

			if (!response.ok) {
				// If the error is JSON formatted, read it for better message
				const errorBody: IErrorDto = await response.json();
				throw new Error(errorBody.errorTitle || "Error loading scene.");
			}

			const data: IPlayScene = await response.json();

			// Update the state with scene data
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
	const handleAnswer = async (selectedAnswer: AnswerOption) => {
		// Check that we have a valid session
		if (!session.sessionId) {
			setError("The game session has not started.");
			return;
		}

		setIsLoading(true);
		setError(null);

		try {
			// API-call: POST /api/StoryPlaying/answer
			const response = await fetch(`${API_URL}/api/StoryPlaying/answer`, {
				method: "POST",
				headers: getAuthHeaders(),
				body: JSON.stringify({
					// What the API expects in AnswerFeedbackDto
					sessionId: session.sessionId,
					selectedAnswerId: selectedAnswer.answerOptionId,
				}),
			});

			if (!response.ok) {
				// Handle Game Over on Level 1, which returns a special message
				if (response.status === 200) {
					// Check if the Game Over response was sent as Ok(200)
					const gameOverResponse = await response.json();
					if (gameOverResponse.message === "Game over") {
						setIsGameOver(true);
						// Update the final score, even if the game is over
						setSession((prev: SessionState) => ({
							...prev,
							score: gameOverResponse.score,
							currentSceneId: null,
							currentSceneType: null,
						}));
						setIsLoading(false);
						return;
					}
				}

				const errorBody: IErrorDto = await response.json();
				throw new Error(
					errorBody.errorTitle || "Error sending response."
				);
			}

			const feedback: IAnswerFeedback = await response.json();

			// First: Display the feedback text for AnswerOption
			setFeedbackText(feedback.sceneText);

			// Update score and level
			setSession((prev: SessionState) => ({
				...prev,
				score: feedback.newScore,
				level: feedback.newLevel,
			}));

			// Vent litt (f.eks. 2 sekunder) før du går til neste scene for å vise feedback
			setTimeout(() => {
				// Logikk for å sjekke om vi skal fortsette, avslutte eller fullføre
				if (!feedback.nextSceneId) {
					// Dette betyr at vi er ferdige med QuestionScenene, og skal til EndingScene (eller Game Over, som er håndtert over)
					handleFinishStory(); // Kaller funksjonen for å fullføre og vise EndingScene
					return;
				}

				// Sett ny scene-ID og type i state (dette trigger fetchScene via useEffect)
				setSession((prev: SessionState) => ({
					...prev,
					currentSceneId: feedback.nextSceneId,
					currentSceneType: feedback.nextSceneType,
				}));
			}, 2000); // 2000ms for å vise feedback
		} catch (e: any) {
			setError(e.message);
			setIsLoading(false);
		}
	};

	// Funksjon for å beregne og laste EndingScene ID
	const handleFinishStory = async () => {
		if (!session.sessionId) return;

		setIsLoading(true);
		setError(null);
		setFeedbackText(null);

		try {
			// 1. API-kall: GET /calculate-ending/{sessionId}
			// Backend velger, lagrer (i DB) og returnerer ID-en til sluttscenen.
			const response = await fetch(
				`${API_URL}/api/StoryPlaying/calculate-ending/${session.sessionId}`
			);

			if (!response.ok) {
				throw new Error("Error calculating final ending.");
			}

			const finishData: ICalculateEndingResponse = await response.json();

			// 2. Sett state til å hente den faktiske EndingScene dataen
			setSession((prev: SessionState) => ({
				...prev,
				currentSceneId: finishData.endingSceneId, // Bruk ID fra responsen
				currentSceneType: SceneType.Ending as SceneType,
			}));

			// useEffect vil trigge fetchScene(EndingSceneId, 'Ending')
		} catch (e: any) {
			setError(e.message);
			setIsLoading(false);
		}
	};

	const startFirstQuestion = () => {
		// Sjekker om vi har data og er i IntroScene
		if (!currentSceneData || currentSceneData.sceneType !== SceneType.Intro)
			return;

		// Henter ID-en som nå er levert av API'et
		const nextId = currentSceneData.nextSceneAfterIntroId;

		if (!nextId) {
			setError("Kan ikke starte: Første spørsmålsmanglet ID fra API.");
			return;
		}

		// Oppdaterer state: Dette trigger automatisk fetchScene via useEffect
		setSession((prev: SessionState) => ({
			...prev,
			currentSceneId: nextId,
			currentSceneType: SceneType.Question as SceneType,
		}));
	};

	// --- Return ---
	return (
		<div className="story-player">
			{isLoading && <p>Loading...</p>}
			{error && <p className="error">Error: {error}</p>}

			{isGameOver && currentSceneData && (
				<div className="game-over">
					{/* Bruk scenetittel hvis tilgjengelig, ellers en generell melding */}
					{/*<h2>{currentSceneData.title || "The Story Ends!"}</h2>*/}
					<h2>{"The Story Ends!"}</h2>

					<p className="ending-text">{currentSceneData.sceneText}</p>

					<p>
						Your final score: {session.score} of{" "}
						{currentSceneData.maxScore || "N/A"}
					</p>

					{/* Legg til en Start New Game-knapp */}
					<button onClick={() => window.location.reload()}>
						Start New Game
					</button>
				</div>
			)}

			{/* Vis selve spillet HVIS IKKE Game Over */}
			{!isLoading && !error && !isGameOver && currentSceneData && (
				<div className="scene-container">
					{/* Viser den midlertidige feedbackteksten etter et svar */}
					{feedbackText && (
						<p className="feedback-text">{feedbackText}</p>
					)}

					<p>
						Level: {session.level} | Score: {session.score} /{" "}
						{currentSceneData.maxScore}
					</p>

					{currentSceneData.sceneType === SceneType.Intro && (
						<div>
							<h3>Introduction</h3>
							<p>{currentSceneData.sceneText}</p>

							<button
								onClick={startFirstQuestion}
								disabled={
									isLoading ||
									!currentSceneData.nextSceneAfterIntroId
								}
							>
								Start Spillet
							</button>
						</div>
					)}

					{currentSceneData.sceneType === SceneType.Question && (
						<div>
							<h3>{currentSceneData.question}</h3>
							<p>{currentSceneData.sceneText}</p>

							<div className="answer-options">
								{currentSceneData.answerOptions?.map(
									(option) => (
										<button
											key={option.answerOptionId}
											onClick={() => handleAnswer(option)}
											disabled={!!feedbackText}
										>
											{option.answer}
										</button>
									)
								)}
							</div>
						</div>
					)}

					{/* 2. FJERNES: Denne sjekken er nå redundant og skaper konflikt.
                {currentSceneData.sceneType === "Ending" && (
                    <div>
                        <h3>The game is finished!</h3>
                        <p>{currentSceneData.sceneText}</p>
                        ...
                    </div>
                )}
                */}
				</div>
			)}
		</div>
	);
};
