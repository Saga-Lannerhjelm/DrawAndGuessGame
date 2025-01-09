import React from "react";

const GameMessage = ({ msg, type }) => {
  console.log(type);
  return <div className={`error-container ${type}`}>{msg}</div>;
};

export default GameMessage;
