import React, { useEffect, useState } from 'react';
import { getSettings, saveNewSettings } from '../../../api/domains/settingsApi'
import './SettingsModal.sass'

export const SettingsModal = ({ show, onClose, parentCallback }) => {
    const [ip1, setIp1] = useState('');
    const [ip2, setIp2] = useState('');
    const [ip3, setIp3] = useState('');

    useEffect(() => async () => {
        const data = await getSettings();
        setIp1(data.ip1);
        setIp2(data.ip2);
        setIp3(data.ip3);
    }, []);

    const ip1Handler = (event) => {
        setIp1(event.target.value);
    };

    const ip2Handler = (event) => {
        setIp2(event.target.value);
    };

    const ip3Handler = (event) => {
        setIp3(event.target.value);
    };

    if (!show) {
        return null;
    }

    function handleConfirm() {
        saveNewSettings({
            archiveIp: ip1,
            archive2Ip: ip2,
            archive3Ip: ip3,
        });
        onClose();
    }

    return (
        <div className='settings-modal-window'>
            <div className='settings-modal-window__component'>
                <p className='settings-modal-window__component__title'>Настройки</p>

                <div className='settings-modal-window__component__content'>
                    <label className='settings-modal-window__component__content__text' htmlFor="inputField1">IP для шлюза и архива 1</label>
                    <input
                        className='settings-modal-window__component__content__input'
                        type="text"
                        id="inputField1"
                        value={ip1}
                        onChange={ip1Handler}
                    />

                    <label className='settings-modal-window__component__content__text' htmlFor="inputField2">IP для шлюза и архива 2</label>
                    <input
                        className='settings-modal-window__component__content__input'
                        type="text"
                        id="inputField2"
                        value={ip2}
                        onChange={ip2Handler}
                    />

                    <label className='settings-modal-window__component__content__text' htmlFor="inputField2">IP для шлюза и архива 3</label>
                    <input
                        className='settings-modal-window__component__content__input'
                        type="text"
                        id="inputField2"
                        value={ip3}
                        onChange={ip3Handler}
                    />
                </div>

                <div className='settings-modal-window__component__footler'>
                    <button className='settings-modal-window__component__footler__cancel-btn' onClick={onClose}>Отмена</button>
                    <button className='settings-modal-window__component__footler__confirm-btn' onClick={handleConfirm}>Сохранить</button>
                </div>
            </div>
        </div>
    );
};