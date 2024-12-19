import React, { useEffect, useRef, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useConnection } from "../../context/ConnectionContext";
import { useNavigate, useParams } from "react-router-dom";
import DrawingBoard from "./DrawingBoard";
import GuessContainer from "./GuessContainer";
import Users from "./Users";
import Header from "../Header";
import GuessForm from "./GuessForm";
import DrawingInfo from "./DrawingInfo";
import TopSection from "./TopSection";

const Game = () => {
  const { connection } = useConnection();
  const [gameRoom, setGameRoom] = useState("");
  const [isDrawing, setIsDrawing] = useState(false);
  const navigate = useNavigate();

  const leaveRoom = async () => {
    console.log("leave");
    await connection.stop();
    navigate("/home");
  };

  const startRound = async () => {
    if (connection) {
      await connection.invoke("StartRound", gameRoom);
    }
  };

  return (
    <>
      <Header gameRoom={gameRoom} onclick={leaveRoom} />
      <button onClick={startRound}>Välj ritare</button>
      <div className="game-container">
        <div>
          <TopSection />
          <div id="canvas-container">
            <DrawingBoard
              gameRoom={gameRoom}
              setGameRoom={setGameRoom}
              isDrawing={isDrawing}
            />
          </div>
          {!isDrawing ? <GuessForm /> : <DrawingInfo />}
        </div>
        <GuessContainer
          gameRoom={gameRoom}
          userIsDrawing={(bool) => setIsDrawing(bool)}
        />
      </div>
    </>
  );
};

export default Game;
