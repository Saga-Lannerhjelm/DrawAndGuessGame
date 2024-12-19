import React from "react";

const Message = ({ message }) => {
  return (
    <>
      <div className="guess-box">
        <div>{message}</div>
        <svg
          width="56"
          height="37"
          viewBox="0 0 56 37"
          fill="none"
          xmlns="http://www.w3.org/2000/svg"
          className="arrow"
        >
          <path
            d="M18.801 31.3925L46.4125 23.2768C51.6779 21.7292 51.6778 14.2708 46.4125 12.7232L18.801 4.60753C15.2785 3.5722 11.75 6.21288 11.75 9.88432L11.75 26.1157C11.75 29.7871 15.2785 32.4278 18.801 31.3925Z"
            fill="white"
            stroke="#242424"
            stroke-width="5"
          />
          <path
            d="M21 10.5C21 5.3 19.6667 1.33333 19 0C10.6 0 9 6 9 9V24C9 34.8 15.5 36.5 19 36C21 30.5 20.5 29 21 25.5V10.5Z"
            fill="white"
          />
        </svg>
      </div>
    </>
  );
};

export default Message;
