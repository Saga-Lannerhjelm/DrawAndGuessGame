import React, { createContext, useContext, useState } from "react";

const ConnectionContext = createContext();

export const ConnectionProvider = ({ children }) => {
  const [connection, setConnection] = useState(undefined);
  const [activeUser, setActiveUser] = useState("");
  return (
    <ConnectionContext.Provider
      value={{ connection, setConnection, activeUser, setActiveUser }}
    >
      {children}
    </ConnectionContext.Provider>
  );
};

export const useConnection = () => useContext(ConnectionContext);
