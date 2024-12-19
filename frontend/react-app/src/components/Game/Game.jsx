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
  const [gameActive, setGameActive] = useState(false);

  const navigate = useNavigate();
  const params = useParams();

  useEffect(() => {
    if (connection === undefined) {
      navigate("/home");
    }
    if (connection) {
      setGameRoom(params.room);
      connection.on("GameCanStart", (canStart) => {
        setGameActive(canStart);
      });
    }
  }, [connection]);

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
      <div className="game-container">
        <div>
          {gameActive ? (
            <>
              <TopSection />
              <div id="canvas-container">
                <DrawingBoard
                  gameRoom={gameRoom}
                  gameActive={gameActive}
                  isDrawing={isDrawing}
                />
              </div>
              {!isDrawing && <GuessForm />}
            </>
          ) : (
            <button onClick={startRound} className="btn">
              Starta spelet
            </button>
          )}
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
