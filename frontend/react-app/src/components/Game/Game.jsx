import React, { useEffect, useRef, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useConnection } from "../../context/ConnectionContext";
import { useParams } from "react-router-dom";
import DrawingBoard from "./DrawingBoard";
import GuessContainer from "./GuessContainer";
import Users from "./Users";
import Header from "../Header";
import GuessForm from "./GuessForm";

const Game = () => {
  const { connection } = useConnection();
  const [gameRoom, setGameRoom] = useState("");
  return (
    <>
      <Header gameRoom={gameRoom} />
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
