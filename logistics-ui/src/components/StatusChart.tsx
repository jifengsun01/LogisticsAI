import { PieChart, Pie, Cell, Tooltip, Legend, ResponsiveContainer } from 'recharts';
import type { Shipment } from '../types';

const STATUSES = ['InTransit', 'Delayed', 'Delivered', 'Critical'] as const;
const COLORS = ['#60a5fa', '#facc15', '#4ade80', '#f87171'];

interface Props {
  shipments: Shipment[];
}

export default function StatusChart({ shipments }: Props) {
  const data = STATUSES.map((status, i) => ({
    name: status,
    value: shipments.filter(s => s.status === status).length,
    color: COLORS[i],
  })).filter(d => d.value > 0);

  if (data.length === 0) return null;

  return (
    <div className="bg-white border rounded-lg p-4 shadow-sm">
      <h2 className="font-semibold mb-2">Shipment Status Breakdown</h2>
      <ResponsiveContainer width="100%" height={240}>
        <PieChart>
          <Pie data={data} dataKey="value" nameKey="name" cx="50%" cy="50%" outerRadius={80} label>
            {data.map((entry, i) => <Cell key={i} fill={entry.color} />)}
          </Pie>
          <Tooltip />
          <Legend />
        </PieChart>
      </ResponsiveContainer>
    </div>
  );
}
