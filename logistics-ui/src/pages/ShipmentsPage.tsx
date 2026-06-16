import { useCallback, useEffect, useState } from 'react';
import type { Shipment } from '../types';
import { getShipments } from '../api/shipments';

const STATUS_STYLE: Record<string, { bg: string; color: string; label: string }> = {
  Critical:  { bg: '#FCEBEB', color: '#791F1F', label: 'Critical'   },
  Delayed:   { bg: '#FAEEDA', color: '#633806', label: 'Delayed'    },
  InTransit: { bg: '#EFF6FF', color: '#1E40AF', label: 'In Transit' },
  Delivered: { bg: '#ECFDF5', color: '#065F46', label: 'Delivered'  },
};

function StatusBadge({ status }: { status: string }) {
  const s = STATUS_STYLE[status] ?? { bg: '#F1F5F9', color: '#475569', label: status };
  return (
    <span style={{ backgroundColor: s.bg, color: s.color }}
          className="inline-block px-2 py-0.5 rounded-full text-xs font-semibold">
      {s.label}
    </span>
  );
}

export default function ShipmentsPage() {
  const [shipments, setShipments] = useState<Shipment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const load = useCallback(async () => {
    try {
      setShipments(await getShipments({ limit: 100 }));
      setError('');
    } catch {
      setError('Could not connect to API.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  return (
    <div className="px-6 py-6 space-y-4 max-w-6xl mx-auto">
      <div>
        <h1 className="text-lg font-semibold text-slate-800">All Shipments</h1>
        <p className="text-xs text-slate-400 mt-0.5">{shipments.length} total</p>
      </div>

      {error && (
        <p className="text-xs text-red-500 bg-red-50 border border-red-200 rounded px-3 py-2">
          {error}
        </p>
      )}

      <div className="bg-white border border-slate-200 rounded-xl overflow-hidden shadow-sm">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-slate-50 border-b border-slate-100 text-xs text-slate-500 uppercase tracking-wide">
                <th className="px-4 py-2.5 text-left font-medium">Tracking ID</th>
                <th className="px-4 py-2.5 text-left font-medium">Route</th>
                <th className="px-4 py-2.5 text-left font-medium">Carrier</th>
                <th className="px-4 py-2.5 text-left font-medium">Weight</th>
                <th className="px-4 py-2.5 text-left font-medium">Scheduled ETA</th>
                <th className="px-4 py-2.5 text-left font-medium">Status</th>
                <th className="px-4 py-2.5 text-left font-medium">Delay</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {loading && (
                <tr>
                  <td colSpan={7} className="px-4 py-10 text-center text-slate-400 text-sm">Loading…</td>
                </tr>
              )}
              {!loading && shipments.map(s => (
                <tr key={s.id} className="hover:bg-slate-50 transition-colors">
                  <td className="px-4 py-3 font-mono text-xs text-slate-500">{s.trackingNumber}</td>
                  <td className="px-4 py-3 text-slate-700">
                    <span className="font-medium">{s.originCity}</span>
                    <span className="text-slate-400 mx-1">→</span>
                    <span className="font-medium">{s.destinationCity}</span>
                  </td>
                  <td className="px-4 py-3 text-slate-500">{s.carrier}</td>
                  <td className="px-4 py-3 text-slate-500">{s.weightKg} kg</td>
                  <td className="px-4 py-3 text-slate-500 text-xs">
                    {new Date(s.scheduledEta).toLocaleDateString()}
                  </td>
                  <td className="px-4 py-3"><StatusBadge status={s.status} /></td>
                  <td className="px-4 py-3">
                    {s.delayHours > 0
                      ? <span className="text-sm font-semibold text-orange-600">+{s.delayHours}h</span>
                      : <span className="text-slate-300">—</span>}
                  </td>
                </tr>
              ))}
              {!loading && shipments.length === 0 && !error && (
                <tr>
                  <td colSpan={7} className="px-4 py-10 text-center text-slate-400 text-sm">
                    No shipments found
                  </td>
                </tr>
              )}
            </tbody>
          </table>
        </div>
      </div>
    </div>
  );
}
