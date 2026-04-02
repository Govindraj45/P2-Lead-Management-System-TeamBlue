import { useState, useEffect } from 'react';
import { PieChart, Pie, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, Cell, ResponsiveContainer } from 'recharts';
import { analyticsService } from '../services/analyticsService';
import Spinner from '../components/ui/Spinner';
import ErrorAlert from '../components/ui/ErrorAlert';

const COLORS = ['#6366f1', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#06b6d4', '#f97316'];

export default function AnalyticsDashboard() {
  const [sourceData, setSourceData] = useState([]);
  const [statusData, setStatusData] = useState([]);
  const [conversionRate, setConversionRate] = useState(null);
  const [salesRepData, setSalesRepData] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState(null);

  useEffect(() => {
    const fetchAll = async () => {
      setLoading(true);
      setError(null);
      try {
        const [sourceRes, statusRes, conversionRes, repRes] = await Promise.all([
          analyticsService.getBySource(),
          analyticsService.getByStatus(),
          analyticsService.getConversionRate(),
          analyticsService.getBySalesRep(),
        ]);
        setSourceData(sourceRes.data);
        setStatusData(statusRes.data);
        setConversionRate(conversionRes.data);
        setSalesRepData(repRes.data);
      } catch (err) {
        setError(err.response?.data?.message || 'Failed to load analytics');
      } finally {
        setLoading(false);
      }
    };
    fetchAll();
  }, []);

  if (loading) return <Spinner size="lg" />;

  return (
    <div>
      <div className="mb-8">
        <h1 className="text-3xl font-extrabold text-gradient">Lead Analytics</h1>
        <p className="text-sm text-gray-500 mt-1">Insights into your sales pipeline performance</p>
      </div>

      <ErrorAlert message={error} onDismiss={() => setError(null)} />

      {/* Stat Cards */}
      {conversionRate !== null && (
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-8">
          <div className="relative overflow-hidden rounded-2xl p-6 text-white shadow-lg shadow-indigo-500/20"
            style={{ background: 'linear-gradient(135deg, #6366f1, #8b5cf6)' }}>
            <div className="absolute top-0 right-0 w-32 h-32 bg-white/10 rounded-full -translate-y-1/2 translate-x-1/2" />
            <p className="text-sm font-medium text-indigo-100">Conversion Rate</p>
            <p className="mt-2 text-4xl font-extrabold">
              {typeof conversionRate === 'object' ? conversionRate.conversionRate : conversionRate}%
            </p>
          </div>
          {typeof conversionRate === 'object' && (
            <>
              <div className="relative overflow-hidden rounded-2xl p-6 text-white shadow-lg shadow-emerald-500/20"
                style={{ background: 'linear-gradient(135deg, #10b981, #059669)' }}>
                <div className="absolute top-0 right-0 w-32 h-32 bg-white/10 rounded-full -translate-y-1/2 translate-x-1/2" />
                <p className="text-sm font-medium text-emerald-100">Converted Leads</p>
                <p className="mt-2 text-4xl font-extrabold">{conversionRate.convertedLeads}</p>
              </div>
              <div className="relative overflow-hidden rounded-2xl p-6 text-white shadow-lg shadow-amber-500/20"
                style={{ background: 'linear-gradient(135deg, #f59e0b, #d97706)' }}>
                <div className="absolute top-0 right-0 w-32 h-32 bg-white/10 rounded-full -translate-y-1/2 translate-x-1/2" />
                <p className="text-sm font-medium text-amber-100">Total Leads</p>
                <p className="mt-2 text-4xl font-extrabold">{conversionRate.totalLeads}</p>
              </div>
            </>
          )}
        </div>
      )}

      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Pie Chart — By Source */}
        <div className="bg-white/80 backdrop-blur-sm shadow-sm rounded-2xl border border-gray-100 overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-100" style={{ background: 'linear-gradient(135deg, #1e1b4b, #312e81)' }}>
            <h2 className="text-sm font-semibold text-indigo-200 uppercase tracking-wider">Leads by Source</h2>
          </div>
          <div className="p-6">
            {sourceData.length === 0 ? (
              <p className="text-sm text-gray-500 text-center py-8">No data available.</p>
            ) : (
              <ResponsiveContainer width="100%" height={300}>
                <PieChart>
                  <Pie
                    data={sourceData}
                    dataKey="count"
                    nameKey="source"
                    cx="50%"
                    cy="50%"
                    outerRadius={100}
                    innerRadius={60}
                    strokeWidth={2}
                    stroke="#fff"
                    label={({ source, count }) => `${source}: ${count}`}
                  >
                    {sourceData.map((_, index) => (
                      <Cell key={index} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip
                    contentStyle={{ borderRadius: '12px', border: 'none', boxShadow: '0 4px 20px rgba(0,0,0,0.1)' }}
                  />
                  <Legend />
                </PieChart>
              </ResponsiveContainer>
            )}
          </div>
        </div>

        {/* Bar Chart — By Status */}
        <div className="bg-white/80 backdrop-blur-sm shadow-sm rounded-2xl border border-gray-100 overflow-hidden">
          <div className="px-6 py-4 border-b border-gray-100" style={{ background: 'linear-gradient(135deg, #1e1b4b, #312e81)' }}>
            <h2 className="text-sm font-semibold text-indigo-200 uppercase tracking-wider">Leads by Status</h2>
          </div>
          <div className="p-6">
            {statusData.length === 0 ? (
              <p className="text-sm text-gray-500 text-center py-8">No data available.</p>
            ) : (
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={statusData}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                  <XAxis dataKey="status" tick={{ fontSize: 12 }} />
                  <YAxis allowDecimals={false} tick={{ fontSize: 12 }} />
                  <Tooltip
                    contentStyle={{ borderRadius: '12px', border: 'none', boxShadow: '0 4px 20px rgba(0,0,0,0.1)' }}
                  />
                  <Bar dataKey="count" fill="url(#barGradient)" radius={[8, 8, 0, 0]} />
                  <defs>
                    <linearGradient id="barGradient" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stopColor="#6366f1" />
                      <stop offset="100%" stopColor="#8b5cf6" />
                    </linearGradient>
                  </defs>
                </BarChart>
              </ResponsiveContainer>
            )}
          </div>
        </div>

        {/* Sales Rep Performance */}
        <div className="bg-white/80 backdrop-blur-sm shadow-sm rounded-2xl border border-gray-100 overflow-hidden lg:col-span-2">
          <div className="px-6 py-4 border-b border-gray-100" style={{ background: 'linear-gradient(135deg, #1e1b4b, #312e81)' }}>
            <h2 className="text-sm font-semibold text-indigo-200 uppercase tracking-wider">Sales Rep Performance</h2>
          </div>
          <div className="p-6">
            {salesRepData.length === 0 ? (
              <p className="text-sm text-gray-500 text-center py-8">No data available.</p>
            ) : (
              <ResponsiveContainer width="100%" height={300}>
                <BarChart data={salesRepData}>
                  <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                  <XAxis dataKey="salesRepId" tick={{ fontSize: 12 }} />
                  <YAxis allowDecimals={false} tick={{ fontSize: 12 }} />
                  <Tooltip
                    contentStyle={{ borderRadius: '12px', border: 'none', boxShadow: '0 4px 20px rgba(0,0,0,0.1)' }}
                  />
                  <Legend />
                  <Bar dataKey="assignedCount" fill="url(#assignedGradient)" name="Assigned" radius={[8, 8, 0, 0]} />
                  <Bar dataKey="convertedCount" fill="url(#convertedGradient)" name="Converted" radius={[8, 8, 0, 0]} />
                  <defs>
                    <linearGradient id="assignedGradient" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stopColor="#6366f1" />
                      <stop offset="100%" stopColor="#818cf8" />
                    </linearGradient>
                    <linearGradient id="convertedGradient" x1="0" y1="0" x2="0" y2="1">
                      <stop offset="0%" stopColor="#10b981" />
                      <stop offset="100%" stopColor="#34d399" />
                    </linearGradient>
                  </defs>
                </BarChart>
              </ResponsiveContainer>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
