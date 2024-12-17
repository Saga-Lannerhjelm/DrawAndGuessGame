import React, { useEffect, useRef, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useConnection } from "../../context/ConnectionContext";
import { useParams } from "react-router-dom";
import DrawingBoard from "./DrawingBoard";

const Game = () => {
  return (
    <>
      <DrawingBoard />
    </>
  );
};

export default Game;
