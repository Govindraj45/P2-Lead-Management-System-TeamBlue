// Import useState hook from React — lets us store and update data inside the component
import { useState } from 'react';
// Import the list of allowed lead sources (like "Website", "Referral") and priorities (like "High", "Low")
import { LEAD_SOURCES, LEAD_PRIORITIES } from '../../utils/constants';
// Import a helper function that checks if the lead form data is valid before submitting
import { validateLeadForm } from '../../utils/validation';
// Import a component that shows error messages in a styled red box
import ErrorAlert from '../ui/ErrorAlert';

// This is the main LeadForm component — used for both creating and editing leads
// It receives: initialValues (existing data for editing), onSubmit (what to do when form is submitted),
// loading (whether saving is in progress), and disabled (whether the form is read-only)
export default function LeadForm({ initialValues = {}, onSubmit, loading, disabled }) {
  // Create a "form" object that stores all the values the user types into the fields
  // If initialValues are provided (edit mode), use those; otherwise start with empty strings
  const [form, setForm] = useState({
    name: initialValues.name || '',
    email: initialValues.email || '',
    phone: initialValues.phone || '',
    company: initialValues.company || '',
    position: initialValues.position || '',
    source: initialValues.source || '',
    priority: initialValues.priority || '',
  });
  // Create an "errors" object to store validation error messages for each field
  const [errors, setErrors] = useState({});

  // This function runs every time the user types in any field — it updates that field's value
  const handleChange = (e) => {
    // Get the field's name (like "email") and its new value from the event
    const { name, value } = e.target;
    // Update just the one field that changed, keeping all other fields the same
    setForm((prev) => ({ ...prev, [name]: value }));
    // If this field had an error before, clear it now since the user is fixing it
    if (errors[name]) {
      setErrors((prev) => ({ ...prev, [name]: undefined }));
    }
  };

  // This function runs when the user clicks "Save Lead"
  const handleSubmit = (e) => {
    // Prevent the browser from refreshing the page (default form behavior)
    e.preventDefault();
    // Check all fields for errors (like missing required fields or invalid email)
    const validationErrors = validateLeadForm(form);
    // If there are any errors, show them and stop — don't submit the form
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }
    // If everything is valid, call the parent's onSubmit function with the form data
    onSubmit(form);
  };

  // This helper function returns the right CSS classes for an input field
  // It shows a red border if there's an error, or a normal gray border otherwise
  const fieldClass = (name) =>
    `mt-1 block w-full rounded-xl border ${
      errors[name] ? 'border-red-300 ring-2 ring-red-100' : 'border-gray-200 bg-gray-50'
    } px-3 py-2.5 shadow-sm focus:border-indigo-400 focus:ring-2 focus:ring-indigo-400/20 focus:bg-white sm:text-sm disabled:bg-gray-100 disabled:cursor-not-allowed`;

  return (
    // The form element — when submitted, it calls handleSubmit
    <form onSubmit={handleSubmit} className="space-y-6">
      {/* A two-column grid layout that arranges form fields side by side on larger screens */}
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Name field — required */}
        <div>
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Name *</label>
          <input name="name" value={form.name} onChange={handleChange} disabled={disabled} className={fieldClass('name')} placeholder="John Doe" />
          {/* Show error message below the field if validation failed */}
          {errors.name && <p className="mt-1 text-sm text-red-500 font-medium">{errors.name}</p>}
        </div>
        {/* Email field — required */}
        <div>
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Email *</label>
          <input name="email" type="email" value={form.email} onChange={handleChange} disabled={disabled} className={fieldClass('email')} placeholder="john@company.com" />
          {errors.email && <p className="mt-1 text-sm text-red-500 font-medium">{errors.email}</p>}
        </div>
        {/* Phone field — optional */}
        <div>
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Phone</label>
          <input name="phone" value={form.phone} onChange={handleChange} disabled={disabled} className={fieldClass('phone')} placeholder="+1 (555) 000-0000" />
          {errors.phone && <p className="mt-1 text-sm text-red-500 font-medium">{errors.phone}</p>}
        </div>
        {/* Company field — optional */}
        <div>
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Company</label>
          <input name="company" value={form.company} onChange={handleChange} disabled={disabled} className={fieldClass('company')} placeholder="Acme Inc." />
        </div>
        {/* Position/job title field — optional */}
        <div>
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Position</label>
          <input name="position" value={form.position} onChange={handleChange} disabled={disabled} className={fieldClass('position')} placeholder="CTO" />
        </div>
        {/* Source dropdown — where the lead came from (required) */}
        <div>
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Source *</label>
          <select name="source" value={form.source} onChange={handleChange} disabled={disabled} className={fieldClass('source')}>
            <option value="">Select source</option>
            {/* Loop through all possible sources and create a dropdown option for each */}
            {LEAD_SOURCES.map((s) => <option key={s} value={s}>{s}</option>)}
          </select>
          {errors.source && <p className="mt-1 text-sm text-red-500 font-medium">{errors.source}</p>}
        </div>
        {/* Priority dropdown — how important this lead is (required) */}
        <div>
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Priority *</label>
          <select name="priority" value={form.priority} onChange={handleChange} disabled={disabled} className={fieldClass('priority')}>
            <option value="">Select priority</option>
            {/* Loop through all possible priorities and create a dropdown option for each */}
            {LEAD_PRIORITIES.map((p) => <option key={p} value={p}>{p}</option>)}
          </select>
          {errors.priority && <p className="mt-1 text-sm text-red-500 font-medium">{errors.priority}</p>}
        </div>
      </div>
      {/* Submit button section — shows "Saving..." while the form is being sent to the server */}
      <div className="flex justify-end">
        <button
          type="submit"
          disabled={loading || disabled}
          className="inline-flex items-center gap-2 px-6 py-2.5 rounded-xl text-sm font-semibold text-white shadow-lg shadow-indigo-500/25 hover:shadow-indigo-500/40 hover:scale-[1.02] active:scale-[0.98] disabled:opacity-50 disabled:cursor-not-allowed"
          style={{ background: 'linear-gradient(135deg, #6366f1, #8b5cf6)' }}
        >
          {loading ? 'Saving...' : 'Save Lead'}
        </button>
      </div>
    </form>
  );
}

/*
 * FILE SUMMARY: LeadForm.jsx
 *
 * This is a reusable form component for creating or editing a lead (potential customer) in the system.
 * It contains input fields for the lead's name, email, phone, company, position, source, and priority.
 * The form validates all required fields before submitting and shows error messages next to any
 * field that has a problem. It can also be set to "disabled" mode so users can view but not change
 * the data — this is used when a lead has been converted and should be read-only.
 */
