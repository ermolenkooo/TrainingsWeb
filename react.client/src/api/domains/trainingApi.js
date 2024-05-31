import { TRAINING_URL, STOP_TRAINING_URL } from '../../const.js';
import { trainingSerializerList, trainingSerializer } from '../serializers/trainingSerializer.js';
import { get, put, post, postFile, del } from '../general/base.js';

export const getTrainings = async () => {
    const response = await get(`${TRAINING_URL}`);
    return trainingSerializerList(response);
};

export const getTraining = async (id) => {
    const response = await get(`${TRAINING_URL}${id}`);
    return trainingSerializer(response);
};

export const stopTraining = async (id) => {
    await get(`${STOP_TRAINING_URL}${id}`);
};