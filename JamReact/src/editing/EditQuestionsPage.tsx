import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getQuestions, updateQuestions, deleteQuestion } from "./storyEditingService";

const EditQuestions = () => {
  const { storyId } = useParams();
  const navigate = useNavigate();

  const [questions, setQuestions] = useState<any[]>([]);

  useEffect(() => {
    const load = async () => {
      const res = await getQuestions(Number(storyId));
      if (!res.ok) return;
      const data = await res.json();
      setQuestions(data);
    };
    load();
  }, [storyId]);

  const handleSave = async () => {
    const res = await updateQuestions(Number(storyId), questions);
    if (res.ok) navigate(`/edit/${storyId}/endings`);
  };

  const handleDelete = async (id: number, index: number) => {
    if (id !== 0) await deleteQuestion(id);
    setQuestions(q => q.filter((_, i) => i !== index));
  };

  return (
    <div className="pixel-bg">
      <h1>Edit Questions</h1>

      {questions.map((q, i) => (
        <div key={i} className="question-editor">
          <textarea
            value={q.storyText}
            onChange={e => {
              const updated = [...questions];
              updated[i].storyText = e.target.value;
              setQuestions(updated);
            }}
          />

          <textarea
            value={q.questionText}
            onChange={e => {
              const updated = [...questions];
              updated[i].questionText = e.target.value;
              setQuestions(updated);
            }}
          />

          <button className="pixel-btn pink" onClick={() => handleDelete(q.questionSceneId, i)}>
            Delete
          </button>
        </div>
      ))}

      <button className="pixel-btn teal" onClick={handleSave}>Next</button>
    </div>
  );
};

export default EditQuestions;
