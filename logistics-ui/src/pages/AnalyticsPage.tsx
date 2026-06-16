import { useEffect, useState } from 'react';
import { PieChart, Pie, Cell, Tooltip, Legend, ResponsiveContainer, BarChart, Bar, XAxis, YAxis, CartesianGrid } from 'recharts';
import type { KpiSummary, Shipment } from '../types';
import { getKpis, getShipments } from '../api/shipments';

const STATUS_COLORS: Record<string, string> = {
  InTransit: '#60a5fa',
  Delayed:   '#facc15',
  Delivered: '#4ade80',
  Critical:  '#f87171',
};

const CARRIER_COLORS = ['#818cf8', '#34d399', '#fb923c', '#f472b6'];

export default function AnalyticsPage() {
  const [shipments, setShipments] = useState<Shipment[]>([]);
  const [kpis, setKpis] = useState<KpiSummary | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    Promise.all([getShipments({ limit: 100 }), getKpis()])
      .then(([s, k]) => { setShipments(s); setKpis(k); })
      .finally(() => setLoading(false));
  }, []);

  if (loading) {
    return (
      <div className="px-6 py-6 max-w-6xl mx-auto">
        <div className="h-6 w-40 bg-slate-200 rounded animate-pulse mb-6" />
        <div className="grid grid-cols-2 gap-6">
          {[1, 2].map(i => <div key={i} className="bg-white border border-slate-200 rounded-xl h-64 animate-pulse" />)}
        </div>
      </div>
    );
  }

  // Status breakdown
  const statusData = ['InTransit', 'Delayed', 'Delivered', 'Critical'].map(s => ({
    name: s === 'InTransit' ? 'In Transit' : s,
    value: shipments.filter(sh => sh.status === s).length,
    color: STATUS_COLORS[s],
  })).filter(d => d.value > 0);

  // Delay by carrier
  const carriers = [...new Set(shipments.map(s => s.carrier))];
  const carrierData = carriers.map((carrier, i) => {
    const group = shipments.filter(s => s.carrier === carrier && s.delayHours > 0);
    return {
      carrier,
      avgDelay: group.length
        ? Math.round(group.reduce((sum, s) => sum + s.delayHours, 0) / group.length)
        : 0,
      count: group.length,
      color: CARRIER_COLORS[i % CARRIER_COLORS.length],
    };
  }).sort((a, b) => b.avgDelay - a.avgDelay);

  return (
    <div className="px-6 py-6 space-y-6 max-w-6xl mx-auto">
      <div>
        <h1 className="text-lg font-semibold text-slate-800">Analytics</h1>
        <p className="text-xs text-slate-400 mt-0.5">Based on {shipments.length} shipments</p>
      </div>

      {/* Summary row */}
      {kpis && (
        <div className="grid grid-cols-4 gap-4">
          {[
            { label: 'Active',      value: kpis.totalActive.toString()             },
            { label: 'On-Time',     value: `${(kpis.onTimeRate * 100).toFixed(1)}%` },
            { label: 'Avg Delay',   value: `${kpis.avgDelayHours.toFixed(1)}h`      },
            { label: 'Critical',    value: kpis.criticalCount.toString()            },
          ].map(({ label, value }) => (
            <div key={label} className="bg-white border border-slate-200 rounded-xl px-4 py-3 shadow-sm">
              <p className="text-slate-400 uppercase tracking-wide" style={{ fontSize: 11 }}>{label}</p>
              <p className="font-bold text-slate-800 mt-1 text-2xl">{value}</p>
            </div>
          ))}
        </div>
      )}

      <div className="grid grid-cols-2 gap-6">
        {/* Status pie */}
        <div className="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
          <h2 className="text-sm font-semibold text-slate-700 mb-4">Status Breakdown</h2>
          <ResponsiveContainer width="100%" height={220}>
            <PieChart>
              <Pie data={statusData} dataKey="value" nameKey="name" cx="50%" cy="50%" outerRadius={80} label>
                {statusData.map((entry, i) => <Cell key={i} fill={entry.color} />)}
              </Pie>
              <Tooltip />
              <Legend />
            </PieChart>
          </ResponsiveContainer>
        </div>

        {/* Avg delay by carrier */}
        <div className="bg-white border border-slate-200 rounded-xl p-5 shadow-sm">
          <h2 className="text-sm font-semibold text-slate-700 mb-4">Avg Delay by Carrier (hours)</h2>
          <ResponsiveContainer width="100%" height={220}>
            <BarChart data={carrierData} margin={{ top: 4, right: 8, left: -10, bottom: 0 }}>
              <CartesianGrid strokeDasharray="3 3" stroke="#f1f5f9" />
              <XAxis dataKey="carrier" tick={{ fontSize: 11 }} />
              <YAxis tick={{ fontSize: 11 }} />
              <Tooltip />
              <Bar dataKey="avgDelay" name="Avg Delay (h)" radius={[4, 4, 0, 0]}>
                {carrierData.map((entry, i) => <Cell key={i} fill={entry.color} />)}
              </Bar>
            </BarChart>
          </ResponsiveContainer>
        </div>
      </div>
    </div>
  );
}
