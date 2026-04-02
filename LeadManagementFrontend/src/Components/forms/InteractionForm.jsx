import { useState } from 'react';

const INTERACTION_TYPES = ['Call', 'Email', 'Meeting', 'Note'];

export default function InteractionForm({ onSubmit, loading, disabled }) {
  const [form, setForm] = useState({
    type: '',
    date: '',
    details: '',
    followUpDate: '',
  });
  const [errors, setErrors] = useState({});

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
    if (errors[name]) setErrors((prev) => ({ ...prev, [name]: undefined }));
  };

  const validate = () => {
    const errs = {};
    if (!form.type) errs.type = 'Type is required';
    if (!form.date) errs.date = 'Date is required';
    else if (new Date(form.date) > new Date()) errs.date = 'Cannot be a future date';
    if (!form.details?.trim()) errs.details = 'Details are required';
    if (form.followUpDate && form.date && new Date(form.followUpDate) <= new Date(form.date)) {
      errs.followUpDate = 'Follow-up must be after interaction date';
    }
    return errs;
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    const validationErrors = validate();
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }
    onSubmit({
      interactionType: form.type,
      interactionDate: form.date,
      notes: form.details,
      followUpDate: form.followUpDate || null,
    });
    setForm({ type: '', date: '', details: '', followUpDate: '' });
  };

  const fieldClass = (name) =>
    `mt-1 block w-full rounded-xl border ${errors[name] ? 'border-red-300 ring-2 ring-red-100' : 'border-gray-200 bg-gray-50'} px-3 py-2.5 shadow-sm focus:border-indigo-400 focus:ring-2 focus:ring-indigo-400/20 focus:bg-white sm:text-sm disabled:bg-gray-100`;

  return (
    <form onSubmit={handleSubmit} className="space-y-4">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
        <div>
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Type *</label>
          <select name="type" value={form.type} onChange={handleChange} disabled={disabled} className={fieldClass('type')}>
            <option value="">Select type</option>
            {INTERACTION_TYPES.map((t) => <option key={t} value={t}>{t}</option>)}
          </select>
          {errors.type && <p className="mt-1 text-sm text-red-500 font-medium">{errors.type}</p>}
        </div>
        <div>
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Date *</label>
          <input type="datetime-local" name="date" value={form.date} onChange={handleChange} disabled={disabled} className={fieldClass('date')} />
          {errors.date && <p className="mt-1 text-sm text-red-500 font-medium">{errors.date}</p>}
        </div>
        <div className="md:col-span-2">
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Details *</label>
          <textarea name="details" rows={3} value={form.details} onChange={handleChange} disabled={disabled} className={fieldClass('details')} placeholder="Describe the interaction..." />
          {errors.details && <p className="mt-1 text-sm text-red-500 font-medium">{errors.details}</p>}
        </div>
        <div>
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Follow-up Date</label>
          <input type="datetime-local" name="followUpDate" value={form.followUpDate} onChange={handleChange} disabled={disabled} className={fieldClass('followUpDate')} />
          {errors.followUpDate && <p className="mt-1 text-sm text-red-500 font-medium">{errors.followUpDate}</p>}
        </div>
      </div>
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
