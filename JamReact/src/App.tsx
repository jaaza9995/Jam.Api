import { BrowserRouter as Router, Routes, Route } from "react-router-dom";
import HomePage from "./home/HomePage";
import CreateIntro from "./create/CreateIntro";
import CreateQuestions from "./create/CreateQuestions";
import CreateEndings from "./create/CreateEndings";
import { StoryCreationProvider } from "./storyCreation/StoryCreationContext";
import { StoryPlayer } from "./StoryPlaying/StoryPlayer";

function App() {
  return (
    <StoryCreationProvider>
      <Router>
        <Routes>
          <Route path="/" element={<HomePage />} />
          <Route path="/create/intro" element={<CreateIntro />} />
          <Route path="/create/questions" element={<CreateQuestions />} />
          <Route path="/create/endings" element={<CreateEndings />} />

          <Route path="/play/:storyId" element={<StoryPlayerWrapper />} />
        </Routes>
      </Router>
    </StoryCreationProvider>
  );
}

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
