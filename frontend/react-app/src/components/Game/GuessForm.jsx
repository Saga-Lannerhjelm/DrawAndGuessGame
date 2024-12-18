import React, { useState } from "react";

const GuessForm = () => {
  const [guess, setGuess] = useState("");
  return (
    <form className="guess-form">
      <input
        type="text"
        placeholder="Gissa"
        onChange={(e) => setGuess(e.target.value)}
        value={guess}
      />
      <button type="submit" className="btn">
        Skicka
      </button>
    </form>
  );
};

export default GuessForm;
