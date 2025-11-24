import { BrowserRouter as Router, Routes, Route, Navigate, useParams, Outlet } from "react-router-dom";
import { Container } from "react-bootstrap";

import HomePage from "./home/HomePage";
import BrowsePage from "./browse/BrowsePage";

// CREATE
import CreateIntro from "./create/CreateIntro";
import CreateQuestions from "./create/CreateQuestions";
import CreateEndings from "./create/CreateEndings";
import { StoryCreationProvider } from "./storyCreation/StoryCreationContext";

// EDIT
import EditStoryPage from "./editing/EditStoryPage";
import EditIntroPage from "./editing/EditIntroPage";
import EditQuestionsPage from "./editing/EditQuestionsPage";
import EditEndingsPage from "./editing/EditEndingsPage";

import NavMenu from "./shared/NavMenu";
import LoginPage from "./auth/LoginPage";
import RegisterPage from "./auth/RegisterPage";
import ProtectedRoute from "./auth/ProtectedRoute";
import { AuthProvider } from "./auth/AuthContext";

// PLAY
import { StoryPlayer } from "./StoryPlaying/StoryPlayer";

// --- Admin ---
import { useAuth } from "./auth/AuthContext";
import AdminUsersPage from "./admin/AdminUsersPage";
import AdminStoriesPage from "./admin/AdminStoriesPage";

const App: React.FC = () => {
	return (
		<AuthProvider>
			<Router>
				<NavMenu />
				<Container className="mt-4">
					<Routes>
						{/* ---------------- PUBLIC ---------------- */}
						<Route path="/login" element={<LoginPage />} />
						<Route path="/register" element={<RegisterPage />} />

						{/* ---------------- PROTECTED ---------------- */}
						<Route element={<ProtectedRoute />}>
							{/* HOME */}
							<Route
								path="/"
								element={
									!localStorage.getItem("token") ? (
										<Navigate to="/login" replace />
									) : (
										<HomePage />
									)
								}
							/>

							{/* CREATE FLOW */}
							<Route
								path="/create/*"
								element={
									<StoryCreationProvider>
										<Routes>
											<Route
												path="intro"
												element={<CreateIntro />}
											/>
											<Route
												path="questions"
												element={<CreateQuestions />}
											/>
											<Route
												path="endings"
												element={<CreateEndings />}
											/>
										</Routes>
									</StoryCreationProvider>
								}
							/>
                            {/* EDIT FLOW */}
							<Route
								path="/edit/:storyId"
								element={<EditStoryPage />}
							/>
							<Route
								path="/edit/:storyId/intro"
								element={<EditIntroPage />}
							/>
							<Route
								path="/edit/:storyId/questions"
								element={<EditQuestionsPage />}
							/>
							<Route
								path="/edit/:storyId/endings"
								element={<EditEndingsPage />}
							/>

							{/* PLAYING FLOW */}
							<Route
								path="/play/:storyId"
								element={<StoryPlayer />}
							/>

                            {/* BROWSE */}
                            <Route 
                                path="/browse" 
                                element={<BrowsePage />} 
                            />

							{/* ADMIN ROUTES */}
							<Route
								path="/admin"
								element={<AdminRouteWrapper />}
							>
								{/* Under-rutene blir nå barn av AdminRouteWrapper og rendres via Outlet */}
								<Route
									path="users"
									element={<AdminUsersPage />}
								/>
								<Route
									path="stories"
									element={<AdminStoriesPage />}
								/>
							</Route>
                        </Route>

						{/* ---------------- CATCH ALL ---------------- */}
						<Route path="*" element={<Navigate to="/" replace />} />
					</Routes>
				</Container>
			</Router>
		</AuthProvider>
	);
};

// 💡 VIKTIG: Hjelpekomponent for å sikre Admin-tilgang.
const AdminRouteWrapper: React.FC = () => {
	const { isAdmin, isLoading } = useAuth();

	if (isLoading) {
		// Viser ingenting eller en spinner mens autentisering pågår
		return <div>Laster brukerdata...</div>;
	}

	if (!isAdmin) {
		// Hvis ikke Admin, sendes brukeren til forsiden eller login
		return <Navigate to="/" replace />;
	}

	// Returner Outlet for å rendre de nestede rutene (/admin/users, /admin/stories)
	return <Outlet />;
};
export default App;
