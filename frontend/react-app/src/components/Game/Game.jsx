import React, { useEffect, useRef, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useConnection } from "../../context/ConnectionContext";
import { useNavigate, useParams } from "react-router-dom";
import DrawingBoard from "./DrawingBoard";
import GuessContainer from "./GuessContainer";
import Users from "./Users";
import Header from "../Header";
import GuessForm from "./GuessForm";

const Game = () => {
  const { connection } = useConnection();
  const [gameRoom, setGameRoom] = useState("");
  const navigate = useNavigate();

  const leaveRoom = async () => {
    console.log("leave");
    await connection.stop();
    navigate("/home");
  };

  return (
    <>
      <Header gameRoom={gameRoom} onclick={leaveRoom} />
      <div className="game-container">
        <div>
          <div id="canvas-container">
            <DrawingBoard gameRoom={gameRoom} setGameRoom={setGameRoom} />
          </div>
          <GuessForm />
        </div>
        <GuessContainer gameRoom={gameRoom} />
      </div>
      <button onClick={() => connection.invoke("StartRound", gameRoom)}>
        Välj ritare
      </button>
    </>
  );
};

export default Game;
