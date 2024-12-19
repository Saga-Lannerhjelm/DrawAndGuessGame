import React from "react";

const TopSection = () => {
  let currentRound = 1;
  const totalRounds = 3;
  let timeInSeconds = 30;
  return (
    <div className="top-section">
      <span>
        Runda {currentRound} av {totalRounds}
      </span>
      <span>00:{timeInSeconds}</span>
    </div>
  );
};

export default TopSection;
