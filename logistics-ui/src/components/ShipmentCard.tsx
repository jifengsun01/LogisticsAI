import type { Shipment } from '../types';

const STATUS_COLORS: Record<string, string> = {
  InTransit: 'bg-blue-100 text-blue-800',
  Delayed:   'bg-yellow-100 text-yellow-800',
  Delivered: 'bg-green-100 text-green-800',
  Critical:  'bg-red-100 text-red-800',
};

interface Props {
  shipment: Shipment;
}

export default function ShipmentCard({ shipment }: Props) {
  const colorClass = STATUS_COLORS[shipment.status] ?? 'bg-gray-100 text-gray-800';

  return (
    <div className="border rounded-lg p-4 shadow-sm bg-white space-y-2">
      <div className="flex justify-between items-start">
        <div>
          <p className="font-mono text-sm text-gray-500">{shipment.trackingNumber}</p>
          <p className="font-semibold">{shipment.originCity} → {shipment.destinationCity}</p>
          <p className="text-sm text-gray-600">{shipment.weightKg} kg · {shipment.carrier}</p>
        </div>
        <span className={`text-xs font-medium px-2 py-1 rounded-full ${colorClass}`}>
          {shipment.status}
        </span>
      </div>
      {shipment.delayHours > 0 && (
        <p className="text-xs text-orange-600 font-medium">Delayed {shipment.delayHours}h</p>
      )}
      {shipment.latestRca && (
        <div className="bg-purple-50 border border-purple-200 rounded p-2 text-xs text-purple-900">
          <span className="font-semibold">AI Analysis: </span>{shipment.latestRca.aiSummary}
        </div>
      )}
    </div>
  );
}
