import React, { createContext, useState } from 'react';

export const AppContext = createContext(null);

export const AppProvider = ({ children }) => {
    const [selectedTrainingId, setSelectedTrainingId] = useState(0);
    const [messages, setMessages] = useState([]);
    const [messages2, setMessages2] = useState([]);

    const addMessage = (newMessage) => {
        setMessages(prevMessages => [...prevMessages, newMessage]);
    };

    const addMessage2 = (newMessage) => {
        setMessages2(prevMessages => [...prevMessages, newMessage]);
    };

    return (
        <AppContext.Provider value={{ messages, addMessage, setMessages, messages2, addMessage2, setMessages2 }}>
            {children}
        </AppContext.Provider>
    );
};