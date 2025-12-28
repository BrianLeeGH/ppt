import { createJobsHubConnection } from './jobsHub';
import { useOutlineStore } from '../../stores/outlineStore';
import { useConversationStore } from '../../stores/conversationStore';
import type { Outline } from '../../stores/outlineStore';
import type { ConversationMessage } from '../api/conversationApi';

let hubConnection: ReturnType<typeof createJobsHubConnection> | null = null;

export async function initializeConversationHub(projectId: string) {
  if (hubConnection) {
    await hubConnection.stop();
  }

  hubConnection = createJobsHubConnection(projectId);
  const outlineStore = useOutlineStore();
  const conversationStore = useConversationStore();

  // Handle OutlineUpdated event
  hubConnection.connection.on('OutlineUpdated', (event: {
    projectId: string;
    outlineId: string;
    outline: Outline;
    revisionNumber: number;
    changedSlideIndices: number[];
  }) => {
    console.log('Outline updated event:', event);
    outlineStore.setOutline(event.outline);
    outlineStore.setChangedSlides(event.changedSlideIndices);
  });

  // Handle MessageReceived event
  hubConnection.connection.on('MessageReceived', (event: {
    projectId: string;
    message: ConversationMessage;
  }) => {
    console.log('Message received event:', event);
    conversationStore.addMessage(event.message);
  });

  await hubConnection.start();

  return hubConnection;
}

export async function disconnectConversationHub() {
  if (hubConnection) {
    await hubConnection.stop();
    hubConnection = null;
  }
}

export function getHubConnection() {
  return hubConnection;
}
