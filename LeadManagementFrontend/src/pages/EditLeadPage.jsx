import { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { leadService } from '../services/leadService';
import LeadForm from '../components/forms/LeadForm';
import Spinner from '../components/ui/Spinner';
import ErrorAlert from '../components/ui/ErrorAlert';

export default function EditLeadPage() {
  const { id } = useParams();
  const navigate = useNavigate();
  const [lead, setLead] = useState(null);
  const [loading, setLoading] = useState(true);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchLead = async () => {
      try {
        const response = await leadService.getById(id);
        setLead(response.data);
      } catch (err) {
        setError(err.response?.data?.message || 'Failed to load lead');
      } finally {
        setLoading(false);
      }
    };
    fetchLead();
  }, [id]);

  const handleSubmit = async (formData) => {
    setSaving(true);
    setError(null);
    try {
      await leadService.update(id, formData);
      navigate(`/leads/${id}`);
    } catch (err) {
      setError(err.response?.data?.message || 'Failed to update lead');
    } finally {
      setSaving(false);
    }
  };

  if (loading) return <Spinner size="lg" />;
  if (!lead) return <ErrorAlert message={error || 'Lead not found'} />;

  const isConverted = lead.status === 'Converted';

  return (
    <div>
      <h1 className="text-3xl font-extrabold text-gradient mb-6">
        Edit Lead {isConverted && <span className="text-sm text-gray-400 font-normal ml-2">(Read-only — Converted)</span>}
      </h1>
      <div className="bg-white/80 backdrop-blur-sm shadow-sm rounded-2xl border border-gray-100 p-6">
        <ErrorAlert message={error} onDismiss={() => setError(null)} />
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
