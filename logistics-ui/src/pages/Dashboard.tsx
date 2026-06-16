import { useCallback, useEffect, useState } from 'react';
import type { KpiSummary, Shipment } from '../types';
import { getKpis, getShipmentDetail, getShipments } from '../api/shipments';

interface Props {
  onAskAbout: (message: string) => void;
}

// ── Status badge ─────────────────────────────────────────────────────────────

const STATUS_STYLE: Record<string, { bg: string; color: string; label: string }> = {
  Critical:  { bg: '#FCEBEB', color: '#791F1F', label: 'Critical'   },
  Delayed:   { bg: '#FAEEDA', color: '#633806', label: 'Delayed'    },
  InTransit: { bg: '#EFF6FF', color: '#1E40AF', label: 'In Transit' },
  Delivered: { bg: '#ECFDF5', color: '#065F46', label: 'Delivered'  },
};

function StatusBadge({ status }: { status: string }) {
  const style = STATUS_STYLE[status] ?? { bg: '#F1F5F9', color: '#475569', label: status };
  return (
    <span
      style={{ backgroundColor: style.bg, color: style.color }}
      className="inline-block px-2 py-0.5 rounded-full text-xs font-semibold"
    >
      {style.label}
    </span>
  );
}

// ── KPI card ──────────────────────────────────────────────────────────────────

function KpiSkeleton() {
  return (
    <div className="bg-white border border-slate-200 rounded-xl px-5 py-4 shadow-sm space-y-2 animate-pulse">
      <div className="h-2.5 w-24 bg-slate-200 rounded" />
      <div className="h-7 w-16 bg-slate-200 rounded" />
      <div className="h-2 w-32 bg-slate-100 rounded" />
    </div>
  );
}

function KpiCard({ label, value, sub }: { label: string; value: string | number; sub: string }) {
  return (
    <div className="bg-white border border-slate-200 rounded-xl px-5 py-4 shadow-sm">
      <p className="text-slate-400 uppercase tracking-wide" style={{ fontSize: 11 }}>{label}</p>
      <p className="font-bold text-slate-800 mt-1" style={{ fontSize: 24 }}>{value}</p>
      <p className="text-slate-400 mt-0.5" style={{ fontSize: 11 }}>{sub}</p>
    </div>
  );
}

// ── Table row ─────────────────────────────────────────────────────────────────

function ShipmentRow({
  shipment,
  onRca,
  rcaLoading,
}: {
  shipment: Shipment;
  onRca: (s: Shipment) => void;
  rcaLoading: boolean;
}) {
  return (
    <tr className="hover:bg-slate-50 transition-colors">
      <td className="px-4 py-3">
        <span className="font-mono text-xs text-slate-500 max-w-[88px] truncate block">
          {shipment.trackingNumber}
        </span>
      </td>
      <td className="px-4 py-3 text-slate-700">
        <span className="font-medium">{shipment.originCity}</span>
        <span className="text-slate-400 mx-1">→</span>
        <span className="font-medium">{shipment.destinationCity}</span>
      </td>
      <td className="px-4 py-3 text-slate-500 text-sm">{shipment.carrier}</td>
      <td className="px-4 py-3">
        {shipment.delayHours > 0 ? (
          <span className="text-sm font-semibold text-orange-600">
            +{shipment.delayHours}h
          </span>
        ) : (
          <span className="text-sm text-slate-400">—</span>
        )}
      </td>
      <td className="px-4 py-3">
        <StatusBadge status={shipment.status} />
      </td>
      <td className="px-4 py-3">
        <button
          onClick={() => onRca(shipment)}
          disabled={rcaLoading}
          className="text-xs font-medium text-indigo-600 hover:text-indigo-800 hover:underline
                     disabled:opacity-40 disabled:cursor-not-allowed transition-colors"
        >
          {rcaLoading ? '…' : 'Why? ↗'}
        </button>
      </td>
    </tr>
  );
}

// ── Dashboard ─────────────────────────────────────────────────────────────────

