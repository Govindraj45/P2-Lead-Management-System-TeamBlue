export default function Pagination({ page, totalPages, onPageChange }) {
  if (totalPages <= 1) return null;

  const pages = [];
  const start = Math.max(1, page - 2);
  const end = Math.min(totalPages, page + 2);

  for (let i = start; i <= end; i++) {
    pages.push(i);
  }

  return (
    <nav className="flex items-center justify-center gap-1.5 mt-8">
      <button
        onClick={() => onPageChange(page - 1)}
        disabled={page <= 1}
        className="flex items-center gap-1 px-3 py-2 text-sm rounded-lg bg-white border border-gray-200 shadow-sm font-medium text-gray-600 disabled:opacity-40 disabled:cursor-not-allowed hover:bg-indigo-50 hover:text-indigo-600 hover:border-indigo-200"
      >
        <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}><path strokeLinecap="round" strokeLinejoin="round" d="M15 19l-7-7 7-7" /></svg>
        Prev
      </button>
      {start > 1 && <span className="px-1.5 text-gray-300">...</span>}
      {pages.map((p) => (
        <button
          key={p}
          onClick={() => onPageChange(p)}
          className={`w-9 h-9 text-sm rounded-lg font-semibold ${
            p === page
              ? 'text-white shadow-md shadow-indigo-500/30'
              : 'bg-white border border-gray-200 shadow-sm text-gray-600 hover:bg-indigo-50 hover:text-indigo-600 hover:border-indigo-200'
          }`}
          style={p === page ? { background: 'linear-gradient(135deg, #6366f1, #8b5cf6)' } : undefined}
        >
          {p}
        </button>
      ))}
      {end < totalPages && <span className="px-1.5 text-gray-300">...</span>}
      <button
        onClick={() => onPageChange(page + 1)}
        disabled={page >= totalPages}
        className="flex items-center gap-1 px-3 py-2 text-sm rounded-lg bg-white border border-gray-200 shadow-sm font-medium text-gray-600 disabled:opacity-40 disabled:cursor-not-allowed hover:bg-indigo-50 hover:text-indigo-600 hover:border-indigo-200"
      >
        Next
        <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}><path strokeLinecap="round" strokeLinejoin="round" d="M9 5l7 7-7 7" /></svg>
      </button>
    </nav>
  );
}
