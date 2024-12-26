import { useState } from "react";
import reactLogo from "./assets/react.svg";
import viteLogo from "/vite.svg";
import "./App.css";
import { Route, Routes } from "react-router-dom";
import Home from "./components/MainPage";
import Login from "./components/Login";
import Game from "./components/Game/Game";
import { ConnectionProvider } from "./context/ConnectionContext";

function App() {
  return (
    <ConnectionProvider>
      <Routes>
        <Route path="/" element={<Login />} />
        <Route path="/home" element={<Home />} />
        <Route path="/game/:room" element={<Game />} />
      </Routes>
    </ConnectionProvider>
  );
}

export default App;
