import React, { useEffect, useState, useContext } from 'react';
import { getTrainings, stopTraining } from '../../api/domains/trainingApi';
import './Trainings.sass';
import { DescriptionModal } from '../modal/descriptionModal/DescriptionModal';
import { SettingsModal } from '../modal/settingsModal/SettingsModal';
import { usePopup } from '../modal/usePopup';
import { WEB_SOCKET_URL } from '../../const.js'
import { AppContext } from '../../api/contexts/appContext/AppContext'

export const Trainings = () => {
    const [trainings, trainingsChange] = useState([]);
    const { addMessage, setMessages, addMessage2, setMessages2 } = useContext(AppContext);
    const [selectedTrainingId, setSelectedTrainingId] = useState(() => {
        return parseInt(localStorage.getItem('selectedTrainingId')) || null;
    });
    const [isShowingDescriptionModal, toggleDescriptionModal] = usePopup();
    const [isShowingSettingsModal, toggleSettingsModal] = usePopup();

    useEffect(() => async () => {
        const data = await getTrainings();
        trainingsChange(data.response);
    }, []);

    const handleTrainingClick = (id) => {
        setSelectedTrainingId(id);
        localStorage.setItem('selectedTrainingId', id);
        localStorage.setItem('selectedTrainingStatus', '');
        localStorage.setItem('selectedTrainingMark', trainings.find(training => training.id === id).mark);
    };

    const setupWebSocketConnection = () => {
        const webSocket = new WebSocket(WEB_SOCKET_URL + selectedTrainingId);

        let intervalId;
        webSocket.onopen = function () {
            console.log('WebSocket соединение установлено.');
            intervalId = setInterval(function () {
                webSocket.send('ping'); // Отправляем ping
            }, 10);
        };

        webSocket.onmessage = (event) => {
            const messageObj = JSON.parse(event.data);
            if (messageObj.type == "textarea1")
                addMessage(messageObj.content);
            else if (messageObj.type == "textarea2")
                addMessage2(messageObj.content);
            else
                localStorage.setItem('selectedTrainingMark', messageObj.content);
        };

        webSocket.onerror = (error) => {
            console.error('Произошла ошибка в WebSocket соединении:', error);
        };

        webSocket.onclose = () => {
            console.log('WebSocket соединение закрыто');
        };
    };

    const startTrainingClick = () => {
        if (selectedTrainingId != null) {
            setMessages([]);
            setMessages2([]);
            localStorage.setItem('selectedTrainingStatus', 'начата');
            setupWebSocketConnection();
        }
    };

    const endTrainingClick = () => {
        if (selectedTrainingId != null && localStorage.getItem('selectedTrainingStatus') == "начата") {
            stopTraining(selectedTrainingId);
            localStorage.setItem('selectedTrainingStatus', 'завершена');
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