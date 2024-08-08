import React, { useEffect, useState } from 'react';
import './ReportModal.sass';

export const ReportModal = ({ show, onClose, onReportTypeChange, parentCallback }) => {
    const [selectedReportType, setSelectedReportType] = useState('');

    useEffect(() => {

    }, []);

    const handleChange = (event) => {
        setSelectedReportType(event.target.value);
        onReportTypeChange(event.target.value);
    };

    if (!show) {
        return null;
    }

    function handleCancel() {
        onClose();
        parentCallback(false);
    }

    function handleConfirm() {
        onClose();
        parentCallback(true);
    }

    return (
        <div className='report-modal-window'>
            <div className='report-modal-window__component'>
                <p className='report-modal-window__component__title'>Составление отчёта</p>

                <div className='settings-modal-window__component__content'>
                    <label className='settings-modal-window__component__content__text' htmlFor="combobox">Выберите вариант отчёта:</label>
                    <select id="combobox" className='report-modal-window__component__content__select' value={selectedReportType} onChange={handleChange}>
                        <option value="" disabled selected hidden>Выберите вариант отчёта</option>
                        <option value="1">Отчёт по противоаварийной тренировке</option>
                        <option value="2">Отчёт по анализу тренировки пуска и останова</option>
                    </select>
                </div>

                <div className='report-modal-window__component__footler'>
                    <button className='report-modal-window__component__footler__cancel-btn' onClick={handleCancel}>Завершить без отчёта</button>
                    <button className='report-modal-window__component__footler__confirm-btn' onClick={handleConfirm}>Составить отчёт</button>
                </div>
            </div>
        </div>
    );
};