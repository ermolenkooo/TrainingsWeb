export const reportSerializer = (response) => {
    return {
        fio: response.fio || "",
        position: response.position || "",
        date: response.date || null,
        trainingName: response.trainingName || "",
        criteriasWithMarks: response.criteriasWithMarks || [],
        endMark: response.endMark || null,
    };
};