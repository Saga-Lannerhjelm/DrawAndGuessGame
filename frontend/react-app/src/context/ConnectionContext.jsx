import React, { createContext, useContext, useState } from "react";

const ApplicationContext = createContext();

export const ConnectionProvider = ({ children }) => {
  const [connection, setConnection] = useState(undefined);
  const [activeUserId, setActiveUserId] = useState("");
  const [users, setUsers] = useState([]);
  const [jwt, setJwt] = useState("");
  return (
    <ApplicationContext.Provider
      value={{
        connection,
        setConnection,
        activeUserId,
        setActiveUserId,
        users,
        setUsers,
        jwt,
        setJwt,
      }}
    >
      {children}
    </ApplicationContext.Provider>
  );
};

export const useConnection = () => useContext(ApplicationContext);
