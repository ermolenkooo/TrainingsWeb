import React, { useEffect, useState, useContext } from 'react';
import { getTrainings } from '../../api/domains/trainingApi';
import './Trainings.sass';
import { DescriptionModal } from '../modal/descriptionModal/DescriptionModal';
import { usePopup } from '../modal/usePopup';
import { WEB_SOCKET_URL } from '../../const.js'
import { AppContext } from '../../api/contexts/appContext/AppContext'

export const Trainings = () => {
    const [trainings, trainingsChange] = useState([]);
    const { messages, setMessages } = useContext(AppContext);
    const [selectedTrainingId, setSelectedTrainingId] = useState(() => {
        return parseInt(localStorage.getItem('selectedTrainingId')) || null;
    });
    const [isShowingDescriptionModal, toggleDescriptionModal] = usePopup();

    const { addMessage } = useContext(AppContext);

    useEffect(() => async () => {
        const data = await getTrainings();
        trainingsChange(data.response);
    });

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
            const newMessage = event.data;
            addMessage(newMessage);
            console.log(messages);
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
            localStorage.setItem('selectedTrainingStatus', 'начата');
            setupWebSocketConnection();
        }
    };

    const endTrainingClick = () => {
        localStorage.setItem('selectedTrainingStatus', 'завершена');
    };

    const awaitStartClick = () => {

    };

    const openSettingsClick = () => {

    };

    return (
        <>
            <div className='trainings-page'>
                <DescriptionModal show={isShowingDescriptionModal} onClose={toggleDescriptionModal} data={trainings.length > 0 && selectedTrainingId != null ? trainings.find(training => training.id === selectedTrainingId).description : ''} />
                <div className='trainings-page__col'>
                    <p className='trainings-page__title'>Перечень тренировок</p>
                    {trainings.map((element) =>
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
                    <button className='trainings-page__button' onClick={() => awaitStartClick()}>Ожидать запуска стартовой марки</button>
                    <button className='trainings-page__button' onClick={() => openSettingsClick()}>Настройки</button>
                </div>
            </div>         
        </>
    );
};