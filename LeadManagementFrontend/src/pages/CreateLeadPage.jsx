// Import useState hook from React — lets us store data that can change (like loading state)
import { useState } from 'react';
// Import useNavigate — lets us send the user to a different page after they create a lead
import { useNavigate } from 'react-router-dom';
// Import the lead service — this talks to the backend API to save lead data
import { leadService } from '../services/leadService';
// Import the reusable LeadForm component that has all the input fields
import LeadForm from '../components/forms/LeadForm';
// Import the ErrorAlert component that shows a red error message box
import ErrorAlert from '../components/ui/ErrorAlert';

// This is the page where users can create a brand new lead
export default function CreateLeadPage() {
  // useNavigate gives us a function to redirect the user to another page
  const navigate = useNavigate();
  // Track whether the form is currently being saved (to show a loading spinner)
  const [loading, setLoading] = useState(false);
  // Track any error message that should be shown to the user
  const [error, setError] = useState(null);

  // This function runs when the user submits the lead form
  const handleSubmit = async (formData) => {
    // Show the loading state while we wait for the server
    setLoading(true);
    // Clear any previous error messages
    setError(null);
    try {
      // Send the new lead data to the backend API
      await leadService.create(formData);
      // If successful, redirect the user back to the leads list page
      navigate('/leads');
    } catch (err) {
      // If something went wrong, show an error message
      setError(err.response?.data?.message || 'Failed to create lead');
    } finally {
      // Whether it succeeded or failed, stop showing the loading state
      setLoading(false);
    }
  };

  return (
    <div>
      {/* Page title */}
      <h1 className="text-3xl font-extrabold text-gradient mb-6">Create New Lead</h1>
      {/* White card container for the form */}
      <div className="bg-white/80 backdrop-blur-sm shadow-sm rounded-2xl border border-gray-100 p-6">
        {/* Show an error alert if something went wrong — user can dismiss it */}
        <ErrorAlert message={error} onDismiss={() => setError(null)} />
        {/* The actual lead form with all the fields — passes the submit handler and loading state */}
        <LeadForm onSubmit={handleSubmit} loading={loading} />
      </div>
    </div>
  );
}

/*
 * FILE SUMMARY: CreateLeadPage.jsx
 *
 * This page lets users create a new lead (potential customer) in the system.
 * It shows the LeadForm component inside a styled card and handles the submission
 * by sending the data to the backend API. If the creation succeeds, the user is
 * redirected to the leads list. If it fails, an error message is displayed.
 * This is one of the main pages sales reps use daily to add new prospects.
 */
