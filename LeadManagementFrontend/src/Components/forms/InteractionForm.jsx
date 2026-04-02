// Import useState hook from React — lets us store and update data inside the component
import { useState } from 'react';

// Define the types of interactions a sales rep can log with a lead
const INTERACTION_TYPES = ['Call', 'Email', 'Meeting', 'Note'];

// This is the InteractionForm component — it lets users log a new interaction with a lead
// It receives: onSubmit (what to do with the data), loading (is it saving?), disabled (is it read-only?)
export default function InteractionForm({ onSubmit, loading, disabled }) {
  // Create a "form" object to store the values the user enters for the interaction
  const [form, setForm] = useState({
    type: '',
    date: '',
    details: '',
    followUpDate: '',
  });
  // Create an "errors" object to store validation error messages for each field
  const [errors, setErrors] = useState({});

  // This function runs every time the user changes a field — it updates that field's value
  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
    // Clear the error for this field when the user starts fixing it
    if (errors[name]) setErrors((prev) => ({ ...prev, [name]: undefined }));
  };

  // This function checks if all the form data is valid before allowing submission
  const validate = () => {
    const errs = {};
    // Type is required — the user must pick Call, Email, Meeting, or Note
    if (!form.type) errs.type = 'Type is required';
    // Date is required and must not be in the future (you can't log something that hasn't happened)
    if (!form.date) errs.date = 'Date is required';
    else if (new Date(form.date) > new Date()) errs.date = 'Cannot be a future date';
    // Details are required — the user must describe what happened in the interaction
    if (!form.details?.trim()) errs.details = 'Details are required';
    // If a follow-up date is set, it must be AFTER the interaction date (not before or same day)
    if (form.followUpDate && form.date && new Date(form.followUpDate) <= new Date(form.date)) {
      errs.followUpDate = 'Follow-up must be after interaction date';
    }
    return errs;
  };

  // This function runs when the user clicks "Add Interaction"
  const handleSubmit = (e) => {
    // Prevent the browser from refreshing the page (default form behavior)
    e.preventDefault();
    // Validate the form first — check for missing or invalid data
    const validationErrors = validate();
    // If there are errors, show them and don't submit the form
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }
    // Send the data to the parent component, renaming fields to match what the backend API expects
    onSubmit({
      interactionType: form.type,
      interactionDate: form.date,
      notes: form.details,
      followUpDate: form.followUpDate || null,
    });
    // Clear the form after successful submission so the user can add another interaction
    setForm({ type: '', date: '', details: '', followUpDate: '' });
  };

  // Helper function that returns CSS classes for a field — shows a red border if there's an error
  const fieldClass = (name) =>
    `mt-1 block w-full rounded-xl border ${errors[name] ? 'border-red-300 ring-2 ring-red-100' : 'border-gray-200 bg-gray-50'} px-3 py-2.5 shadow-sm focus:border-indigo-400 focus:ring-2 focus:ring-indigo-400/20 focus:bg-white sm:text-sm disabled:bg-gray-100`;

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      {/* A two-column grid layout for the form fields */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        {/* Interaction type dropdown — required */}
        <div>
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Type *</label>
          <select name="type" value={form.type} onChange={handleChange} disabled={disabled} className={fieldClass('type')}>
            <option value="">Select type</option>
            {/* Loop through each interaction type and create a dropdown option */}
            {INTERACTION_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
          </select>
          {errors.type && <p className="mt-1 text-sm text-red-500 font-medium">{errors.type}</p>}
        </div>
        {/* Interaction date picker — required, must not be a future date */}
        <div>
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Date *</label>
          <input type="datetime-local" name="date" value={form.date} onChange={handleChange} disabled={disabled} className={fieldClass('date')} />
          {errors.date && <p className="mt-1 text-sm text-red-500 font-medium">{errors.date}</p>}
        </div>
        {/* Details text area — spans both columns, required description of what happened */}
        <div className="md:col-span-2">
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Details *</label>
          <textarea name="details" rows={3} value={form.details} onChange={handleChange} disabled={disabled} className={fieldClass('details')} placeholder="Describe the interaction..." />
          {errors.details && <p className="mt-1 text-sm text-red-500 font-medium">{errors.details}</p>}
        </div>
        {/* Follow-up date picker — optional, but if set must be after the interaction date */}
        <div>
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Follow-up Date</label>
          <input type="datetime-local" name="followUpDate" value={form.followUpDate} onChange={handleChange} disabled={disabled} className={fieldClass('followUpDate')} />
          {errors.followUpDate && <p className="mt-1 text-sm text-red-500 font-medium">{errors.followUpDate}</p>}
        </div>
      </div>
      {/* Submit button — shows "Adding..." while the interaction is being saved */}
      <div className="flex justify-end">
        <button
          type="submit"
          disabled={loading || disabled}
          className="inline-flex items-center gap-2 px-5 py-2.5 rounded-xl text-sm font-semibold text-white shadow-lg shadow-indigo-500/25 hover:shadow-indigo-500/40 hover:scale-[1.02] active:scale-[0.98] disabled:opacity-50 disabled:cursor-not-allowed"
          style={{ background: 'linear-gradient(135deg, #6366f1, #8b5cf6)' }}
        >
          {loading ? 'Adding...' : 'Add Interaction'}
        </button>
      </div>
    </form>
  );
}

/*
 * FILE SUMMARY: InteractionForm.jsx
 *
 * This form component lets users log a new interaction (call, email, meeting, or note) with a lead.
 * It validates that the interaction date is not in the future, the follow-up date comes after the
 * interaction date, and all required fields are filled in. After successful submission, the form
 * clears itself so another interaction can be added right away. This component is used on the
 * lead detail page to track all communication history with a potential customer.
 */
