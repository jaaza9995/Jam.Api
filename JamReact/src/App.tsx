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

import './App.css';

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

            {/* Catch-all */}
            <Route path="*" element={<Navigate to="/" replace />} />
          </Routes>

        </Container>
      </Router>
    </AuthProvider>
  );
};

export default App;
