// This object maps each lead status to a set of colors (background, text, ring, dot)
// Each status gets its own unique color so the user can quickly identify it visually
const STATUS_CONFIG = {
  New: { bg: 'bg-blue-50', text: 'text-blue-700', ring: 'ring-blue-600/20', dot: 'bg-blue-500' },
  Contacted: { bg: 'bg-amber-50', text: 'text-amber-700', ring: 'ring-amber-600/20', dot: 'bg-amber-500' },
  Qualified: { bg: 'bg-emerald-50', text: 'text-emerald-700', ring: 'ring-emerald-600/20', dot: 'bg-emerald-500' },
  Unqualified: { bg: 'bg-rose-50', text: 'text-rose-700', ring: 'ring-rose-600/20', dot: 'bg-rose-500' },
  Converted: { bg: 'bg-violet-50', text: 'text-violet-700', ring: 'ring-violet-600/20', dot: 'bg-violet-500' },
};

// This component displays a small colored badge showing the current status of a lead
export default function StatusBadge({ status }) {
  // Look up the colors for this status, or fall back to gray if the status is unknown
  const config = STATUS_CONFIG[status] || { bg: 'bg-gray-50', text: 'text-gray-700', ring: 'ring-gray-600/20', dot: 'bg-gray-500' };
  return (
    // A small pill-shaped badge with a colored background, border ring, and text
    <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold ring-1 ring-inset ${config.bg} ${config.text} ${config.ring}`}>
      {/* A tiny colored dot that pulses gently to draw attention */}
      <span className={`w-1.5 h-1.5 rounded-full ${config.dot} animate-pulse-glow`} />
      {/* The status name displayed as text (e.g., "New", "Qualified") */}
      {status}
    </span>
  );
}

/*
 * FILE SUMMARY: StatusBadge.jsx
 *
 * This file creates a small colored pill/badge that shows the current status of a lead
 * (New, Contacted, Qualified, Unqualified, or Converted). Each status gets its own
 * color scheme so users can tell at a glance where a lead is in the sales pipeline.
 * It includes a tiny animated dot for visual emphasis. This badge is used in lead
 * tables and detail pages throughout the application.
 */
