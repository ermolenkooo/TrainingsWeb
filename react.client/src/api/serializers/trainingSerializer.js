export const trainingSerializer = (response) => {
    return {
        id: response.id || 0,
        name: response.name || "",
        description: response.description || "",
        startMarka: response.startMarka || "",
        mark: response.mark || null,
        startDateTime: response.startDateTime || null,
    };
};

export const trainingSerializerList = (response) => {
    return {
        response: response && response.length > 0 ? response.map((elem) => trainingSerializer(elem)) : [],
    };
};