import React from 'react';
import './DescriptionModal.sass'

export const DescriptionModal = ({ show, onClose, parentCallback, data }) => {
    if (!show) {
        return null;
    }

    function handleConfirm() {
        if (parentCallback)
            parentCallback();
        onClose();
    }

    return (
        <div className='description-modal-window'>
            <div className='description-modal-window__component'>
                <p className='description-modal-window__component__title'>Описание тренировки</p>

                <div className='description-modal-window__component__content'>
                    <textarea className='description-modal-window__component__content__textarea' value={data} disabled/>
                </div>

                <div className='description-modal-window__component__footler'>
                    <button className='description-modal-window__component__footler__confirm-btn' onClick={handleConfirm}>ОК</button>
                </div>
            </div>
        </div>
    );
};