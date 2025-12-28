import apiClient from './client';

export interface ConversationMessage {
  id: string;
  role: string;
  content: string;
  createdAt: string;
}

export interface SendMessageRequest {
  content: string;
}

export interface SendMessageResponse {
  message: ConversationMessage;
  jobId: string;
}

export interface GetMessagesResponse {
  messages: ConversationMessage[];
}

export const conversationApi = {
  async getMessages(projectId: string, limit: number = 50): Promise<GetMessagesResponse> {
    const response = await apiClient.get(`/projects/${projectId}/conversation/messages`, {
      params: { limit }
    });
    return response.data;
  },

  async sendMessage(projectId: string, content: string): Promise<SendMessageResponse> {
    const response = await apiClient.post(`/projects/${projectId}/conversation/messages`, {
      content
    });
    return response.data;
  }
};
