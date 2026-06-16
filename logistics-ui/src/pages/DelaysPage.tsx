import { useCallback, useEffect, useState } from 'react';
import type { Shipment } from '../types';
import { getShipments } from '../api/shipments';

const STATUS_STYLE: Record<string, { bg: string; color: string; label: string }> = {
  Critical: { bg: '#FCEBEB', color: '#791F1F', label: 'Critical' },
  Delayed:  { bg: '#FAEEDA', color: '#633806', label: 'Delayed'  },
};

interface Props {
  onAskAbout: (message: string) => void;
}

export default function DelaysPage({ onAskAbout }: Props) {
  const [shipments, setShipments] = useState<Shipment[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const load = useCallback(async () => {
    try {
      const all = await getShipments({ limit: 100 });
      setShipments(all.filter(s => s.status === 'Delayed' || s.status === 'Critical')
                      .sort((a, b) => b.delayHours - a.delayHours));
      setError('');
    } catch {
      setError('Could not connect to API.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  const totalDelayHours = shipments.reduce((sum, s) => sum + s.delayHours, 0);
  const criticalCount   = shipments.filter(s => s.status === 'Critical').length;

  return (
    <div className="px-6 py-6 space-y-5 max-w-6xl mx-auto">
      <div>
        <h1 className="text-lg font-semibold text-slate-800">Delays &amp; Critical</h1>
        <p className="text-xs text-slate-400 mt-0.5">Shipments requiring attention</p>
      </div>

      {error && (
        <p className="text-xs text-red-500 bg-red-50 border border-red-200 rounded px-3 py-2">{error}</p>
      )}

      {/* Summary strip */}
      {!loading && shipments.length > 0 && (
        <div className="grid grid-cols-3 gap-4">
          {[
            { label: 'Affected Shipments', value: shipments.length.toString() },
            { label: 'Critical',           value: criticalCount.toString()    },
            { label: 'Total Delay Hours',  value: `${totalDelayHours}h`       },
          ].map(({ label, value }) => (
            <div key={label} className="bg-white border border-slate-200 rounded-xl px-5 py-4 shadow-sm">
              <p className="text-slate-400 uppercase tracking-wide" style={{ fontSize: 11 }}>{label}</p>
              <p className="font-bold text-slate-800 mt-1" style={{ fontSize: 24 }}>{value}</p>
            </div>
          ))}
        </div>
      )}

      <div className="bg-white border border-slate-200 rounded-xl overflow-hidden shadow-sm">
        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-slate-50 border-b border-slate-100 text-xs text-slate-500 uppercase tracking-wide">
                <th className="px-4 py-2.5 text-left font-medium">Tracking ID</th>
                <th className="px-4 py-2.5 text-left font-medium">Route</th>
                <th className="px-4 py-2.5 text-left font-medium">Carrier</th>
                <th className="px-4 py-2.5 text-left font-medium">Delay</th>
                <th className="px-4 py-2.5 text-left font-medium">Status</th>
                <th className="px-4 py-2.5 text-left font-medium"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {loading && (
                <tr>
                  <td colSpan={6} className="px-4 py-10 text-center text-slate-400 text-sm">Loading…</td>
                </tr>
              )}
              {!loading && shipments.map(s => {
                const st = STATUS_STYLE[s.status] ?? { bg: '#F1F5F9', color: '#475569', label: s.status };
                return (
                  <tr key={s.id} className="hover:bg-slate-50 transition-colors">
                    <td className="px-4 py-3 font-mono text-xs text-slate-500">{s.trackingNumber}</td>
                    <td className="px-4 py-3 text-slate-700">
                      <span className="font-medium">{s.originCity}</span>
                      <span className="text-slate-400 mx-1">→</span>
                      <span className="font-medium">{s.destinationCity}</span>
                    </td>
                    <td className="px-4 py-3 text-slate-500">{s.carrier}</td>
                    <td className="px-4 py-3">
                      <span className="text-sm font-bold text-orange-600">+{s.delayHours}h</span>
                    </td>
                    <td className="px-4 py-3">
                      <span style={{ backgroundColor: st.bg, color: st.color }}
                            className="inline-block px-2 py-0.5 rounded-full text-xs font-semibold">
                        {st.label}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <button
                        onClick={() => onAskAbout(`Run root-cause analysis for shipment ${s.trackingNumber}`)}
                        className="text-xs font-medium text-indigo-600 hover:text-indigo-800 hover:underline transition-colors"
                      >
                        Why? ↗
                      </button>
                    </td>
                  </tr>
                );
              })}
              {!loading && shipments.length === 0 && !error && (
                <tr>
                  <td colSpan={6} className="px-4 py-10 text-center text-slate-400 text-sm">
                    No delayed or critical shipments right now
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
