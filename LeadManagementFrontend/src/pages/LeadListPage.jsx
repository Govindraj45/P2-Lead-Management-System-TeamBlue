// Import React hooks: useState stores data in the component, useEffect runs code when the page loads or data changes
import { useState, useEffect } from 'react';
// Import Link for clickable navigation without full page reloads
import { Link } from 'react-router-dom';
// Import our lead service — it talks to the backend API to get, create, and delete leads
import { leadService } from '../services/leadService';
// Import the authentication context so we can check what role the logged-in user has
import { useAuth } from '../context/AuthContext';
// Import reusable UI components (colorful status labels, loading spinner, error banner, page buttons)
import StatusBadge from '../components/ui/StatusBadge';
import Spinner from '../components/ui/Spinner';
import ErrorAlert from '../components/ui/ErrorAlert';
import Pagination from '../components/ui/Pagination';

// The list of possible lead statuses used in the filter dropdown
const STATUSES = ['All', 'New', 'Contacted', 'Qualified', 'Unqualified', 'Converted'];
// The list of possible lead sources used in the filter dropdown
const SOURCES = ['All', 'Website', 'Referral', 'ColdCall', 'Event', 'Partner'];

// This is the main Lead List page — it shows all leads in a searchable, filterable table
export default function LeadListPage() {
  // Get the "hasRole" function so we can check if the user is an Admin (for delete access)
  const { hasRole } = useAuth();

  // State variables to hold the page data and UI state
  const [leads, setLeads] = useState([]);           // The list of leads to show in the table
  const [loading, setLoading] = useState(true);      // Whether data is still being fetched
  const [error, setError] = useState(null);          // Stores any error message
  const [page, setPage] = useState(1);               // The current page number (for pagination)
  const [totalPages, setTotalPages] = useState(1);   // How many pages of leads exist in total
  const [statusFilter, setStatusFilter] = useState('All');  // Which status the user is filtering by
  const [sourceFilter, setSourceFilter] = useState('All');  // Which source the user is filtering by
  const [search, setSearch] = useState('');           // The text the user typed in the search box

  // This function calls the backend API to fetch leads with the current filters and page number
  const fetchLeads = async () => {
    setLoading(true);
    setError(null);
    try {
      // Build the query parameters based on what filters are active
      const params = { page, pageSize: 10 };
      if (statusFilter !== 'All') params.status = statusFilter;
      if (sourceFilter !== 'All') params.source = sourceFilter;
      if (search.trim()) params.search = search.trim();

      // Send the request to the backend and store the results
      const response = await leadService.getAll(params);
      const data = response.data;
      setLeads(data.items || data);
      setTotalPages(data.totalPages || 1);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to load leads');
    } finally {
      setLoading(false);
    }
  };

  // Re-fetch leads whenever the page number, status filter, or source filter changes
  useEffect(() => {
    fetchLeads();
  }, [page, statusFilter, sourceFilter]);

  // When the user submits the search form, reset to page 1 and fetch leads
  const handleSearch = (e) => {
    e.preventDefault();
    setPage(1);
    fetchLeads();
  };

  // When the user clicks "Delete" on a lead, ask for confirmation then call the API
  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this lead?')) return;
    try {
      await leadService.delete(id);
      fetchLeads();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to delete lead');
    }
  };

  // Everything below is the visible page content (JSX)
  return (
    <div>
      {/* Page header with title and "New Lead" button */}
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-3xl font-extrabold text-gradient">Leads</h1>
          <p className="text-sm text-gray-500 mt-1">Manage and track your sales pipeline</p>
        </div>
        {/* Button that navigates to the "Create Lead" form */}
        <Link
          to="/leads/create"
          className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl text-sm font-semibold text-white shadow-lg shadow-indigo-500/25 hover:shadow-indigo-500/40 hover:scale-[1.02] active:scale-[0.98]"
          style={{ background: 'linear-gradient(135deg, #6366f1, #8b5cf6)' }}
        >
          {/* Plus icon (SVG) */}
          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 4v16m8-8H4" />
          </svg>
          New Lead
        </Link>
      </div>

      {/* Filter bar — lets the user narrow down leads by status, source, or search text */}
      <div className="bg-white/80 backdrop-blur-sm rounded-2xl shadow-sm border border-gray-100 p-5 mb-6">
        <div className="flex flex-wrap gap-4 items-end">
          {/* Status filter dropdown */}
          <div>
            <label className="block text-xs font-semibold text-gray-400 uppercase tracking-wider mb-1.5">Status</label>
            <select
              value={statusFilter}
              onChange={(e) => { setStatusFilter(e.target.value); setPage(1); }}
              className="block rounded-xl border border-gray-200 bg-gray-50 px-3 py-2 text-sm focus:border-indigo-400 focus:ring-2 focus:ring-indigo-400/20 focus:bg-white"
            >
              {STATUSES.map((s) => <option key={s} value={s}>{s}</option>)}
            </select>
          </div>
          {/* Source filter dropdown */}
          <div>
            <label className="block text-xs font-semibold text-gray-400 uppercase tracking-wider mb-1.5">Source</label>
            <select
              value={sourceFilter}
              onChange={(e) => { setSourceFilter(e.target.value); setPage(1); }}
              className="block rounded-xl border border-gray-200 bg-gray-50 px-3 py-2 text-sm focus:border-indigo-400 focus:ring-2 focus:ring-indigo-400/20 focus:bg-white"
            >
              {SOURCES.map((s) => <option key={s} value={s}>{s}</option>)}
            </select>
          </div>
          {/* Search form — user can type a name or email to search for */}
          <form onSubmit={handleSearch} className="flex gap-2">
            <div className="relative">
              {/* Magnifying glass search icon */}
              <svg className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
              </svg>
              <input
                type="text"
                placeholder="Search by name or email..."
                value={search}
                onChange={(e) => setSearch(e.target.value)}
                className="block rounded-xl border border-gray-200 bg-gray-50 pl-9 pr-3 py-2 text-sm focus:border-indigo-400 focus:ring-2 focus:ring-indigo-400/20 focus:bg-white"
              />
            </div>
            <button
              type="submit"
              className="px-4 py-2 text-sm rounded-xl bg-indigo-50 hover:bg-indigo-100 text-indigo-600 font-medium border border-indigo-100"
            >
              Search
            </button>
          </form>
        </div>
      </div>

      {/* Show an error banner if something went wrong */}
      <ErrorAlert message={error} onDismiss={() => setError(null)} />

      {/* Conditional rendering: show spinner while loading, empty state if no leads, or the table */}
      {loading ? (
        <Spinner size="lg" />
      ) : leads.length === 0 ? (
        // Empty state — shown when no leads match the current filters
        <div className="text-center py-16">
          {/* Empty inbox icon */}
          <div className="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-gray-100 mb-4">
            <svg className="w-8 h-8 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
              <path strokeLinecap="round" strokeLinejoin="round" d="M20 13V6a2 2 0 00-2-2H6a2 2 0 00-2 2v7m16 0v5a2 2 0 01-2 2H6a2 2 0 01-2-2v-5m16 0h-2.586a1 1 0 00-.707.293l-2.414 2.414a1 1 0 01-.707.293h-3.172a1 1 0 01-.707-.293l-2.414-2.414A1 1 0 006.586 13H4" />
            </svg>
          </div>
          <p className="text-gray-500 font-medium">No leads found</p>
          <p className="text-gray-400 text-sm mt-1">Try adjusting your filters or create a new lead</p>
        </div>
      ) : (
        <>
          {/* Leads table — displays each lead as a row */}
          <div className="bg-white/80 backdrop-blur-sm shadow-sm rounded-2xl border border-gray-100 overflow-hidden">
            <table className="min-w-full divide-y divide-gray-100">
              {/* Table header row with column names */}
              <thead>
                <tr style={{ background: 'linear-gradient(135deg, #1e1b4b, #312e81)' }}>
                  <th className="px-6 py-3.5 text-left text-xs font-semibold text-indigo-200 uppercase tracking-wider">Name</th>
                  <th className="px-6 py-3.5 text-left text-xs font-semibold text-indigo-200 uppercase tracking-wider">Email</th>
                  <th className="px-6 py-3.5 text-left text-xs font-semibold text-indigo-200 uppercase tracking-wider">Status</th>
                  <th className="px-6 py-3.5 text-left text-xs font-semibold text-indigo-200 uppercase tracking-wider">Source</th>
                  <th className="px-6 py-3.5 text-left text-xs font-semibold text-indigo-200 uppercase tracking-wider">Priority</th>
                  <th className="px-6 py-3.5 text-right text-xs font-semibold text-indigo-200 uppercase tracking-wider">Actions</th>
                </tr>
              </thead>
              {/* Table body — one row per lead, created by looping through the leads array */}
              <tbody className="divide-y divide-gray-50">
                {leads.map((lead) => (
                  <tr key={lead.leadId} className="hover:bg-indigo-50/50 group">
                    {/* Lead name — clicking it goes to the lead detail page */}
                    <td className="px-6 py-4 whitespace-nowrap">
                      <Link to={`/leads/${lead.leadId}`} className="text-sm font-semibold text-indigo-600 hover:text-indigo-800 group-hover:underline decoration-indigo-300 underline-offset-2">
                        {lead.name}
                      </Link>
                    </td>
                    {/* Lead email */}
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{lead.email}</td>
                    {/* Lead status shown as a colorful badge */}
                    <td className="px-6 py-4 whitespace-nowrap"><StatusBadge status={lead.status} /></td>
                    {/* Lead source with a small dot indicator */}
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="text-sm text-gray-500 flex items-center gap-1.5">
                        <span className="w-1.5 h-1.5 rounded-full bg-gray-300" />
                        {lead.source}
                      </span>
                    </td>
                    {/* Lead priority — color-coded: red for High, amber for Medium, gray for Low */}
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className={`inline-flex items-center gap-1.5 text-sm font-medium ${
                        lead.priority === 'High' ? 'text-rose-600' :
                        lead.priority === 'Medium' ? 'text-amber-600' : 'text-gray-500'
                      }`}>
                        <span className={`w-2 h-2 rounded-full ${
                          lead.priority === 'High' ? 'bg-rose-500' :
                          lead.priority === 'Medium' ? 'bg-amber-400' : 'bg-gray-300'
                        }`} />
                        {lead.priority}
                      </span>
                    </td>
                    {/* Action buttons — View, Edit (if not converted), Delete (only for Admins) */}
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm space-x-1">
                      <Link
                        to={`/leads/${lead.leadId}`}
                        className="inline-flex items-center px-2.5 py-1 rounded-lg text-gray-500 hover:text-indigo-600 hover:bg-indigo-50 font-medium"
                      >
                        View
                      </Link>
                      {/* Only show "Edit" if the lead is NOT converted (converted leads are read-only) */}
                      {lead.status !== 'Converted' && (
                        <Link
                          to={`/leads/${lead.leadId}/edit`}
                          className="inline-flex items-center px-2.5 py-1 rounded-lg text-indigo-500 hover:text-indigo-700 hover:bg-indigo-50 font-medium"
                        >
                          Edit
                        </Link>
                      )}
                      {/* Only Admins can see and use the "Delete" button */}
                      {hasRole('Admin') && (
                        <button
                          onClick={() => handleDelete(lead.leadId)}
                          className="inline-flex items-center px-2.5 py-1 rounded-lg text-rose-500 hover:text-rose-700 hover:bg-rose-50 font-medium"
                        >
                          Delete
                        </button>
                      )}
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
          {/* Pagination controls — lets the user move between pages of results */}
          <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
        </>
      )}
    </div>
  );
}

/*
 * FILE SUMMARY:
 * This is the Lead List page — the main page where users see all their sales leads in a table.
 * It supports filtering by status and source, searching by name or email, and paginating through
 * results (10 leads per page). Each lead row shows name, email, status badge, source, and priority.
 * Users can view or edit any lead, but converted leads cannot be edited. Only Admins can delete leads.
 * The page automatically re-fetches data whenever filters or the page number change.
 */
