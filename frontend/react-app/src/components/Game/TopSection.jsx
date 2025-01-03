import React from "react";

const TopSection = ({ time, round }) => {
  let totalRounds = 3;
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
