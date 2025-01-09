import { useState } from "react";
import reactLogo from "./assets/react.svg";
import viteLogo from "/vite.svg";
import "./App.css";
import { Route, Routes } from "react-router-dom";
import Home from "./components/MainPage";
import Login from "./components/Login";
import Game from "./components/Game/Game";
import { ConnectionProvider } from "./context/ConnectionContext";
import Highscore from "./components/highscore/highscore";
import NavBar from "./components/Navbar";

function App() {
  return (
    <>
      <NavBar />
      <ConnectionProvider>
        <Routes>
          <Route path="/login" element={<Login />} />
          <Route path="/" element={<Home />} />
          <Route path="/highscore" element={<Highscore />} />
          <Route path="/game/:room" element={<Game />} />
        </Routes>
      </ConnectionProvider>
    </>
  );
}

export default App;
