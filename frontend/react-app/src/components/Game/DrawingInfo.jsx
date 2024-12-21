import React from "react";

const DrawingInfo = ({ clearCanvas, word }) => {
  return (
    <div className="draw-info-container">
      <div>
        <p>Du och XXX ritar</p>
        <p>Rita en: {word.toUpperCase()}</p>
      </div>
      <button className="btn" onClick={clearCanvas}>
        Rensa
      </button>
    </div>
  );
};

export default DrawingInfo;
