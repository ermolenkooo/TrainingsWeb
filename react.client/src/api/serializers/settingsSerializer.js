export const settingsSerializer = (response) => {
    return {
        ip1: response.archiveIp || "",
        ip2: response.archive2Ip || "",
        ip3: response.archive3Ip || "",
    };
};