import React from "react";
import { useConnection } from "../../context/ConnectionContext";

const ResultCard = ({ startNewRound, gameResult }) => {
  const { users, activeUserId } = useConnection();
  return (
    <div>
      ResultCard
      <button onClick={startNewRound} className="btn">
        Nästa runda
      </button>
    </div>
  );
};

export default ResultCard;
