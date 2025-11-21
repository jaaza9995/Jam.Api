import React, { useEffect, useState } from "react";
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

	// 2. Data for den aktuelle scenen (innholdet som skal vises)
	const [currentSceneData, setCurrentSceneData] = useState<IPlayScene | null>(
		null
	);

	// 3. Tilstand for lasting og feil
	const [isLoading, setIsLoading] = useState<boolean>(false);
	const [error, setError] = useState<string | null>(null);

	// 4. Midlertidig tilstand for å vise feedbackteksten etter et svar
	const [feedbackText, setFeedbackText] = useState<string | null>(null);

	// 5. Tilstand for å markere at spillet er ferdig
	const [isGameOver, setIsGameOver] = useState<boolean>(false);

	// --- useEffect 1: Starter spillet og henter første scene ---
	useEffect(() => {
		// Hvis vi allerede har en session ID, betyr det at spillet er i gang.
		if (session.sessionId) {
			fetchScene(session.currentSceneId, session.currentSceneType);
			return;
		}

		// Hvis vi ikke har en session ID, må vi starte en ny
		const startSession = async () => {
			setIsLoading(true);
			setError(null);

			try {
				const response = await fetch(
					`${API_URL}/api/StoryPlaying/start/${storyId}`,
					{
						method: "POST",
						headers: getAuthHeaders()
					}
				);

				if (!response.ok) {
					const errorBody: IErrorDto = await response.json();
					throw new Error(
						errorBody.errorTitle || "Kunne ikke starte spilløkt."
					);
				}

				const data: IStartSessionResponse = await response.json();

				// Oppdater session state
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
		};

		// Kall kun startSession én gang ved initial lasting av komponenten
		startSession();
	}, [storyId, session.sessionId]); // Trigger når storyId endres eller session.sessionId settes for første gang

	// Funksjon for å hente scenedata (kalles internt i useEffect)
	const fetchScene = async (
		sceneId: number | null,
		sceneType: SceneType | null
	) => {
		if (!sceneId || !sceneType || !session.sessionId) return;

		setIsLoading(true);
		setError(null);
		setFeedbackText(null); // Skjul feedback når vi laster ny scene

		try {
			// API-kall: /api/StoryPlaying/scene?sceneId=X&sceneType=Y&sessionId=Z
			const response = await fetch(
				`${API_URL}/api/StoryPlaying/scene?sceneId=${sceneId}&sceneType=${sceneType}&sessionId=${session.sessionId}`,
				{
					headers: { "Content-Type": "application/json" },
				}
			);

			if (!response.ok) {
				throw new Error("Error loading scene");
			}

			const data: IPlayScene = await response.json();

			// Oppdater tilstanden med scenedata
			setCurrentSceneData(data);

			// Sjekk om dette er en EndingScene
			if (data.sceneType === "Ending") {
				setIsGameOver(true);
			}
		} catch (e: any) {
			setError(e.message);
		} finally {
			setIsLoading(false);
		}
	};

	// Funksjon for å sende valgt svar til API
	const handleAnswer = async (selectedAnswer: AnswerOption) => {
		// Sjekk at vi har en gyldig session
		if (!session.sessionId) {
			setError("The game session has not started.");
			return;
		}

		setIsLoading(true);
		setError(null);

		try {
			// API-kall: POST /api/StoryPlaying/answer
			const response = await fetch(`${API_URL}/api/StoryPlaying/answer`, {
				method: "POST",
				headers: getAuthHeaders(),
				body: JSON.stringify({
					// Hva APIet forventer i AnswerFeedbackDto
					sessionId: session.sessionId,
					selectedAnswerId: selectedAnswer.answerOptionId,
				}),
			});

			if (!response.ok) {
				// Håndter Game Over på Level 1, som returnerer en spesiell melding
				if (response.status === 200) {
					// Sjekk om Game Over responsen ble sendt som Ok(200)
					const gameOverResponse = await response.json();
					if (gameOverResponse.message === "Game over") {
						setIsGameOver(true);
						// Oppdater sluttscore, selv om spillet er over
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

			// Først: Vis feedbackteksten for AnswerOption
			setFeedbackText(feedback.sceneText);

			// Oppdater score og level
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
				currentSceneType: "Ending" as SceneType,
			}));

			// useEffect vil trigge fetchScene(EndingSceneId, 'Ending')
		} catch (e: any) {
			setError(e.message);
			setIsLoading(false);
		}
	};

	const startFirstQuestion = () => {
		// Sjekker om vi har data og er i IntroScene
		if (!currentSceneData || currentSceneData.sceneType !== "Intro") return;

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
			currentSceneType: "Question" as SceneType,
		}));
	};

	// --- Return ---
	return (
		<div className="story-player">
			{isLoading && <p>Loading...</p>}
			{error && <p className="error">Error: {error}</p>}

			{isGameOver && currentSceneData && (
				<div className="game-over">
					<h2>The Story is finished!</h2>

					{/* Viser EndingScene teksten, som ble hentet via fetchScene */}
					<p className="ending-text">{currentSceneData.sceneText}</p>

					{/* Statistikk (bruker session.score da currentSceneData kanskje ikke oppdateres) */}
					<p>
						Your final score: {session.score} of{" "}
						{currentSceneData.maxScore}
					</p>
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

					{currentSceneData.sceneType === "Intro" && (
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

					{currentSceneData.sceneType === "Question" && (
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
