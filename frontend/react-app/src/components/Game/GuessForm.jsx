import React, { useState } from "react";

const GuessForm = ({ sendGuess }) => {
  const [guess, setGuess] = useState("");
  return (
    <div>
      <form
        className="guess-form"
        onSubmit={(e) => {
          e.preventDefault();
          sendGuess(guess);
          setGuess("");
        }}
      >
        <input
          type="text"
          placeholder="Gissa"
          onChange={(e) => setGuess(e.target.value)}
          value={guess}
        />
        <button type="submit" className="btn" disabled={guess == ""}>
          Skicka
        </button>
      </form>
    </div>
  );
};

export default GuessForm;
