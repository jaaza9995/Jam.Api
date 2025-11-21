import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import HomePage from "./home/HomePage";
import CreateIntro from "./create/CreateIntro";
import CreateQuestions from "./create/CreateQuestions";
import CreateEndings from "./create/CreateEndings";
import { StoryCreationProvider } from "./storyCreation/StoryCreationContext";

function App() {
  return (
    <StoryCreationProvider>
      <Router>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/create/intro" element={<CreateIntro />} />
          <Route path="/create/questions" element={<CreateQuestions />} />
          <Route path="/create/endings" element={<CreateEndings />} />
        </Routes>
      </Router>
    </StoryCreationProvider>
  );
}

export default App;
