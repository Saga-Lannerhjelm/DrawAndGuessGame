import React, { createContext, useContext, useState } from "react";

const ConnectionContext = createContext();

export const ConnectionProvider = ({ children }) => {
  const [connection, setConnection] = useState(undefined);
  const [activeUserId, setActiveUserId] = useState("");
  const [users, setUsers] = useState([]);
  return (
    <ConnectionContext.Provider
      value={{
        connection,
        setConnection,
        activeUserId,
        setActiveUserId,
        users,
        setUsers,
      }}
    >
      {children}
    </ConnectionContext.Provider>
  );
};

export const useConnection = () => useContext(ConnectionContext);
