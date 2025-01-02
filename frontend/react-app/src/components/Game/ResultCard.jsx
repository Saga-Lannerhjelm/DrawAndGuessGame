import React from "react";
import { useConnection } from "../../context/ConnectionContext";

const ResultCard = ({ startNewRound, showGameResult, roundData }) => {
  const { users, activeUserId } = useConnection();
  console.log(users);
  return (
    <div className="result-box">
      <h2>Resultat {roundData && "För runda " + roundData.roundNr}</h2>
      {users.map((user, index) => (
        <>
          <p key={index}>
            {user.info.username} {activeUserId === user.info.id ? "(Du)" : ""}
            {user.info.totalPoints} (+{user.round.points})
          </p>
        </>
      ))}
      {!showGameResult && (
        <button onClick={startNewRound} className="btn">
          Nästa runda
        </button>
      )}
    </div>
  );
};

export default ResultCard;
