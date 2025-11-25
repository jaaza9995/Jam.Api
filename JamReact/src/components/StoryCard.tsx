import React from "react";
import { Story as StoryCardType } from "../types/createStory";
import { useNavigate } from "react-router-dom";

interface Props {
  story: StoryCardType;
  showEditButton?: boolean; // valgfri EDIT-knapp
}

const StoryCard: React.FC<Props> = ({ story, showEditButton = false }) => {
  const navigate = useNavigate();

  return (
    <div className="story-card">
      {/* Story content */}
      <div className="story-content">
        <h3>{story.title}</h3>
        <p>{story.description}</p>
      </div>

      {/* Bottom info: question count, difficulty, private code */}
      <div className="bottom-info">
        <div className="diff-qc-row">
          <span className="question-count">
            <p>Questions: {story.questionCount}</p>
          </span>

          <span className="difficulty">
            {story.difficultyLevel === 0 && (
              <button className="difficulty easy">Easy</button>
            )}
            {story.difficultyLevel === 1 && (
              <button className="difficulty medium">Medium</button>
            )}
            {story.difficultyLevel === 2 && (
              <button className="difficulty hard">Hard</button>
            )}
          </span>
        </div>

        {story.accessibility === 1 && (
          <p className="private-code">Game Code: {story.code}</p>
        )}
      </div>

      {/* Buttons */}
      <div className="story-buttons">
        {showEditButton && (
          <button
            className="pixel-btn edit"
            onClick={() => navigate(`/edit/${story.storyId}`)}
          >
            EDIT
          </button>
        )}

        <button
          className="pixel-btn play"
          onClick={() => navigate(`/play/${story.storyId}`)}
        >
          PLAY
        </button>
      </div>
    </div>
  );
};

export default StoryCard;
