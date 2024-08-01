import React, { createContext, useState, useEffect, useRef } from 'react';
import { HubConnectionBuilder } from '@microsoft/signalr';

export const AppContext = createContext(null);

export const AppProvider = ({ children }) => {
    const [selectedTrainingId, setSelectedTrainingId] = useState(0);
    const [messages, setMessages] = useState([]);
    const [messages2, setMessages2] = useState([]);
    //const [removedConnection, setRemovedConnection] = useState(null);
    const connectionRef = useRef(null); // Используем useRef для хранения соединения

    //const removedConnection = new HubConnectionBuilder()
    //    .withUrl('/hub')
    //    //.withAutomaticReconnect()
    //    .build();

    useEffect(() => {
        //const connection = new HubConnectionBuilder()
        //    .withUrl('/hub')
        //    .withAutomaticReconnect()
        //    .build();

        //setRemovedConnection(connection);

        if (connectionRef.current) {
            return;
        }

        const connection = new HubConnectionBuilder()
            .withUrl('/hub')
            .withAutomaticReconnect()
            .build();

        if (connectionRef.current)
            return;

        connectionRef.current = connection; // Сохраняем соединение в ref

        //setRemovedConnection(connection); // Сохраняем соединение в состояния

        const startConnection = async () => {
            try {
                await connection.start();
                console.log('SignalR connected');
            } catch (error) {
                console.error('Error starting SignalR connection:', error);
                // Попробуйте переподключиться через 5 секунд
                setTimeout(startConnection, 5000);
            }
        };

        //connection.start()
        //    .then(() => {
        //        console.log('SignalR Connected Removed');
        //    })
        //    .catch((error) => {
        //        console.error(error);
        //        setTimeout(() => startConnection(), 5000);
        //    });

        connection.on("StartRemoved", () => {
            hubConnection.stop();
            setMessages([]);
            setMessages2([]);
        });

        connection.on("ReceiveRemoved", message => {
            addMessage(message);
            console.log(message);
        });

        connection.on("Receive2Removed", message => {
            addMessage2(message);
        });

        connection.on("ReceiveMarkRemoved", message => {
            localStorage.setItem('selectedTrainingMark', message);
        });

        connection.on("ReceiveStatusRemoved", message => {
            localStorage.setItem('selectedTrainingStatus', message);
        });

        connection.on("TrainingIsEndRemoved", () => {
            //removedConnection.stop();
        });

        connection.onclose(error => {
            console.log('SignalR Connection Removed closed', error);
            //setTimeout(startConnection, 5000);
        });

        startConnection();

        return () => {
            connection.stop();
            connectionRef.current = null; // Обнуляем ref
        };

    }, []);

    const hubConnection = new HubConnectionBuilder()
        .withUrl("/hub")
        .build();

    const addMessage = (newMessage) => {
        setMessages(prevMessages => [...prevMessages, newMessage]);
    };

    const addMessage2 = (newMessage) => {
        setMessages2(prevMessages => [...prevMessages, newMessage]);
    };

    return (
        <AppContext.Provider value={{ messages, addMessage, setMessages, messages2, addMessage2, setMessages2, removedConnection: connectionRef.current, hubConnection }}>
            {children}
        </AppContext.Provider>
    );
};