// Import useState (store changing data) and useEffect (run code when the page loads) from React
import { useState, useEffect } from 'react';
// Import useParams (get the lead ID from the URL) and useNavigate (redirect to another page)
import { useParams, useNavigate } from 'react-router-dom';
// Import the lead service — this talks to the backend API to fetch and update lead data
import { leadService } from '../services/leadService';
// Import the reusable LeadForm component that has all the input fields
import LeadForm from '../components/forms/LeadForm';
// Import the loading spinner shown while data is being fetched
import Spinner from '../components/ui/Spinner';
// Import the error alert component that shows a red error message box
import ErrorAlert from '../components/ui/ErrorAlert';

// This is the page where users can edit an existing lead's information
export default function EditLeadPage() {
  // Get the lead's ID from the URL (e.g., /leads/5/edit → id = 5)
  const { id } = useParams();
  // useNavigate gives us a function to redirect the user to another page
  const navigate = useNavigate();
  // Store the lead data fetched from the API
  const [lead, setLead] = useState(null);
  // Track whether we're still loading the lead data from the server
  const [loading, setLoading] = useState(true);
  // Track whether the form is currently being saved
  const [saving, setSaving] = useState(false);
  // Track any error message that should be shown to the user
  const [error, setError] = useState(null);

  // This runs once when the page loads — it fetches the lead's current data from the API
  useEffect(() => {
    const fetchLead = async () => {
      try {
        // Ask the backend API for the lead with this ID
        const response = await leadService.getById(id);
        // Store the lead data so we can show it in the form
        setLead(response.data);
      } catch (err) {
        // If fetching fails, show an error message
        setError(err.response?.data?.message || 'Failed to load lead');
      } finally {
        // Whether it succeeded or failed, stop showing the loading spinner
        setLoading(false);
      }
    };
    fetchLead();
  }, [id]);

  // This function runs when the user submits the edited form
  const handleSubmit = async (formData) => {
    // Show the saving state while we wait for the server
    setSaving(true);
    // Clear any previous error messages
    setError(null);
    try {
      // Send the updated data to the backend API
      await leadService.update(id, formData);
      // If successful, redirect to the lead's detail page
      navigate(`/leads/${id}`);
    } catch (err) {
      // If something went wrong, show an error message
      setError(err.response?.data?.message || 'Failed to update lead');
    } finally {
      // Whether it succeeded or failed, stop showing the saving state
      setSaving(false);
    }
  };

  // While loading, show a spinner instead of the form
  if (loading) return <Spinner size="lg" />;
  // If we couldn't find the lead, show an error message
  if (!lead) return <ErrorAlert message={error || 'Lead not found'} />;

  // Check if this lead has been converted — converted leads cannot be edited
  const isConverted = lead.status === 'Converted';

  return (
    <div>
      {/* Page title — shows "(Read-only — Converted)" if the lead is converted */}
      <h1 className="text-3xl font-extrabold text-gradient mb-6">
        Edit Lead {isConverted && <span className="text-sm text-gray-400 font-normal ml-2">(Read-only — Converted)</span>}
      </h1>
      {/* White card container for the form */}
      <div className="bg-white/80 backdrop-blur-sm shadow-sm rounded-2xl border border-gray-100 p-6">
        {/* Show an error alert if something went wrong — user can dismiss it */}
        <ErrorAlert message={error} onDismiss={() => setError(null)} />
        {/* The lead form — pre-filled with existing data, disabled if the lead is converted */}
        <LeadForm
          initialValues={lead}
          onSubmit={handleSubmit}
          loading={saving}
          disabled={isConverted}
        />
      </div>
    </div>
  );
}

/*
 * FILE SUMMARY: EditLeadPage.jsx
 *
 * This page lets users edit an existing lead's information. It first fetches the lead's
 * current data from the backend API, then shows the LeadForm pre-filled with that data.
 * If the lead's status is "Converted," the form becomes read-only because converted leads
 * should not be modified. When the user saves changes, the updated data is sent to the API.
 * This page is important for keeping lead information accurate as sales reps learn more about prospects.
 */
