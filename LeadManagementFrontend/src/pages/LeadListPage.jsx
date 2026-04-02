import { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { leadService } from '../services/leadService';
import { useAuth } from '../context/AuthContext';
import StatusBadge from '../components/ui/StatusBadge';
import Spinner from '../components/ui/Spinner';
import ErrorAlert from '../components/ui/ErrorAlert';
import Pagination from '../components/ui/Pagination';

const STATUSES = ['All', 'New', 'Contacted', 'Qualified', 'Unqualified', 'Converted'];
const SOURCES = ['All', 'Website', 'Referral', 'ColdCall', 'Event', 'Partner'];

export default function LeadListPage() {
  const { hasRole } = useAuth();
  const [leads, setLeads] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);
  const [page, setPage] = useState(1);
  const [totalPages, setTotalPages] = useState(1);
  const [statusFilter, setStatusFilter] = useState('All');
  const [sourceFilter, setSourceFilter] = useState('All');
  const [search, setSearch] = useState('');

  const fetchLeads = async () => {
    setLoading(true);
    setError(null);
    try {
      const params = { page, pageSize: 10 };
      if (statusFilter !== 'All') params.status = statusFilter;
      if (sourceFilter !== 'All') params.source = sourceFilter;
      if (search.trim()) params.search = search.trim();

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

  useEffect(() => {
    fetchLeads();
  }, [page, statusFilter, sourceFilter]);

  const handleSearch = (e) => {
    e.preventDefault();
    setPage(1);
    fetchLeads();
  };

  const handleDelete = async (id) => {
    if (!window.confirm('Are you sure you want to delete this lead?')) return;
    try {
      await leadService.delete(id);
      fetchLeads();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to delete lead');
    }
  };

  return (
    <div>
      <div className="flex items-center justify-between mb-8">
        <div>
          <h1 className="text-3xl font-extrabold text-gradient">Leads</h1>
          <p className="text-sm text-gray-500 mt-1">Manage and track your sales pipeline</p>
        </div>
        <Link
          to="/leads/create"
          className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl text-sm font-semibold text-white shadow-lg shadow-indigo-500/25 hover:shadow-indigo-500/40 hover:scale-[1.02] active:scale-[0.98]"
          style={{ background: 'linear-gradient(135deg, #6366f1, #8b5cf6)' }}
        >
          <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
            <path strokeLinecap="round" strokeLinejoin="round" d="M12 4v16m8-8H4" />
          </svg>
          New Lead
        </Link>
      </div>

      {/* Filters */}
      <div className="bg-white/80 backdrop-blur-sm rounded-2xl shadow-sm border border-gray-100 p-5 mb-6">
        <div className="flex flex-wrap gap-4 items-end">
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
          <form onSubmit={handleSearch} className="flex gap-2">
            <div className="relative">
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

      <ErrorAlert message={error} onDismiss={() => setError(null)} />

      {loading ? (
        <Spinner size="lg" />
      ) : leads.length === 0 ? (
        <div className="text-center py-16">
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
          <div className="bg-white/80 backdrop-blur-sm shadow-sm rounded-2xl border border-gray-100 overflow-hidden">
            <table className="min-w-full divide-y divide-gray-100">
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
              <tbody className="divide-y divide-gray-50">
                {leads.map((lead) => (
                  <tr key={lead.leadId} className="hover:bg-indigo-50/50 group">
                    <td className="px-6 py-4 whitespace-nowrap">
                      <Link to={`/leads/${lead.leadId}`} className="text-sm font-semibold text-indigo-600 hover:text-indigo-800 group-hover:underline decoration-indigo-300 underline-offset-2">
                        {lead.name}
                      </Link>
                    </td>
                    <td className="px-6 py-4 whitespace-nowrap text-sm text-gray-500">{lead.email}</td>
                    <td className="px-6 py-4 whitespace-nowrap"><StatusBadge status={lead.status} /></td>
                    <td className="px-6 py-4 whitespace-nowrap">
                      <span className="text-sm text-gray-500 flex items-center gap-1.5">
                        <span className="w-1.5 h-1.5 rounded-full bg-gray-300" />
                        {lead.source}
                      </span>
                    </td>
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
                    <td className="px-6 py-4 whitespace-nowrap text-right text-sm space-x-1">
                      <Link
                        to={`/leads/${lead.leadId}`}
                        className="inline-flex items-center px-2.5 py-1 rounded-lg text-gray-500 hover:text-indigo-600 hover:bg-indigo-50 font-medium"
                      >
                        View
                      </Link>
                      {lead.status !== 'Converted' && (
                        <Link
                          to={`/leads/${lead.leadId}/edit`}
                          className="inline-flex items-center px-2.5 py-1 rounded-lg text-indigo-500 hover:text-indigo-700 hover:bg-indigo-50 font-medium"
                        >
                          Edit
                        </Link>
                      )}
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
          <Pagination page={page} totalPages={totalPages} onPageChange={setPage} />
        </>
      )}
    </div>
  );
}
