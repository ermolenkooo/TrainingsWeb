import { SETTINGS_URL } from '../../const.js';
import { settingsSerializer } from '../serializers/settingsSerializer.js';
import { get, put, post, postFile, del } from '../general/base.js';

export const getSettings = async () => {
    const response = await get(`${SETTINGS_URL}`);
    return settingsSerializer(response);
};

export const saveNewSettings = async (body) => {
    return await post(`${SETTINGS_URL}`, {
        archiveIp: body.archiveIp,
        archive2Ip: body.archive2Ip,
        archive3Ip: body.archive3Ip,
    });
};