import React, { useEffect } from "react";
import { useConnection } from "../../context/ConnectionContext";

const ResultCard = ({ startNewRound, showGameResult, roundData, endGame }) => {
  const { users, activeUserId } = useConnection();
  useEffect(() => {
    users.sort((a, b) => {
      b.totalPoints - a.totalPoints;
    });
  }, []);

  return (
    <div className="result-box">
      <h2>Resultat {roundData && "För runda " + roundData.roundNr}</h2>
      {users.map((user, index) => (
        <p key={index}>
          {user.info.username} {activeUserId === user.info.id ? "(Du)" : ""}
          {user.info.totalPoints} (+{user.round.points}{" "}
          {user.round.isDrawing ? "ritare" : ""})
          {showGameResult && index === 0 ? "vinnare" : ""}
        </p>
      ))}
      {!showGameResult ? (
        <button onClick={startNewRound} className="btn">
          Nästa runda
        </button>
      ) : (
        <button onClick={endGame} className="btn">
          Avsluta spelet
        </button>
      )}
    </div>
  );
};

export default ResultCard;
