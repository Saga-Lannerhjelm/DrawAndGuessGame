import React, { useEffect, useRef, useState } from "react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";

const Game = () => {
  const [color, setColor] = useState("#ffffff");
  const [connection, setConnection] = useState(undefined);
  const canvasRef = useRef(null);

  async function startConnection() {
    if (!connection) {
      const connection = new HubConnectionBuilder()
        .withUrl("http://localhost:5034/draw")
        .configureLogging(LogLevel.Information)
        .withAutomaticReconnect()
        .build();

      connection.on("Drawing", (start, end, color) => {
        drawStroke(start, end, color);
      });

      try {
        await connection.start();
        setConnection(connection);
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
    }
  }

  useEffect(() => {
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

      if (penDown && connection) {
        drawStroke(start, end, "#ffffff");
        connection.invoke("Drawing", start, end, color);
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
  }, [connection]);

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
        <button onClick={startConnection}>Starta</button>
        <canvas ref={canvasRef} style={{ border: "1px solid white" }}></canvas>
      </div>
    </>
  );
};

export default Game;
