import React, { useEffect, useRef, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

const Game = () => {
  const [color, setColor] = useState("#ffffff");
  const [connection, setConnection] = useState(undefined);
  const [gameRoom, setGameRoom] = useState("");
  const [gameActive, setGameActive] = useState(false);
  const [loading, setLoading] = useState(true);
  const canvasRef = useRef(null);

  async function startConnection(role, gameRoom) {
    setGameRoom(gameRoom);
    if (!connection) {
      const connection = new HubConnectionBuilder()
        .withUrl("http://localhost:5034/draw")
        .configureLogging(LogLevel.Information)
        .withAutomaticReconnect()
        .build();

      connection.on("Drawing", (start, end, color) => {
        drawStroke(start, end, color);
      });

      connection.on("JoinedGame", (msg, role) => {
        console.log(msg);
      });

      try {
        await connection.start();
        connection.invoke("JoinGame", role, gameRoom);
        setConnection(connection);
        setGameActive(true);
      } catch (error) {
        console.error();
      }
    }
  }

  async function getColor() {
    try {
      const response = await fetch(
        "https://x-colors.yurace.pro/api/random/?type=dark"
      );

      if (!response.ok) throw new Error(`Response status: ${response.status}`);

      return (await response.json()).hex;
    } catch (error) {
      console.error(error);
      return null;
    } finally {
      setLoading(false);
    }
  }

  useEffect(() => {
    if (gameActive) {
      getColor().then((color) => setColor(color));
      const canvas = canvasRef.current;
      canvas.width = window.innerWidth;
      canvas.height = window.innerHeight - 200;

      let penDown = false;
      let prevPoint = { x: 0, y: 0 };

      function draw(x, y) {
        const start = {
          x: prevPoint.x - canvas.offsetLeft,
          y: prevPoint.y - canvas.offsetTop,
        };

        const end = {
          x: x - canvas.offsetLeft,
          y: y - canvas.offsetTop,
        };

        if (penDown && connection && !loading) {
          drawStroke(start, end, "#ffffff");
          console.log(color);
          connection.invoke("Drawing", start, end, color, gameRoom);
        }
        prevPoint = { x: x, y: y };
      }

      canvas.addEventListener("mousedown", () => (penDown = true));
      canvas.addEventListener("mousemove", (e) => draw(e.pageX, e.pageY));
      canvas.addEventListener("mouseup", () => (penDown = false));

      return () => {
        canvas.addEventListener("mousedown", () => (penDown = true));
        canvas.addEventListener("mousemove", (e) => draw(e.pageX, e.pageY));
        canvas.addEventListener("mouseup", () => (penDown = false));
      };
    }
  }, [connection, gameActive, loading]);

  function drawStroke(start, end, color) {
    const canvas = canvasRef.current;
    const ctx = canvas.getContext("2d");
    ctx.strokeStyle = color;
    ctx.lineWidth = 5;
    ctx.lineCap = "round";
    ctx.lineJoin = "round";
    ctx.beginPath();
    ctx.moveTo(start.x, start.y);
    ctx.lineTo(end.x, end.y);
    ctx.stroke();
  }
  return (
    <>
      <div>
        <button onClick={() => startConnection("artist", "123")}>
          Starta som ritare
        </button>
        <button onClick={() => startConnection("guesser", "123")}>
          Starta som gissare
        </button>
        {gameActive && (
          <canvas
            ref={canvasRef}
            style={{ border: "1px solid white" }}
          ></canvas>
        )}
      </div>
    </>
  );
};

export default Game;
