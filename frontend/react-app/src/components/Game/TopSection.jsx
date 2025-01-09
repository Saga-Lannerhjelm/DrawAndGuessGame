import React from "react";

const TopSection = ({ time, round, roundNr }) => {
  const totalRounds = roundNr;
  return (
    <div className="top-section">
      <span>
        Runda {round.roundNr} av {totalRounds}
      </span>
      <span>00:{time}</span>
    </div>
  );
};

export default TopSection;
