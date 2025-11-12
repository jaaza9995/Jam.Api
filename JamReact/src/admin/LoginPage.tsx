import React, { useState } from "react";
import { useNavigate } from "react-router-dom";
import "./LoginPage.css";

const API_URL = import.meta.env.VITE_API_URL;

const LoginPage = () => {
  const navigate = useNavigate();
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    try {
      const response = await fetch("http://localhost:5000/api/auth/login", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ username, password }),
      });

      if (response.ok) {
        const data = await response.json();
        localStorage.setItem("token", data.Token);
        localStorage.setItem("username", data.username);
        navigate("/home"); // <- riktig her
      } else {
        alert("Invalid username or password");
      }
    } catch (error) {
      console.error("Login failed:", error);
    }
  };

  return (
    <div className="pixel-bg">
      <div className="login-container text-center">
        <h1 className="game-title">MATH QUEST</h1>

        <form className="login-form" onSubmit={handleSubmit}>
          <input
            type="text"
            placeholder="USERNAME"
            value={username}
            onChange={(e) => setUsername(e.target.value)}
            className="pixel-input"
            required
          />
          <input
            type="password"
            placeholder="PASSWORD"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            className="pixel-input"
            required
          />

          <div className="button-row">
            <button type="submit" className="pixel-btn brown">
              LOGIN
            </button>
            <button type="button" className="pixel-btn yellow">
              SIGN UP
            </button>
          </div>
        </form>
      </div>
    </div>
  );
};

export default LoginPage;
