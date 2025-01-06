import React from "react";

const DrawingInfo = ({ clearCanvas, word, changeWord }) => {
  return (
    <div className="draw-info-container">
      <div>
        <p>Du och XXX ritar</p>
        <p>Rita en: {word.toUpperCase()}</p>
        <button onClick={changeWord}>Ändra ord</button>
      </div>
      <button className="btn" onClick={clearCanvas}>
        Rensa
      </button>
    </div>
  );
};

export default DrawingInfo;
