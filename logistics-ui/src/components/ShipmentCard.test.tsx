import { render, screen } from '@testing-library/react';
import ShipmentCard from './ShipmentCard';
import type { Shipment } from '../types';

// A factory that returns a minimal valid Shipment object.
// We override only the fields each test cares about using the spread operator: { ...baseShipment(), delayHours: 5 }
const baseShipment = (): Shipment => ({
  id: '1',
  trackingNumber: 'TRK-001',
  carrier: 'FedEx',
  originCity: 'Los Angeles, CA',
  destinationCity: 'New York, NY',
  status: 'InTransit',
  weightKg: 10,
  scheduledEta: '2026-07-20T00:00:00Z',
  delayHours: 0,
  createdAt: '2026-07-13T00:00:00Z',
});

// ── Test 1 ────────────────────────────────────────────────────────────────────

// describe() groups related tests under a label — makes the output easier to read.
describe('ShipmentCard', () => {

  it('renders the tracking number and route', () => {
    // render() mounts the component into a virtual DOM (jsdom).
    // We pass a plain object — no real API call needed.
    render(<ShipmentCard shipment={baseShipment()} />);

    // screen.getByText() searches the rendered output for an element with this text.
    // If the element is NOT found, the test fails automatically.
    expect(screen.getByText('TRK-001')).toBeInTheDocument();

    // The component renders "Los Angeles, CA → New York, NY" as one string.
    expect(screen.getByText('Los Angeles, CA → New York, NY')).toBeInTheDocument();
  });

  // ── Test 2 ──────────────────────────────────────────────────────────────────

  it('shows delay text only when delayHours > 0', () => {
    // Override just delayHours using spread: copy all base fields, then replace delayHours.
    const { rerender } = render(<ShipmentCard shipment={{ ...baseShipment(), delayHours: 0 }} />);

    // queryByText() returns null (instead of throwing) when the element is not found.
    // We use this when we EXPECT something to be absent.
    expect(screen.queryByText(/Delayed/)).not.toBeInTheDocument();

    // rerender() updates the same mounted component with new props — no remount needed.
    rerender(<ShipmentCard shipment={{ ...baseShipment(), delayHours: 5 }} />);

    // Now delayHours is 5, so "Delayed 5h" should appear.
    // /Delayed 5h/ is a regex — it matches any string containing "Delayed 5h".
    expect(screen.getByText(/Delayed 5h/)).toBeInTheDocument();
  });

  // ── Test 3 ──────────────────────────────────────────────────────────────────

  it('shows AI analysis section only when latestRca is present', () => {
    // No latestRca — the purple AI Analysis block should not render.
    const { rerender } = render(<ShipmentCard shipment={baseShipment()} />);
    expect(screen.queryByText(/AI Analysis/)).not.toBeInTheDocument();

    // Now add a latestRca object — the component should render the analysis block.
    rerender(<ShipmentCard shipment={{
      ...baseShipment(),
      latestRca: {
        rootCauseCategory: 'WeatherDelay',
        aiSummary: 'Storm caused a 5-hour delay at the Chicago hub.',
        confidenceScore: 0.92,
        analyzedAt: '2026-07-13T00:00:00Z',
      },
    }} />);

    // "AI Analysis:" is rendered as a <span> inside the purple box.
    expect(screen.getByText(/AI Analysis/)).toBeInTheDocument();
    expect(screen.getByText(/Storm caused a 5-hour delay/)).toBeInTheDocument();
  });
});
