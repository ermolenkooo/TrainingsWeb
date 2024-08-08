import { REPORTS_URL } from '../../const.js';
import { reportSerializer } from '../serializers/reportSerializer.js';
import { get, put, post, postFile, del } from '../general/base.js';

export const createReport = async (type, body) => {
    return await post(`${REPORTS_URL}${type}`, {
        fio: body.fio,
        position: body.position,
        date: body.date,
        trainingName: body.trainingName,
        criteriasWithMarks: body.criteriasWithMarks,
        endMark: body.endMark,
    });
};