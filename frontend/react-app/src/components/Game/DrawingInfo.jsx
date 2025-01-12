import React, { useEffect } from "react";

const DrawingInfo = ({ clearCanvas, word, changeWord, users }) => {
  const findArtists = () => {
    artists = users.filter((u) => u.round.isDrawing);
  };
  let artists;
  findArtists();

  useEffect(() => {
    findArtists();
  }, [users]);

  return (
    <div className="draw-info-container">
      <div>
        <p>
          {artists[0].info.username} och {artists[1].info.username} ritar
        </p>
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
