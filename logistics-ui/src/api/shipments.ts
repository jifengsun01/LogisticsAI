import type { KpiSummary, Shipment, ShipmentFilters } from '../types';
import api from './client';

export const getKpis = (): Promise<KpiSummary> =>
  api.get<KpiSummary>('/shipments/kpis').then(r => r.data);

export const getShipments = (filters: ShipmentFilters = {}): Promise<Shipment[]> =>
  api.get<Shipment[]>('/shipments', { params: filters }).then(r => r.data);

export const getShipmentDetail = (id: string): Promise<Shipment> =>
  api.get<Shipment>(`/shipments/${id}`).then(r => r.data);
