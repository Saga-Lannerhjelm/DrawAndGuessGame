import React from "react";

const Message = ({ message }) => {
  return (
    <div className="guess-box">
      <div>{message}</div>
    </div>
  );
};

export default Message;
