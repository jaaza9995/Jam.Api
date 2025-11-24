import { BrowserRouter as Router, Routes, Route, Navigate, useParams } from "react-router-dom";
import { Container } from "react-bootstrap";
import { StoryPlayer } from "./StoryPlaying/StoryPlayer";

import HomePage from "./home/HomePage";
import BrowsePage from "./browse/BrowsePage";

// CREATE
import CreateIntro from "./create/CreateIntro";
import CreateQuestions from "./create/CreateQuestions";
import CreateEndings from "./create/CreateEndings";
import { StoryCreationProvider } from "./create/StoryCreationContext";

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


import './App.css';

// Wrapper that converts :storyId from the URL to a number for StoryPlayer
const StoryPlayerWrapper: React.FC = () => {
  const { storyId } = useParams<{ storyId: string }>();
  const storyIdNumber = parseInt(storyId || "0", 10);

  if (!storyIdNumber) {
    return <div>Feil: Historie ID mangler eller er ugyldig.</div>;
  }

  return <StoryPlayer storyId={storyIdNumber} />;
};

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
                  !localStorage.getItem("token")
                    ? <Navigate to="/login" replace />
                    : <HomePage />
                }
              />

              {/* CREATE FLOW */}
              <Route
                path="/create/*"
                element={
                  <StoryCreationProvider>
                    <Routes>
                      <Route path="intro" element={<CreateIntro />} />
                      <Route path="questions" element={<CreateQuestions />} />
                      <Route path="endings" element={<CreateEndings />} />
                    </Routes>
                  </StoryCreationProvider>
                }
              />

              {/* EDIT FLOW */}
              <Route path="/edit/:storyId" element={<EditStoryPage />} />
              <Route path="/edit/:storyId/intro" element={<EditIntroPage />} />
              <Route path="/edit/:storyId/questions" element={<EditQuestionsPage />} />
              <Route path="/edit/:storyId/endings" element={<EditEndingsPage />} />

              {/* BROWSE */}
              <Route path="/browse" element={<BrowsePage />} />

              {/* PLAY */}
              <Route path="/play/:storyId" element={<StoryPlayerWrapper />} />

            </Route>

            {/* ---------------- CATCH ALL ---------------- */}
            <Route path="*" element={<Navigate to="/" replace />} />

          </Routes>
        </Container>
      </Router>
    </AuthProvider>
  );
};

export default App;
