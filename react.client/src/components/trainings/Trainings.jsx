import React, { useEffect, useState, useContext } from 'react';
import { getTrainings, stopTraining } from '../../api/domains/trainingApi';
import './Trainings.sass';
import { DescriptionModal } from '../modal/descriptionModal/DescriptionModal';
import { SettingsModal } from '../modal/settingsModal/SettingsModal';
import { usePopup } from '../modal/usePopup';
import { AppContext } from '../../api/contexts/appContext/AppContext';
import * as signalR from "@microsoft/signalr";

export const Trainings = () => {
    const [trainings, trainingsChange] = useState([]);
    /*const [hubConnection, setHubConnection] = useState(null);*/
    const { addMessage, setMessages, addMessage2, setMessages2 } = useContext(AppContext);
    const [selectedTrainingId, setSelectedTrainingId] = useState(() => {
        return parseInt(localStorage.getItem('selectedTrainingId')) || null;
    });
    const [isShowingDescriptionModal, toggleDescriptionModal] = usePopup();
    const [isShowingSettingsModal, toggleSettingsModal] = usePopup();

    useEffect(() => {
        getTrainings().then(data => {
            trainingsChange(data.response);
        });

        const removedConnection = new signalR.HubConnectionBuilder()
            .withUrl('/hub')
            .build();

        removedConnection.start()
            .then(() => {
                console.log('SignalR Connected Removed');
            })
            .catch((error) => {
                console.error(error);
            });

        removedConnection.on("StartRemoved", () => {
            hubConnection.stop();
            setMessages([]);
            setMessages2([]);
        });

        removedConnection.on("ReceiveRemoved", message => {
            addMessage(message);
        });

        removedConnection.on("Receive2Removed", message => {
            addMessage2(message);
        });

        removedConnection.on("ReceiveMarkRemoved", message => {
            localStorage.setItem('selectedTrainingMark', message);
        });

        removedConnection.on("ReceiveStatusRemoved", message => {
            localStorage.setItem('selectedTrainingStatus', message);
        });

        removedConnection.on("TrainingIsEndRemoved", () => {
            //removedConnection.stop();
        });

        removedConnection.onclose(error => {
            console.log('SignalR Connection Removed closed', error);
        });

        return () => {
            removedConnection.stop();
        };

    }, []);

    const handleTrainingClick = (id) => {
        setSelectedTrainingId(id);
        localStorage.setItem('selectedTrainingId', id);
        localStorage.setItem('selectedTrainingStatus', '');
        localStorage.setItem('selectedTrainingMark', trainings.find(training => training.id === id).mark);
    };

    const hubConnection = new signalR.HubConnectionBuilder()
        .withUrl("/hub")
        .build();

    const setupSignalRConnection = () => {
        //const connection = new signalR.HubConnectionBuilder()
        //    .withUrl("/hub")
        //    .build();

        //setHubConnection(connection);

        hubConnection.start()
            .then(() => {
                console.log('SignalR Connected');
                hubConnection.invoke("Start", selectedTrainingId.toString())
                    .catch(function (err) {
                        return console.error(err.toString());
                    });

                hubConnection.on("Start", function () {
                    setMessages([]);
                    setMessages2([]);
                });

                hubConnection.on("Receive", function (message) {
                    addMessage(message);
                });

                hubConnection.on("Receive2", function (message) {
                    addMessage2(message);
                });

                hubConnection.on("ReceiveMark", function (message) {
                    localStorage.setItem('selectedTrainingMark', message);
                });

                hubConnection.on("ReceiveStatus", function (message) {
                    localStorage.setItem('selectedTrainingStatus', message);
                });

                hubConnection.on("TrainingIsEnd", function () {
                    hubConnection.stop();
                });
            })
            .catch(function (err) {
                return console.error('Error while starting connection: ' + err.toString());
            });

        hubConnection.onclose((error) => {
            console.log('SignalR Connection closed', error);
        });
    }

    const startTrainingClick = () => {
        if (selectedTrainingId != null)
            setupSignalRConnection();
    };

    const endTrainingClick = () => {
        if (selectedTrainingId != null && localStorage.getItem('selectedTrainingStatus') == "Начата") {
            //const hubConnection = new signalR.HubConnectionBuilder()
            //    .withUrl("/hub")
            //    .build();

            //hubConnection.invoke("End")
            //    .catch(function (err) {
            //        return console.error(err.toString());
            //    });

            hubConnection.start()
                .then(() => {
                    console.log('SignalR Connected');
                    hubConnection.invoke("End")
                        .catch(function (err) {
                            return console.error(err.toString());
                        });

                    hubConnection.on("TrainingIsEnd", function () {
                        hubConnection.stop();
                    });
                })
                .catch(function (err) {
                    return console.error('Error while starting connection: ' + err.toString());
                });
        }
    };

    return (
        <>
            <div className='trainings-page'>
                <DescriptionModal show={isShowingDescriptionModal} onClose={toggleDescriptionModal} data={trainings.length > 0 && selectedTrainingId != null ? trainings.find(training => training.id === selectedTrainingId).description : ''} />
                <SettingsModal show={isShowingSettingsModal} onClose={toggleSettingsModal} />
                <div className='trainings-page__col'>
                    <p className='trainings-page__title'>Перечень тренировок</p>
                    {
                        trainings.map((element) =>
                        <div
                            className={selectedTrainingId === element.id ? 'trainings-page__selected-element' : 'trainings-page__element'}
                            key={element.id}
                            onClick={() => handleTrainingClick(element.id)}>
                            {element.name}
                        </div>)}
                </div>
                <div className='trainings-page__col'>
                    <button className='trainings-page__button' onClick={toggleDescriptionModal}>Открыть описание тренировки</button>
                    <button className='trainings-page__button' onClick={() => startTrainingClick()}>Начать запись сценария и оценку</button>
                    <button className='trainings-page__button' onClick={() => endTrainingClick()}>Завершить оценку</button>
                    <button className='trainings-page__button'>Ожидать запуска стартовой марки</button>
                    <button className='trainings-page__button' onClick={toggleSettingsModal}>Настройки</button>
                </div>
            </div>         
        </>
    );
};