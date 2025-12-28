import * as signalR from '@microsoft/signalr';

const hubUrl = import.meta.env.VITE_HUB_BASE_URL || 'http://localhost:5000/hubs/jobs';

export const createJobsHubConnection = (projectId: string) => {
  const connection = new signalR.HubConnectionBuilder()
    .withUrl(hubUrl)
    .withAutomaticReconnect()
    .build();

  connection.on('JobStatusUpdated', (status) => {
    console.log('Job status updated:', status);
  });

  connection.on('JobProgressUpdated', (progress) => {
    console.log('Job progress updated:', progress);
  });

  const start = async () => {
    try {
      await connection.start();
      console.log('SignalR Connected.');
      await connection.invoke('JoinProjectGroup', projectId);
    } catch (err) {
      console.log('SignalR Connection Error: ', err);
      setTimeout(start, 5000);
    }
  };

  return {
    connection,
    start,
    stop: () => connection.stop(),
  };
};
