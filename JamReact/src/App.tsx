import { BrowserRouter as Router, Routes, Route, Navigate } from "react-router-dom";
import { Container } from 'react-bootstrap'

import HomePage from "./home/HomePage";
import CreateIntro from "./create/CreateIntro";
import CreateQuestions from "./create/CreateQuestions";
import CreateEndings from "./create/CreateEndings";

import { StoryCreationProvider } from "./storyCreation/StoryCreationContext";

import NavMenu from './shared/NavMenu'
import LoginPage from './auth/LoginPage'
import RegisterPage from './auth/RegisterPage'
import ProtectedRoute from './auth/ProtectedRoute'
import { AuthProvider } from './auth/AuthContext'

import { StoryPlayer } from "./StoryPlaying/StoryPlayer";

const App: React.FC = () => {
  return (
    <AuthProvider>
      <Router>
        <NavMenu />
        <Container className="mt-4">

          <Routes>
            {/* Public */}
            <Route path="/login" element={<LoginPage />} />
            <Route path="/register" element={<RegisterPage />} />

            {/* Protected */}
            <Route element={<ProtectedRoute />}>
              <Route path="/" element={<HomePage />} />

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
            </Route>

            <Route path="/play/:storyId" element={<StoryPlayerWrapper />} />

            {/* Catch-all */}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>

        </Container>
      </Router>
    </AuthProvider>
  );
};


// 💡 VIKTIG: Hjelpekomponent for å hente storyId fra URL
// Dette er nødvendig for å fange opp ID-en fra URL-en og sende den som en number prop.
import { useParams } from 'react-router-dom';

const StoryPlayerWrapper = () => {
    // useParams henter verdien etter :storyId i URL-en
    const { storyId } = useParams<{ storyId: string }>(); 
    
    // Konverterer string (fra URL) til number, da StoryPlayer forventer number.
    const storyIdNumber = parseInt(storyId || '0', 10);
    
    // Render StoryPlayer kun hvis vi har en gyldig ID
    if (!storyIdNumber) {
        return <div>Feil: Historie ID mangler eller er ugyldig.</div>;
    }

    return <StoryPlayer storyId={storyIdNumber} />;
};

export default App;
