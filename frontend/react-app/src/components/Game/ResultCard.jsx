import React from "react";
import { useConnection } from "../../context/ConnectionContext";

const ResultCard = ({ startNewRound, gameResult }) => {
  const { users, activeUser } = useConnection();
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
