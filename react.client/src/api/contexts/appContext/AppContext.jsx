import React, { createContext, useState } from 'react';

export const AppContext = createContext(null);

export const AppProvider = ({ children }) => {
    const [selectedTrainingId, setSelectedTrainingId] = useState(1);
    const [messages, setMessages] = useState([]);

    const addMessage = (newMessage) => {
        setMessages(prevMessages => [...prevMessages, newMessage]);
    };

    return (
        <AppContext.Provider value={{ messages, addMessage, setMessages }}>
            {children}
        </AppContext.Provider>
    );
};