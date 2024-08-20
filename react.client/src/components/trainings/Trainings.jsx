import React, { useEffect, useState, useContext } from 'react';
import { getTrainings, stopTraining } from '../../api/domains/trainingApi';
import { createReport } from '../../api/domains/reportApi';
import './Trainings.sass';
import { DescriptionModal } from '../modal/descriptionModal/DescriptionModal';
import { SettingsModal } from '../modal/settingsModal/SettingsModal';
import { ReportModal } from '../modal/reportModal/ReportModal';
import { usePopup } from '../modal/usePopup';
import { AppContext } from '../../api/contexts/appContext/AppContext';
import * as signalR from "@microsoft/signalr";

export const Trainings = () => {
    const [trainings, trainingsChange] = useState([]);
    const { criteriasWithMarks, criteriasWithMarksChange } = useContext(AppContext);
    const { criteriasWithMarks2, criteriasWithMarks2Change } = useContext(AppContext);
    const [selectedReportType, setSelectedReportType] = useState('');
    const [selectedTrainingDate, setSelectedTrainingDate] = useState(() => {
        return localStorage.getItem('selectedTrainingDate') || null;
    });
    const { addMessage, setMessages, addMessage2, setMessages2, removedConnection, hubConnection } = useContext(AppContext);
    const [selectedTrainingId, setSelectedTrainingId] = useState(() => {
        return parseInt(localStorage.getItem('selectedTrainingId')) || null;
    });
    const [isShowingDescriptionModal, toggleDescriptionModal] = usePopup();
    const [isShowingSettingsModal, toggleSettingsModal] = usePopup();
    const [isShowingReportModal, toggleReportModal] = usePopup();

    const handleReportTypeChange = (reportType) => {
        setSelectedReportType(reportType);
    };

    useEffect(() => {
        getTrainings().then(data => {
            trainingsChange(data.response);
        });

        //localStorage.setItem('selectedTrainingId', '');
        //localStorage.setItem('selectedTrainingStatus', '');
        //localStorage.setItem('selectedTrainingMark', '');
    }, []);

    const handleTrainingClick = (id) => {
        setSelectedTrainingId(id);
        localStorage.setItem('selectedTrainingId', id);
        localStorage.setItem('selectedTrainingStatus', '');
        localStorage.setItem('selectedTrainingMark', trainings.find(training => training.id === id).mark);
    };

    const setupSignalRConnection = () => {

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
                    criteriasWithMarksChange([]);
                    criteriasWithMarks2Change([]);
                });

                hubConnection.on("Receive", function (message) {
                    addMessage(message);
                });

                hubConnection.on("Receive2", function (message) {
                    addMessage2(message);
                });

                hubConnection.on("ReceiveCriterias1", function (message) {
                    var array = criteriasWithMarks;
                    array.push(message);
                    criteriasWithMarksChange(array);
                });

                hubConnection.on("ReceiveCriterias2", function (message) {
                    var array = criteriasWithMarks2;
                    array.push(message);
                    criteriasWithMarks2Change(array);
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
        if (selectedTrainingId != null) {
            localStorage.setItem('selectedTrainingDate', new Date);
            setSelectedTrainingDate(new Date);
            setupSignalRConnection();
        }
    };

    const endTraining = (reportNeed) => {
        if (selectedTrainingId != null && localStorage.getItem('selectedTrainingStatus') == "Начата") {

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

                    hubConnection.on("IsOver", function () {
                        if (selectedReportType != '' && reportNeed) {
                            var training = trainings.find(item => item.id === selectedTrainingId);
                            createReport(selectedReportType, {
                                fio: "",
                                position: "",
                                date: new Date(selectedTrainingDate).toISOString(),
                                trainingName: training.name,
                                criteriasWithMarks: selectedReportType === '1'? criteriasWithMarks : criteriasWithMarks2,
                                endMark: localStorage.getItem('selectedTrainingMark') || null,
                            });
                        }
                    });
                })
                .catch(function (err) {
                    return console.error('Error while starting connection: ' + err.toString());
                });
        }
    }

    return (
        <>
            <div className='trainings-page'>
                <DescriptionModal show={isShowingDescriptionModal} onClose={toggleDescriptionModal} data={trainings.length > 0 && selectedTrainingId != null ? trainings.find(training => training.id === selectedTrainingId).description : ''} />
                <SettingsModal show={isShowingSettingsModal} onClose={toggleSettingsModal} />
                <ReportModal show={isShowingReportModal} onClose={toggleReportModal} onReportTypeChange={handleReportTypeChange} parentCallback={endTraining} />
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
                    <button className='trainings-page__button' onClick={toggleReportModal}>Завершить оценку</button>
                    <button className='trainings-page__button'>Ожидать запуска стартовой марки</button>
                    <button className='trainings-page__button' onClick={toggleSettingsModal}>Настройки</button>
                </div>
            </div>         
        </>
    );
};