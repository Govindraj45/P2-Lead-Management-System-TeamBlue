// This component shows page navigation buttons (like "1 2 3 ... Next") at the bottom of a list
// It takes the current page number, total pages, and a function to call when a page is clicked
export default function Pagination({ page, totalPages, onPageChange }) {
  // If there's only one page (or none), no pagination is needed — hide it
  if (totalPages <= 1) return null;

  // Build a list of page numbers to show (up to 2 pages before and after the current page)
  const pages = [];
  const start = Math.max(1, page - 2);
  const end = Math.min(totalPages, page + 2);

  // Loop through and add each page number to the array
  for (let i = start; i <= end; i++) {
    pages.push(i);
  }

  return (
    // The navigation bar that holds all the page buttons, centered on screen
    <nav className="flex items-center justify-center gap-1.5 mt-8">
      {/* "Previous" button — goes back one page, disabled if already on page 1 */}
      <button
        onClick={() => onPageChange(page - 1)}
        disabled={page <= 1}
        className="flex items-center gap-1 px-3 py-2 text-sm rounded-lg bg-white border border-gray-200 shadow-sm font-medium text-gray-600 disabled:opacity-40 disabled:cursor-not-allowed hover:bg-indigo-50 hover:text-indigo-600 hover:border-indigo-200"
      >
        {/* SVG icon for left arrow */}
        <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}><path strokeLinecap="round" strokeLinejoin="round" d="M15 19l-7-7 7-7" /></svg>
        Prev
      </button>
      {/* Show "..." if there are earlier pages that aren't displayed */}
      {start > 1 && <span className="px-1.5 text-gray-300">...</span>}
      {/* Render a button for each visible page number */}
      {pages.map((p) => (
        <button
          key={p}
          onClick={() => onPageChange(p)}
          // The current page gets a highlighted gradient style; other pages get a plain white style
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
      {/* Show "..." if there are later pages that aren't displayed */}
      {end < totalPages && <span className="px-1.5 text-gray-300">...</span>}
      {/* "Next" button — goes forward one page, disabled if already on the last page */}
      <button
        onClick={() => onPageChange(page + 1)}
        disabled={page >= totalPages}
        className="flex items-center gap-1 px-3 py-2 text-sm rounded-lg bg-white border border-gray-200 shadow-sm font-medium text-gray-600 disabled:opacity-40 disabled:cursor-not-allowed hover:bg-indigo-50 hover:text-indigo-600 hover:border-indigo-200"
      >
        Next
        {/* SVG icon for right arrow */}
        <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}><path strokeLinecap="round" strokeLinejoin="round" d="M9 5l7 7-7 7" /></svg>
      </button>
    </nav>
  );
}

/*
 * FILE SUMMARY: Pagination.jsx
 *
 * This file creates the page navigation controls (Prev, 1, 2, 3, ..., Next) shown at the
 * bottom of lists like the leads table. It figures out which page numbers to display based
 * on the current page, highlights the active page with a purple gradient, and disables the
 * Prev/Next buttons when you're at the first or last page. This component is essential for
 * browsing through large sets of data without loading everything at once.
 */
