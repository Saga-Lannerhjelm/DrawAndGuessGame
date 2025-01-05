import React, { useEffect, useRef, useState } from "react";
import { useConnection } from "../../context/ConnectionContext";
import { useParams } from "react-router-dom";
import DrawingInfo from "./DrawingInfo";

const DrawingBoard = ({ gameRoom, isDrawing, gameActive, word }) => {
  const [color, setColor] = useState("#ffffff");

  const [loading, setLoading] = useState(true);
  const canvasRef = useRef(null);

  const { connection } = useConnection();

  useEffect(() => {
    if (connection) {
      connection.on("Drawing", (start, end, color) => {
        drawStroke(start, end, color);
      });
      connection.on("clearCanvas", () => {
        clearCanvas();
      });
    }
    clearCanvas();
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
      getColor()
        .then((color) => setColor(color))
        .catch(() => setColor("green"));
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

        const mouseDownHandler = () => (penDown = true);
        const mouseMoveHandler = (e) => draw(e.pageX, e.pageY);
        const mouseUpHandler = () => (penDown = false);

        canvas.addEventListener("mousedown", mouseDownHandler);
        canvas.addEventListener("mousemove", mouseMoveHandler);
        canvas.addEventListener("mouseup", mouseUpHandler);

        return () => {
          canvas.removeEventListener("mousedown", mouseDownHandler);
          canvas.removeEventListener("mousemove", mouseMoveHandler);
          canvas.removeEventListener("mouseup", mouseUpHandler);
        };
      }
    }
  }, [connection, gameActive, loading, isDrawing]);

  // useEffect(() => {
  //   console.log(amountDrawn);
  // }, [amountDrawn]);

  function drawStroke(start, end, color) {
    const canvas = canvasRef.current;

    if (canvas) {
      if (!isDrawing) {
        color = "black";
      }

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
        {/* {gameActive &&  */}
        <canvas ref={canvasRef} className="canvas"></canvas>
        {/* } */}
        {isDrawing && <DrawingInfo clearCanvas={sendClearCanvas} word={word} />}
      </div>
    </>
  );
};

export default DrawingBoard;