export default function Dashboard({ onAskAbout }: Props) {
  const [shipments, setShipments] = useState<Shipment[]>([]);
  const [kpis, setKpis] = useState<KpiSummary | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');
  const [rcaLoading, setRcaLoading] = useState<string | null>(null);

  const load = useCallback(async () => {
    try {
      const [s, k] = await Promise.all([getShipments(), getKpis()]);
      setShipments(s);
      setKpis(k);
      setError('');
    } catch {
      setError('Could not connect to API — check that the backend is running.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => {
    load();
    const interval = setInterval(load, 30_000);
    return () => clearInterval(interval);
  }, [load]);

  const handleRca = async (shipment: Shipment) => {
    setRcaLoading(shipment.id);
    try {
      const detail = await getShipmentDetail(shipment.id);
      onAskAbout(`Run root-cause analysis for shipment ${detail.trackingNumber}`);
    } catch {
      onAskAbout(`Run root-cause analysis for shipment ${shipment.trackingNumber}`);
    } finally {
      setRcaLoading(null);
    }
  };

  const activeShipments = shipments.filter(s => s.status !== 'Delivered');

  return (
    <div className="px-6 py-6 space-y-6 max-w-6xl mx-auto">

      {/* Page header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-lg font-semibold text-slate-800">Overview</h1>
          <p className="text-xs text-slate-400 mt-0.5">Updates every 30 s</p>
        </div>
        {error && (
          <p className="text-xs text-red-500 bg-red-50 border border-red-200 rounded px-3 py-1.5">
            {error}
          </p>
        )}
      </div>

      {/* KPI cards — 3 across, skeletons while fetching */}
      <div className="grid grid-cols-3 gap-4">
        {loading ? (
          <>
            <KpiSkeleton />
            <KpiSkeleton />
            <KpiSkeleton />
          </>
        ) : (
          <>
            <KpiCard
              label="Active Shipments"
              value={kpis?.totalActive ?? '—'}
              sub="currently in transit or delayed"
            />
            <KpiCard
              label="On-Time Rate"
              value={kpis ? `${(kpis.onTimeRate * 100).toFixed(1)}%` : '—'}
              sub="of shipments on schedule"
            />
            <KpiCard
              label="Avg Delay"
              value={kpis ? `${kpis.avgDelayHours.toFixed(1)} h` : '—'}
              sub="across delayed shipments"
            />
          </>
        )}
      </div>

      {/* Delayed / active shipments table */}
      <div className="bg-white border border-slate-200 rounded-xl overflow-hidden shadow-sm">
        <div className="px-4 py-3 border-b border-slate-100 flex items-center justify-between">
          <div>
            <h2 className="text-sm font-semibold text-slate-700">Active &amp; Delayed Shipments</h2>
            <p className="text-xs text-slate-400 mt-0.5">{activeShipments.length} shipments</p>
          </div>
          {kpis && kpis.criticalCount > 0 && (
            <span
              style={{ backgroundColor: '#FCEBEB', color: '#791F1F' }}
              className="text-xs font-semibold px-2.5 py-1 rounded-full"
            >
              {kpis.criticalCount} critical
            </span>
          )}
        </div>

        <div className="overflow-x-auto">
          <table className="w-full text-sm">
            <thead>
              <tr className="bg-slate-50 border-b border-slate-100 text-xs text-slate-500 uppercase tracking-wide">
                <th className="px-4 py-2.5 text-left font-medium w-28">Tracking ID</th>
                <th className="px-4 py-2.5 text-left font-medium">Route</th>
                <th className="px-4 py-2.5 text-left font-medium">Carrier</th>
                <th className="px-4 py-2.5 text-left font-medium w-20">Delay</th>
                <th className="px-4 py-2.5 text-left font-medium w-28">Status</th>
                <th className="px-4 py-2.5 text-left font-medium w-16"></th>
              </tr>
            </thead>
            <tbody className="divide-y divide-slate-100">
              {activeShipments
                .slice()
                .sort((a, b) => b.delayHours - a.delayHours)
                .map(s => (
                  <ShipmentRow
                    key={s.id}
                    shipment={s}
                    onRca={handleRca}
                    rcaLoading={rcaLoading === s.id}
                  />
                ))}
              {activeShipments.length === 0 && (
                <tr>
                  <td colSpan={6} className="px-4 py-10 text-center text-slate-400 text-sm">
                    {loading
                      ? 'Loading…'
                      : error
                        ? 'Could not load shipments'
                        : 'No delayed shipments right now'}
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
