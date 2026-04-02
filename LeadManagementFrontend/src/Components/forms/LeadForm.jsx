import { useState } from 'react';
import { LEAD_SOURCES, LEAD_PRIORITIES } from '../../utils/constants';
import { validateLeadForm } from '../../utils/validation';
import ErrorAlert from '../ui/ErrorAlert';

export default function LeadForm({ initialValues = {}, onSubmit, loading, disabled }) {
  const [form, setForm] = useState({
    name: initialValues.name || '',
    email: initialValues.email || '',
    phone: initialValues.phone || '',
    company: initialValues.company || '',
    position: initialValues.position || '',
    source: initialValues.source || '',
    priority: initialValues.priority || '',
  });
  const [errors, setErrors] = useState({});

  const handleChange = (e) => {
    const { name, value } = e.target;
    setForm((prev) => ({ ...prev, [name]: value }));
    if (errors[name]) {
      setErrors((prev) => ({ ...prev, [name]: undefined }));
    }
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    const validationErrors = validateLeadForm(form);
    if (Object.keys(validationErrors).length > 0) {
      setErrors(validationErrors);
      return;
    }
    onSubmit(form);
  };

  const fieldClass = (name) =>
    `mt-1 block w-full rounded-xl border ${
      errors[name] ? 'border-red-300 ring-2 ring-red-100' : 'border-gray-200 bg-gray-50'
    } px-3 py-2.5 shadow-sm focus:border-indigo-400 focus:ring-2 focus:ring-indigo-400/20 focus:bg-white sm:text-sm disabled:bg-gray-100 disabled:cursor-not-allowed`;

  return (
    <form onSubmit={handleSubmit} className="space-y-6">
      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        <div>
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Name *</label>
          <input name="name" value={form.name} onChange={handleChange} disabled={disabled} className={fieldClass('name')} placeholder="John Doe" />
          {errors.name && <p className="mt-1 text-sm text-red-500 font-medium">{errors.name}</p>}
        </div>
        <div>
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Email *</label>
          <input name="email" type="email" value={form.email} onChange={handleChange} disabled={disabled} className={fieldClass('email')} placeholder="john@company.com" />
          {errors.email && <p className="mt-1 text-sm text-red-500 font-medium">{errors.email}</p>}
        </div>
        <div>
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Phone</label>
          <input name="phone" value={form.phone} onChange={handleChange} disabled={disabled} className={fieldClass('phone')} placeholder="+1 (555) 000-0000" />
          {errors.phone && <p className="mt-1 text-sm text-red-500 font-medium">{errors.phone}</p>}
        </div>
        <div>
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Company</label>
          <input name="company" value={form.company} onChange={handleChange} disabled={disabled} className={fieldClass('company')} placeholder="Acme Inc." />
        </div>
        <div>
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Position</label>
          <input name="position" value={form.position} onChange={handleChange} disabled={disabled} className={fieldClass('position')} placeholder="CTO" />
        </div>
        <div>
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Source *</label>
          <select name="source" value={form.source} onChange={handleChange} disabled={disabled} className={fieldClass('source')}>
            <option value="">Select source</option>
            {LEAD_SOURCES.map((s) => <option key={s} value={s}>{s}</option>)}
          </select>
          {errors.source && <p className="mt-1 text-sm text-red-500 font-medium">{errors.source}</p>}
        </div>
        <div>
          <label className="block text-xs font-semibold text-gray-500 uppercase tracking-wider">Priority *</label>
          <select name="priority" value={form.priority} onChange={handleChange} disabled={disabled} className={fieldClass('priority')}>
            <option value="">Select priority</option>
            {LEAD_PRIORITIES.map((p) => <option key={p} value={p}>{p}</option>)}
          </select>
          {errors.priority && <p className="mt-1 text-sm text-red-500 font-medium">{errors.priority}</p>}
        </div>
      </div>
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
