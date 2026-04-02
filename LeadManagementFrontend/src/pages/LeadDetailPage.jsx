// Import React hooks: useState stores data, useEffect runs code when the page loads
import { useState, useEffect } from 'react';
// Import routing tools: useParams reads the lead ID from the URL, Link creates navigation links, useNavigate for programmatic navigation
import { useParams, Link, useNavigate } from 'react-router-dom';
// Import services that talk to the backend APIs for leads and interactions
import { leadService } from '../services/leadService';
import { interactionService } from '../services/interactionService';
// Import the auth context so we can check user roles (e.g., only Managers/Admins can convert leads)
import { useAuth } from '../context/AuthContext';
// Import the map of allowed status transitions (e.g., "New" can go to "Contacted" but not "Converted")
import { VALID_TRANSITIONS } from '../utils/constants';
// Import reusable UI components
import StatusBadge from '../components/ui/StatusBadge';
import Spinner from '../components/ui/Spinner';
import ErrorAlert from '../components/ui/ErrorAlert';
import InteractionForm from '../components/forms/InteractionForm';

// This is the Lead Detail page — it shows everything about a single lead and its interactions
export default function LeadDetailPage() {
  // Get the lead ID from the URL (e.g., /leads/5 → id = "5")
  const { id } = useParams();
  const navigate = useNavigate();
  // Get the "hasRole" function to check the logged-in user's role
  const { hasRole } = useAuth();

  // State variables to hold the lead data, interactions, and UI state
  const [lead, setLead] = useState(null);                    // The lead's full details
  const [interactions, setInteractions] = useState([]);       // List of all interactions for this lead
  const [loading, setLoading] = useState(true);               // Whether the page is still loading
  const [actionLoading, setActionLoading] = useState(false);  // Whether a status change or conversion is in progress
  const [interactionLoading, setInteractionLoading] = useState(false); // Whether an interaction is being added
  const [error, setError] = useState(null);                   // Stores any error message

  // Fetch the lead details and its interactions from the backend at the same time
  const fetchData = async () => {
    setLoading(true);
    try {
      // Promise.all fetches both the lead and its interactions in parallel (faster)
      const [leadRes, interactionsRes] = await Promise.all([
        leadService.getById(id),
        interactionService.getByLeadId(id),
      ]);
      setLead(leadRes.data);
      setInteractions(interactionsRes.data);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to load lead');
    } finally {
      setLoading(false);
    }
  };

  // When the page loads (or the lead ID changes), fetch the data
  useEffect(() => {
    fetchData();
  }, [id]);

  // Called when the user clicks a "Move to [status]" button — sends a status change request to the backend
  const handleStatusChange = async (newStatus) => {
    setActionLoading(true);
    setError(null);
    try {
      await leadService.changeStatus(id, { status: newStatus });
      fetchData();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to change status');
    } finally {
      setActionLoading(false);
    }
  };

  // Called when the user clicks "Convert Lead" — this is irreversible and changes the lead to "Converted"
  const handleConvert = async () => {
    if (!window.confirm('Convert this lead? This action is irreversible.')) return;
    setActionLoading(true);
    setError(null);
    try {
      await leadService.convert(id);
      fetchData();
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to convert lead');
    } finally {
      setActionLoading(false);
    }
  };

  // Called when the user submits the "Add Interaction" form — creates a new interaction (call, email, meeting, or note)
  const handleAddInteraction = async (formData) => {
    setInteractionLoading(true);
    setError(null);
    try {
      await interactionService.create(id, formData);
      // After creating, re-fetch all interactions so the new one appears in the list
      const res = await interactionService.getByLeadId(id);
      setInteractions(res.data);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to add interaction');
    } finally {
      setInteractionLoading(false);
    }
  };

  // While data is loading, show a spinner. If no lead was found, show an error.
  if (loading) return <Spinner size="lg" />;
  if (!lead) return <ErrorAlert message={error || 'Lead not found'} />;

  // Determine what actions are available based on the lead's current status and the user's role
  const isConverted = lead.status === 'Converted';
  const allowedTransitions = VALID_TRANSITIONS[lead.status] || [];
  // Only Managers and Admins can convert a Qualified lead
  const canConvert = lead.status === 'Qualified' && hasRole('SalesManager', 'Admin');

  // Everything below is the visible page content (JSX)
  return (
    <div className="space-y-6">
      {/* Page header — "Back to Leads" link, lead name, Edit button, and Convert button */}
      <div className="flex items-center justify-between">
        <div>
          {/* Back arrow link to return to the leads list */}
          <Link to="/leads" className="inline-flex items-center gap-1 text-sm text-indigo-500 hover:text-indigo-700 font-medium">
            <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}><path strokeLinecap="round" strokeLinejoin="round" d="M15 19l-7-7 7-7" /></svg>
            Back to Leads
          </Link>
          {/* Lead name displayed with gradient text */}
          <h1 className="text-3xl font-extrabold text-gradient mt-1">{lead.name}</h1>
        </div>
        <div className="flex items-center gap-3">
          {/* Edit button — only shown if the lead is NOT converted (converted = read-only) */}
          {!isConverted && (
            <Link
              to={`/leads/${id}/edit`}
              className="inline-flex items-center gap-1.5 px-4 py-2 text-sm font-medium rounded-xl border border-gray-200 bg-white text-gray-700 hover:bg-gray-50 shadow-sm"
            >
              {/* Pencil edit icon */}
              <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}><path strokeLinecap="round" strokeLinejoin="round" d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z" /></svg>
              Edit
            </Link>
          )}
          {/* Convert button — only shown when the lead is Qualified and the user is a Manager or Admin */}
          {canConvert && (
            <button
              onClick={handleConvert}
              disabled={actionLoading}
              className="inline-flex items-center gap-1.5 px-5 py-2 text-sm font-semibold rounded-xl text-white shadow-lg shadow-emerald-500/25 hover:shadow-emerald-500/40 hover:scale-[1.02] active:scale-[0.98] disabled:opacity-50"
              style={{ background: 'linear-gradient(135deg, #10b981, #059669)' }}
            >
              {/* Checkmark circle icon */}
              <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}><path strokeLinecap="round" strokeLinejoin="round" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
              Convert Lead
            </button>
          )}
        </div>
      </div>

      {/* Show an error banner if something went wrong */}
      <ErrorAlert message={error} onDismiss={() => setError(null)} />

      {/* Lead Details card — shows all the lead's information in a grid */}
      <div className="bg-white/80 backdrop-blur-sm shadow-sm rounded-2xl border border-gray-100 overflow-hidden">
        {/* Dark indigo header */}
        <div className="px-6 py-4 border-b border-gray-100" style={{ background: 'linear-gradient(135deg, #1e1b4b, #312e81)' }}>
          <h2 className="text-sm font-semibold text-indigo-200 uppercase tracking-wider">Lead Information</h2>
        </div>
        {/* Grid of detail fields (email, phone, company, position, source, priority, status, created date) */}
        <div className="p-6">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <DetailField label="Email" value={lead.email} icon="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
            <DetailField label="Phone" value={lead.phone || '—'} icon="M3 5a2 2 0 012-2h3.28a1 1 0 01.948.684l1.498 4.493a1 1 0 01-.502 1.21l-2.257 1.13a11.042 11.042 0 005.516 5.516l1.13-2.257a1 1 0 011.21-.502l4.493 1.498a1 1 0 01.684.949V19a2 2 0 01-2 2h-1C9.716 21 3 14.284 3 6V5z" />
            <DetailField label="Company" value={lead.company || '—'} icon="M19 21V5a2 2 0 00-2-2H7a2 2 0 00-2 2v16m14 0h2m-2 0h-5m-9 0H3m2 0h5M9 7h1m-1 4h1m4-4h1m-1 4h1m-5 10v-5a1 1 0 011-1h2a1 1 0 011 1v5m-4 0h4" />
            <DetailField label="Position" value={lead.position || '—'} icon="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z" />
            <DetailField label="Source" value={lead.source} icon="M13.828 10.172a4 4 0 00-5.656 0l-4 4a4 4 0 105.656 5.656l1.102-1.101m-.758-4.899a4 4 0 005.656 0l4-4a4 4 0 00-5.656-5.656l-1.1 1.1" />
            <DetailField label="Priority" value={lead.priority} icon="M3 21v-4m0 0V5a2 2 0 012-2h6.5l1 1H21l-3 6 3 6h-8.5l-1-1H5a2 2 0 00-2 2zm9-13.5V9" />
            {/* Status shown as a colorful badge instead of plain text */}
            <div>
              <dt className="text-xs font-semibold text-gray-400 uppercase tracking-wider">Status</dt>
              <dd className="mt-2"><StatusBadge status={lead.status} /></dd>
            </div>
            <DetailField label="Created" value={new Date(lead.createdDate).toLocaleDateString()} icon="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z" />
            {/* Only show "Assigned Rep ID" if the lead has been assigned to someone */}
            {lead.assignedToRepId && (
              <DetailField label="Assigned Rep ID" value={lead.assignedToRepId} icon="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0" />
            )}
          </div>
        </div>
      </div>

      {/* Status Transitions section — shows buttons to move the lead to the next valid status */}
      {allowedTransitions.length > 0 && !isConverted && (
        <div className="bg-white/80 backdrop-blur-sm shadow-sm rounded-2xl border border-gray-100 p-6">
          <h2 className="text-sm font-semibold text-gray-400 uppercase tracking-wider mb-4">Status Transition</h2>
          <div className="flex items-center gap-3 flex-wrap">
            {/* Show the current status */}
            <StatusBadge status={lead.status} />
            {/* Arrow icon pointing to the next status options */}
            <svg className="w-5 h-5 text-gray-300" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}><path strokeLinecap="round" strokeLinejoin="round" d="M13 7l5 5m0 0l-5 5m5-5H6" /></svg>
            {/* Buttons for each valid next status (except "Converted" which has its own special button) */}
            {allowedTransitions
              .filter((s) => s !== 'Converted')
              .map((status) => (
                <button
                  key={status}
                  onClick={() => handleStatusChange(status)}
                  disabled={actionLoading}
                  className="px-4 py-2 text-sm font-semibold rounded-xl border border-indigo-200 text-indigo-700 bg-indigo-50 hover:bg-indigo-100 hover:border-indigo-300 disabled:opacity-50 shadow-sm"
                >
                  Move to {status}
                </button>
              ))}
            {/* Separate "Convert Lead" button with green gradient (only for Managers/Admins with Qualified leads) */}
            {canConvert && (
              <button
                onClick={handleConvert}
                disabled={actionLoading}
                className="px-4 py-2 text-sm font-semibold rounded-xl text-white disabled:opacity-50 shadow-sm"
                style={{ background: 'linear-gradient(135deg, #10b981, #059669)' }}
              >
                Convert Lead
              </button>
            )}
          </div>
        </div>
      )}

      {/* Interactions section — shows the timeline of all calls, emails, meetings, and notes for this lead */}
      <div className="bg-white/80 backdrop-blur-sm shadow-sm rounded-2xl border border-gray-100 overflow-hidden">
        {/* Header with interaction count badge */}
        <div className="px-6 py-4 border-b border-gray-100" style={{ background: 'linear-gradient(135deg, #1e1b4b, #312e81)' }}>
          <h2 className="text-sm font-semibold text-indigo-200 uppercase tracking-wider">
            Interactions
            {interactions.length > 0 && (
              <span className="ml-2 inline-flex items-center px-2 py-0.5 rounded-full text-xs bg-white/20 text-white font-medium">
                {interactions.length}
              </span>
            )}
          </h2>
        </div>
        <div className="p-6">
          {/* Show the "Add Interaction" form only if the lead is not converted */}
          {!isConverted && (
            <div className="mb-6 p-5 bg-gradient-to-r from-indigo-50/80 to-purple-50/50 rounded-xl border border-indigo-100">
              <h3 className="text-sm font-semibold text-indigo-700 mb-3 flex items-center gap-1.5">
                {/* Plus icon */}
                <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}><path strokeLinecap="round" strokeLinejoin="round" d="M12 4v16m8-8H4" /></svg>
                Add Interaction
              </h3>
              {/* The InteractionForm component handles the form fields and validation */}
              <InteractionForm
                onSubmit={handleAddInteraction}
                loading={interactionLoading}
                disabled={isConverted}
              />
            </div>
          )}

          {/* If there are no interactions yet, show an empty state message */}
          {interactions.length === 0 ? (
            <div className="text-center py-8">
              {/* Chat bubble icon */}
              <div className="inline-flex items-center justify-center w-12 h-12 rounded-xl bg-gray-100 mb-3">
                <svg className="w-6 h-6 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M8 12h.01M12 12h.01M16 12h.01M21 12c0 4.418-4.03 8-9 8a9.863 9.863 0 01-4.255-.949L3 20l1.395-3.72C3.512 15.042 3 13.574 3 12c0-4.418 4.03-8 9-8s9 3.582 9 8z" />
                </svg>
              </div>
              <p className="text-sm text-gray-500">No interactions yet</p>
            </div>
          ) : (
            // Timeline view — interactions are displayed along a vertical line, most like a chat history
            <div className="relative">
              {/* Vertical timeline line (gradient from indigo to transparent) */}
              <div className="absolute left-4 top-2 bottom-2 w-0.5 bg-gradient-to-b from-indigo-300 via-purple-300 to-transparent" />
              <div className="space-y-4">
                {/* Loop through each interaction and render it as a timeline card */}
                {interactions.map((interaction) => (
                  <div key={interaction.interactionId} className="relative pl-10">
                    {/* Small dot on the timeline line */}
                    <div className="absolute left-2.5 top-5 w-3 h-3 rounded-full bg-indigo-500 ring-4 ring-indigo-50" />
                    {/* Interaction card — shows type, date, notes, and optional follow-up date */}
                    <div className="bg-white border border-gray-100 rounded-xl p-4 shadow-sm hover:shadow-md card-hover">
                      <div className="flex items-center justify-between mb-2">
                        {/* Interaction type badge with emoji (Call, Email, Meeting, or Note) */}
                        <span className="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-lg text-xs font-semibold bg-indigo-50 text-indigo-700 ring-1 ring-inset ring-indigo-200">
                          {interaction.interactionType === 'Call' && '📞'}
                          {interaction.interactionType === 'Email' && '✉️'}
                          {interaction.interactionType === 'Meeting' && '🤝'}
                          {interaction.interactionType === 'Note' && '📝'}
                          {' '}{interaction.interactionType}
                        </span>
                        {/* Date and time of the interaction */}
                        <span className="text-xs text-gray-400 font-medium">
                          {new Date(interaction.interactionDate).toLocaleString()}
                        </span>
                      </div>
                      {/* The interaction notes/description */}
                      <p className="text-sm text-gray-700 leading-relaxed">{interaction.notes}</p>
                      {/* Follow-up date (only shown if one was set) */}
                      {interaction.followUpDate && (
                        <p className="flex items-center gap-1 text-xs text-amber-600 mt-2 font-medium">
                          {/* Clock icon */}
                          <svg className="w-3.5 h-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}><path strokeLinecap="round" strokeLinejoin="round" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z" /></svg>
                          Follow-up: {new Date(interaction.followUpDate).toLocaleString()}
                        </p>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          )}
        </div>
      </div>
    </div>
  );
}

// A small helper component that displays one field of lead information (label + value + icon)
function DetailField({ label, value, icon }) {
  return (
    <div className="flex items-start gap-3">
      {/* Show a small icon in an indigo box if an icon path was provided */}
      {icon && (
        <div className="flex-shrink-0 w-8 h-8 rounded-lg bg-indigo-50 flex items-center justify-center mt-0.5">
          <svg className="w-4 h-4 text-indigo-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
            <path strokeLinecap="round" strokeLinejoin="round" d={icon} />
          </svg>
        </div>
      )}
      {/* The label (e.g., "Email") and value (e.g., "john@example.com") */}
      <div>
        <dt className="text-xs font-semibold text-gray-400 uppercase tracking-wider">{label}</dt>
        <dd className="mt-0.5 text-sm font-medium text-gray-900">{value}</dd>
      </div>
    </div>
  );
}

/*
 * FILE SUMMARY:
 * This is the Lead Detail page — it shows all the information about a single lead, including
 * contact details, status, source, priority, and the assigned sales rep. It lets users change
 * the lead's status through valid transitions (e.g., New → Contacted → Qualified) and convert
 * qualified leads (only Managers/Admins can do this). It also displays a timeline of all
 * interactions (calls, emails, meetings, notes) and provides a form to add new ones.
 * Converted leads are locked — they cannot be edited or have new interactions added.
 */
