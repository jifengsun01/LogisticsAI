import type { ChatMessage, ChatResponse } from '../types';
import api from './client';

export const sendMessage = (sessionId: string, message: string): Promise<ChatResponse> =>
  api.post<ChatResponse>('/chat', { sessionId, message }).then(r => r.data);

export const getChatHistory = (sessionId: string): Promise<ChatMessage[]> =>
  api.get<ChatMessage[]>(`/chat/sessions/${sessionId}/messages`).then(r => r.data);
