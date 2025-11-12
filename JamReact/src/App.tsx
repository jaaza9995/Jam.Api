import { BrowserRouter, Routes, Route } from "react-router-dom";
import LoginPage from "./admin/LoginPage";
import HomePage from "./home/HomePage";
import React from "react";

const App: React.FC = () => {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<LoginPage />} />
        <Route path="/home" element={<HomePage />} />
      </Routes>
    </BrowserRouter>
  );
}

export default App;
