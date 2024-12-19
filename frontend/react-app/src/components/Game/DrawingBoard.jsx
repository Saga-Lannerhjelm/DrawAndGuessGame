import React, { useEffect, useRef, useState } from "react";
import { useConnection } from "../../context/ConnectionContext";
import { useParams } from "react-router-dom";
import DrawingInfo from "./DrawingInfo";

const DrawingBoard = ({ gameRoom, setGameRoom, isDrawing }) => {
  const [color, setColor] = useState("#ffffff");
  const [gameActive, setGameActive] = useState(false);
  const [loading, setLoading] = useState(true);
  const canvasRef = useRef(null);

  const { connection } = useConnection();
  const params = useParams();

  useEffect(() => {
    if (connection) {
      setGameActive(true);
      setGameRoom(params.room);
      connection.on("Drawing", (start, end, color) => {
        drawStroke(start, end, color);
      });
      connection.on("clearCanvas", () => {
        clearCanvas();
      });
    }
  }, [connection]);

  async function getColor() {
    try {
      // Link to API https://github.com/cheatsnake/xColors-api
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

      const handelResize = () => {
        canvas.width = document.getElementById("canvas-container").offsetWidth;
        canvas.height =
          document.getElementById("canvas-container").offsetHeight;
      };

      handelResize();
      window.addEventListener("resize", handelResize);

      let penDown = false;
      let prevPoint = { x: 0, y: 0 };

      if (isDrawing) {
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
            drawStroke(start, end, "red");
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
    }
  }, [connection, gameActive, loading, isDrawing]);

  function drawStroke(start, end, color) {
    if (!isDrawing) {
      color = "black";
    }
    const canvas = canvasRef.current;
    const ctx = canvas.getContext("2d");
    ctx.strokeStyle = color;
    ctx.lineWidth = 4;
    ctx.lineCap = "round";
    ctx.lineJoin = "round";
    ctx.beginPath();
    ctx.moveTo(start.x, start.y);
    ctx.lineTo(end.x, end.y);
    ctx.stroke();
  }

  const sendClearCanvas = async () => {
    if (connection) {
      connection.invoke("SendClearCanvas", gameRoom);
    }
  };

  function clearCanvas() {
    const canvas = canvasRef.current;
    const ctx = canvas.getContext("2d");
    ctx.clearRect(0, 0, canvas.width, canvas.height);
  }
  return (
    <>
      <div>
        {gameActive && <canvas ref={canvasRef} className="canvas"></canvas>}
        {isDrawing && <DrawingInfo clearCanvas={sendClearCanvas} />}
      </div>
    </>
  );
};

export default DrawingBoard;
