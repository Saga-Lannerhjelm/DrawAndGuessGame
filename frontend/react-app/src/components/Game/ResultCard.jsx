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
        <div key={index} className="result-user">
          <p>
            {" "}
            {user.info.username}
            {user.round.isDrawing
              ? activeUserId === user.info.id
                ? "(Du - ritare)"
                : "(ritare)"
              : ""}
            {showGameResult && index === 0 ? "(vinnare)" : ""}
          </p>
          <p>
            {user.info.totalPoints} (+{user.round.points} )
          </p>
        </div>
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
