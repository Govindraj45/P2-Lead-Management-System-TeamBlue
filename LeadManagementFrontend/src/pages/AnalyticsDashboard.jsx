// Import React hooks: useState stores data, useEffect runs code when the page loads
import { useState, useEffect } from 'react';
// Import chart components from the "recharts" library — these draw the pie charts and bar charts
import { PieChart, Pie, BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, Legend, Cell, ResponsiveContainer } from 'recharts';
// Import our analytics service — it talks to the backend API to fetch report data
import { analyticsService } from '../services/analyticsService';
// Import reusable UI components for showing a loading spinner and error messages
import Spinner from '../components/ui/Spinner';
import ErrorAlert from '../components/ui/ErrorAlert';

// A list of colors used for the chart sections (indigo, green, amber, red, purple, cyan, orange)
const COLORS = ['#6366f1', '#10b981', '#f59e0b', '#ef4444', '#8b5cf6', '#06b6d4', '#f97316'];

// This is the main Analytics Dashboard component — it shows charts and stats about leads
export default function AnalyticsDashboard() {
  // State variables to hold the data we get from the backend API
  const [sourceData, setSourceData] = useState([]);          // Leads grouped by where they came from (Website, Referral, etc.)
  const [statusData, setStatusData] = useState([]);          // Leads grouped by their current status (New, Contacted, etc.)
  const [conversionRate, setConversionRate] = useState(null); // The percentage of leads that were successfully converted
  const [salesRepData, setSalesRepData] = useState([]);      // How many leads each sales rep has and how many they converted
  const [loading, setLoading] = useState(true);              // Whether data is still being fetched
  const [error, setError] = useState(null);                  // Stores any error message if something goes wrong

  // useEffect runs once when the page first loads — it fetches all analytics data from the backend
  useEffect(() => {
    const fetchAll = async () => {
      setLoading(true);
      setError(null);
      try {
        // Fetch all 4 types of analytics data at the same time (in parallel) for speed
        const [sourceRes, statusRes, conversionRes, repRes] = await Promise.all([
          analyticsService.getBySource(),
          analyticsService.getByStatus(),
          analyticsService.getConversionRate(),
          analyticsService.getBySalesRep(),
        ]);
        // Store each API response in its matching state variable
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

  // If data is still loading, show a spinner instead of the dashboard
  if (loading) return <Spinner size="lg" />;

  // Everything below is the visible page content (JSX)
  return (
    <div>
      {/* Page title and subtitle */}
      <div className="mb-8">
        <h1 className="text-3xl font-extrabold text-gradient">Lead Analytics</h1>
        <p className="text-sm text-gray-500 mt-1">Insights into your sales pipeline performance</p>
      </div>

      {/* Show an error banner if something went wrong */}
      <ErrorAlert message={error} onDismiss={() => setError(null)} />

      {/* Stat Cards — show conversion rate, converted leads count, and total leads count */}
      {conversionRate !== null && (
        <div className="grid grid-cols-1 sm:grid-cols-3 gap-4 mb-8">
          {/* Conversion Rate card — purple gradient background */}
          <div className="relative overflow-hidden rounded-2xl p-6 text-white shadow-lg shadow-indigo-500/20"
            style={{ background: 'linear-gradient(135deg, #6366f1, #8b5cf6)' }}>
            {/* Decorative circle shape in the corner */}
            <div className="absolute top-0 right-0 w-32 h-32 bg-white/10 rounded-full -translate-y-1/2 translate-x-1/2" />
            <p className="text-sm font-medium text-indigo-100">Conversion Rate</p>
            {/* Display the conversion rate percentage — handle both object and plain number formats */}
            <p className="mt-2 text-4xl font-extrabold">
              {typeof conversionRate === 'object' ? conversionRate.conversionRate : conversionRate}%
            </p>
          </div>
          {/* If the API returned an object with extra details, also show Converted & Total Lead cards */}
          {typeof conversionRate === 'object' && (
            <>
              {/* Converted Leads card — green gradient background */}
              <div className="relative overflow-hidden rounded-2xl p-6 text-white shadow-lg shadow-emerald-500/20"
                style={{ background: 'linear-gradient(135deg, #10b981, #059669)' }}>
                <div className="absolute top-0 right-0 w-32 h-32 bg-white/10 rounded-full -translate-y-1/2 translate-x-1/2" />
                <p className="text-sm font-medium text-emerald-100">Converted Leads</p>
                <p className="mt-2 text-4xl font-extrabold">{conversionRate.convertedLeads}</p>
              </div>
              {/* Total Leads card — amber/gold gradient background */}
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

      {/* Charts section — arranged in a 2-column grid on large screens */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">

        {/* Pie Chart — shows leads broken down by source (Website, Referral, ColdCall, etc.) */}
        <div className="bg-white/80 backdrop-blur-sm shadow-sm rounded-2xl border border-gray-100 overflow-hidden">
          {/* Dark indigo header bar */}
          <div className="px-6 py-4 border-b border-gray-100" style={{ background: 'linear-gradient(135deg, #1e1b4b, #312e81)' }}>
            <h2 className="text-sm font-semibold text-indigo-200 uppercase tracking-wider">Leads by Source</h2>
          </div>
          <div className="p-6">
            {sourceData.length === 0 ? (
              <p className="text-sm text-gray-500 text-center py-8">No data available.</p>
            ) : (
              // ResponsiveContainer makes the chart resize automatically to fit its parent
              <ResponsiveContainer width="100%" height={300}>
                <PieChart>
                  {/* The Pie draws a donut chart (innerRadius creates the hole in the middle) */}
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
                    {/* Each slice gets a different color from our COLORS array */}
                    {sourceData.map((_, index) => (
                      <Cell key={index} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  {/* Tooltip shows details when you hover over a pie slice */}
                  <Tooltip
                    contentStyle={{ borderRadius: '12px', border: 'none', boxShadow: '0 4px 20px rgba(0,0,0,0.1)' }}
                  />
                  {/* Legend shows which color means which source */}
                  <Legend />
                </PieChart>
              </ResponsiveContainer>
            )}
          </div>
        </div>

        {/* Bar Chart — shows leads broken down by status (New, Contacted, Qualified, etc.) */}
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
                  {/* CartesianGrid draws the light gray grid lines behind the bars */}
                  <CartesianGrid strokeDasharray="3 3" stroke="#e5e7eb" />
                  {/* XAxis shows the status names along the bottom */}
                  <XAxis dataKey="status" tick={{ fontSize: 12 }} />
                  {/* YAxis shows the count numbers on the left side */}
                  <YAxis allowDecimals={false} tick={{ fontSize: 12 }} />
                  <Tooltip
                    contentStyle={{ borderRadius: '12px', border: 'none', boxShadow: '0 4px 20px rgba(0,0,0,0.1)' }}
                  />
                  {/* Each bar uses a gradient fill from indigo to purple */}
                  <Bar dataKey="count" fill="url(#barGradient)" radius={[8, 8, 0, 0]} />
                  {/* Define the gradient color that the bars use */}
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

        {/* Sales Rep Performance — a wide bar chart spanning both columns, showing assigned vs converted per rep */}
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
                  {/* Each sales rep is shown along the bottom axis */}
                  <XAxis dataKey="salesRepId" tick={{ fontSize: 12 }} />
                  <YAxis allowDecimals={false} tick={{ fontSize: 12 }} />
                  <Tooltip
                    contentStyle={{ borderRadius: '12px', border: 'none', boxShadow: '0 4px 20px rgba(0,0,0,0.1)' }}
                  />
                  <Legend />
                  {/* Indigo bars show how many leads are assigned to each rep */}
                  <Bar dataKey="assignedCount" fill="url(#assignedGradient)" name="Assigned" radius={[8, 8, 0, 0]} />
                  {/* Green bars show how many leads each rep successfully converted */}
                  <Bar dataKey="convertedCount" fill="url(#convertedGradient)" name="Converted" radius={[8, 8, 0, 0]} />
                  {/* Gradient definitions for the two bar colors */}
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

/*
 * FILE SUMMARY:
 * This is the Analytics Dashboard page — it shows managers and admins a visual overview
 * of the sales pipeline using charts and stat cards. It fetches four types of data from
 * the backend: leads by source (pie chart), leads by status (bar chart), the overall
 * conversion rate (stat cards), and each sales rep's performance (grouped bar chart).
 * All data is loaded in parallel when the page opens, and a loading spinner is shown
 * until everything is ready. This page helps the team understand how leads are flowing
 * through the system and which reps are performing best.
 */
