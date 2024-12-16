import React, { useEffect, useRef, useState } from "react";

const Game = () => {
  const [color, setColor] = useState("#ffffff");
  const [loading, setLoading] = useState(true);
  const canvasRef = useRef(null);

  const getColor = async () => {
    try {
      const response = await fetch(
        "https://x-colors.yurace.pro/api/random/?type=dark"
      );

      if (!response.ok) {
        throw new Error(`Response status: ${response.status}`);
      }

      const result = await response.json();
      setColor(result.hex);
    } catch (error) {
      console.error(error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    getColor();
  }, []);

  useEffect(() => {
    const canvas = canvasRef.current;
    const ctx = canvas.getContext("2d");

    // canvas.style.width = window.innerWidth;
    // canvas.style.height = window.innerHeight - 200;
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight - 200;

    let penDown = false;
    let prevPoint = { x: 0, y: 0 };

    const draw = (e) => {
      if (!loading && penDown) {
        ctx.strokeStyle = color;
        ctx.lineWidth = 10;
        ctx.lineCap = "round";
        ctx.lineJoin = "round";
        ctx.beginPath();
        ctx.moveTo(
          prevPoint.x - canvas.offsetLeft,
          prevPoint.y - canvas.offsetTop
        );
        ctx.lineTo(e.pageX - canvas.offsetLeft, e.pageY - canvas.offsetTop);
        ctx.stroke();
      }
      prevPoint = { x: e.pageX, y: e.pageY };
    };

    canvas.addEventListener("mousedown", () => (penDown = true));
    canvas.addEventListener("mousemove", (e) => draw(e));
    canvas.addEventListener("mouseup", () => (penDown = false));

    return () => {
      canvas.addEventListener("mousedown", () => (penDown = true));
      canvas.addEventListener("mousemove", (e) => draw(e));
      canvas.addEventListener("mouseup", () => (penDown = false));
    };
  }, [color]);

  return (
    <>
      <div>
        <canvas ref={canvasRef} style={{ border: "1px solid white" }}></canvas>
      </div>
    </>
  );
};

export default Game;
