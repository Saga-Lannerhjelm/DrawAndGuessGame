import React from "react";

const DrawingInfo = () => {
  const word = "Fladdermus";
  return (
    <div className="draw-info-container">
      <div>
        <p>Du och XXX ritar</p>
        <p>Rita en: {word.toUpperCase()}</p>
      </div>
      <button className="btn">Rensa</button>
    </div>
  );
};

export default DrawingInfo;
