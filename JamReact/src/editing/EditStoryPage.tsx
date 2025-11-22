import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getStoryMetadata, updateStoryMetadata } from "./storyEditingService";

const EditStory = () => {
  const { storyId } = useParams();
  const navigate = useNavigate();

  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [difficulty, setDifficulty] = useState(1);
  const [accessibility, setAccessibility] = useState(0);

  useEffect(() => {
    const load = async () => {
      const res = await getStoryMetadata(Number(storyId));
      if (!res.ok) return;
      const data = await res.json();

      setTitle(data.title);
      setDescription(data.description);
      setDifficulty(data.difficultyLevel);
      setAccessibility(data.accessibility);
    };
    load();
  }, [storyId]);

  const handleSave = async () => {
    const res = await updateStoryMetadata(Number(storyId), {
      storyId,
      title,
      description,
      difficultyLevel: difficulty,
      accessibility,
    });

    if (res.ok) navigate(`/edit/${storyId}/intro`);
  };

  return (
    <div className="pixel-bg">
      <h1>Edit Story</h1>

      <label>Title</label>
      <input value={title} onChange={e => setTitle(e.target.value)} />

      <label>Description</label>
      <textarea value={description} onChange={e => setDescription(e.target.value)} />

      <label>Difficulty</label>
      <select value={difficulty} onChange={e => setDifficulty(Number(e.target.value))}>
        <option value={1}>Easy</option>
        <option value={2}>Medium</option>
        <option value={3}>Hard</option>
      </select>

      <label>Accessibility</label>
      <select value={accessibility} onChange={e => setAccessibility(Number(e.target.value))}>
        <option value={0}>Public</option>
        <option value={1}>Private</option>
      </select>

      <button className="pixel-btn teal" onClick={handleSave}>Next</button>
    </div>
  );
};

export default EditStory;
