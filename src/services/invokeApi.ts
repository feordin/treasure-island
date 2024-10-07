import { CommandRequest } from '../modules/ProcessCommandResponse';

// invoke the azure function here
export const invokeAzureFunction = async (command: CommandRequest) => {
    try {
        console.log('Invoking Azure function at url: ', process.env.REACT_APP_API_URL + '/api/ProcessGameCommand');
        const response = await fetch(process.env.REACT_APP_API_URL + '/api/ProcessGameCommand', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(command),
        });

        if (!response.ok) {
            throw new Error('Network response was not ok');
        }

        const data = await response.text();
        return data;
    } catch (error) {
        console.error('Error invoking Azure function:', error);
        return { response: 'There was an error processing your command.', newImage: undefined };
    }
};