import React, { useEffect, useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { getEndings, updateEndings } from "./storyEditingService";

const EditEndings = () => {
  const { storyId } = useParams();
  const navigate = useNavigate();

  const [good, setGood] = useState("");
  const [neutral, setNeutral] = useState("");
  const [bad, setBad] = useState("");

  useEffect(() => {
    const load = async () => {
      const res = await getEndings(Number(storyId));
      if (!res.ok) return;

      const data = await res.json();
      setGood(data.goodEnding);
      setNeutral(data.neutralEnding);
      setBad(data.badEnding);
    };
    load();
  }, [storyId]);

  const handleSave = async () => {
    const res = await updateEndings(Number(storyId), {
      goodEnding: good,
      neutralEnding: neutral,
      badEnding: bad,
    });

    if (res.ok) navigate("/");
  };

  return (
    <div className="pixel-bg">
      <h1>Edit Endings</h1>

      <textarea value={good} onChange={e => setGood(e.target.value)} />
      <textarea value={neutral} onChange={e => setNeutral(e.target.value)} />
      <textarea value={bad} onChange={e => setBad(e.target.value)} />

      <button className="pixel-btn teal" onClick={handleSave}>Save</button>
    </div>
  );
};

export default EditEndings;
