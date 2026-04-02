import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { leadService } from '../services/leadService';
import LeadForm from '../components/forms/LeadForm';
import ErrorAlert from '../components/ui/ErrorAlert';

export default function CreateLeadPage() {
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);

  const handleSubmit = async (formData) => {
    setLoading(true);
    setError(null);
    try {
      await leadService.create(formData);
      navigate('/leads');
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to create lead');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div>
      <h1 className="text-3xl font-extrabold text-gradient mb-6">Create New Lead</h1>
      <div className="bg-white/80 backdrop-blur-sm shadow-sm rounded-2xl border border-gray-100 p-6">
        <ErrorAlert message={error} onDismiss={() => setError(null)} />
        <LeadForm onSubmit={handleSubmit} loading={loading} />
      </div>
    </div>
  );
}
