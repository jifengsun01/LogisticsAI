export interface ShipmentEvent {
  eventType: string;
  location: string;
  description: string;
  occurredAt: string;
  metadata?: string;
}

export interface RcaResult {
  rootCauseCategory: string;
  aiSummary: string;
  confidenceScore: number;
  analyzedAt: string;
}

export interface Shipment {
  id: string;
  trackingNumber: string;
  carrier: string;
  originCity: string;
  destinationCity: string;
  status: 'InTransit' | 'Delayed' | 'Delivered' | 'Critical';
  weightKg: number;
  scheduledEta: string;
  actualEta?: string;
  delayHours: number;
  createdAt: string;
  events?: ShipmentEvent[];
  latestRca?: RcaResult | null;
}

export interface ShipmentFilters {
  status?: string;
  carrier?: string;
  limit?: number;
}

export interface KpiSummary {
  totalActive: number;
  onTimeRate: number;
  avgDelayHours: number;
  criticalCount: number;
}

export interface ChatMessage {
  id: string;
  role: 'user' | 'assistant' | 'tool';
  content: string;
  tokensUsed?: number;
  createdAt: string;
}

export interface ChatResponse {
  reply: string;
  toolCallsMade: string[];
}
