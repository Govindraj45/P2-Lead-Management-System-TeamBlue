const STATUS_CONFIG = {
  New: { bg: 'bg-blue-50', text: 'text-blue-700', ring: 'ring-blue-600/20', dot: 'bg-blue-500' },
  Contacted: { bg: 'bg-amber-50', text: 'text-amber-700', ring: 'ring-amber-600/20', dot: 'bg-amber-500' },
  Qualified: { bg: 'bg-emerald-50', text: 'text-emerald-700', ring: 'ring-emerald-600/20', dot: 'bg-emerald-500' },
  Unqualified: { bg: 'bg-rose-50', text: 'text-rose-700', ring: 'ring-rose-600/20', dot: 'bg-rose-500' },
  Converted: { bg: 'bg-violet-50', text: 'text-violet-700', ring: 'ring-violet-600/20', dot: 'bg-violet-500' },
};

export default function StatusBadge({ status }) {
  const config = STATUS_CONFIG[status] || { bg: 'bg-gray-50', text: 'text-gray-700', ring: 'ring-gray-600/20', dot: 'bg-gray-500' };
  return (
    <span className={`inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold ring-1 ring-inset ${config.bg} ${config.text} ${config.ring}`}>
      <span className={`w-1.5 h-1.5 rounded-full ${config.dot} animate-pulse-glow`} />
      {status}
    </span>
  );
}
