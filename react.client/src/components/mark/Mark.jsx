import './Mark.sass';
import React, { useEffect, useState, useContext } from 'react';
import { getTraining } from '../../api/domains/trainingApi';
import { AppContext } from '../../api/contexts/appContext/AppContext'

export const Mark = () => {
    const [ selectedTraining, setSelectedTraining ] = useState(null);
    const { selectedTrainingId } = useContext(AppContext);
    const { selectedTrainingStatus } = useContext(AppContext);
    const { selectedTrainingMark } = useContext(AppContext);
    //const [ selectedTrainingMark ] = useState(null);

    const { messages, messages2 } = useContext(AppContext);

    useEffect(() => {
        const fetchData = () => {
            getTraining(selectedTrainingId).then(data => {
                setSelectedTraining(data);
            });
        };

        if (selectedTrainingId) {
            fetchData();
        }
    }, [selectedTrainingId]);

    useEffect(() => {
        //setSelectedTrainingId(localStorage.getItem('selectedTrainingId'));
        //setSelectedTrainingStatus(localStorage.getItem('selectedTrainingStatus'));
        //setSelectedTrainingMark(localStorage.getItem('selectedTrainingMark'));
    }, []);

    return (
        <>
            <div className='mark-page'>
                <div className='mark-page__col'>
                    <div className='mark-page__row'> 
                        <p className='mark-page__title'>Наименование текущей тренировки: </p>
                        <p className='mark-page__text'> {selectedTraining ? selectedTraining.name : ''} </p>
                    </div>
                    <p></p>
                    <div className='mark-page__row'>
                        <p className='mark-page__title'>Описание текущей тренировки: </p>
                        <p className='mark-page__text'> {selectedTraining ? selectedTraining.description : ''} </p>
                    </div>
                    <p></p>
                    <div className='mark-page__row'>
                        <p className='mark-page__title'>Статус текущей тренировки: </p>
                        <p className='mark-page__text'> {selectedTrainingStatus} </p>
                    </div>
                    <p></p>
                    <div className='mark-page__row'>
                        <p className='mark-page__title'>Количество баллов в начале: </p>
                        <p className='mark-page__text'> {selectedTraining ? selectedTraining.mark : ''} </p>
                    </div>
                    <p></p>
                    <div className='mark-page__row'>
                        <p className='mark-page__title'>Итоговая оценка: </p>
                        <p className='mark-page__text'> {selectedTrainingMark} </p>
                    </div>
                </div>
                <div className='mark-page__col'>
                    <p className='mark-page__title'>Снятие баллов</p>
                    <textarea className='mark-page__big-textarea' value={messages.join('\n\n')} disabled/>
                    <p></p>
                    <textarea className='mark-page__textarea' value={messages2.join('\n\n')} disabled/>
                </div>
            </div>         
        </>
    );
};