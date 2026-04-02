import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import Spinner from '../components/ui/Spinner';

export default function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const { login, loading, error } = useAuth();
  const navigate = useNavigate();

  const handleSubmit = async (e) => {
    e.preventDefault();
    try {
      await login(email, password);
      navigate('/leads');
    } catch {
      // error is set in context
    }
  };

  return (
    <div className="min-h-screen flex" style={{ background: 'linear-gradient(135deg, #f8fafc 0%, #eef2ff 50%, #f5f3ff 100%)' }}>

      {/* ===== LEFT — Branding panel ===== */}
      <div className="relative hidden lg:flex lg:w-[480px] xl:w-[520px] flex-col justify-between overflow-hidden"
        style={{ background: 'linear-gradient(135deg, #1e1b4b, #312e81, #3730a3)' }}>

        {/* Decorative orbs */}
        <div className="absolute -top-24 -left-24 w-96 h-96 rounded-full bg-indigo-500/10 blur-3xl" />
        <div className="absolute -bottom-32 -right-32 w-[420px] h-[420px] rounded-full bg-purple-500/10 blur-3xl" />
        <div className="absolute top-1/3 right-0 w-64 h-64 rounded-full border border-white/5" />
        <div className="absolute bottom-1/4 left-8 w-40 h-40 rounded-full border border-white/5" />

        {/* Top content */}
        <div className="relative z-10 px-10 pt-12">
          <div className="flex items-center gap-2.5 mb-16">
            <div className="w-9 h-9 rounded-lg bg-gradient-to-br from-indigo-400 to-purple-500 flex items-center justify-center shadow-lg shadow-indigo-500/30">
              <svg className="w-5 h-5 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M13 10V3L4 14h7v7l9-11h-7z" />
              </svg>
            </div>
            <span className="text-lg font-bold text-white tracking-tight">LeadCRM</span>
          </div>

          <h2 className="text-3xl xl:text-4xl font-bold text-white leading-tight">
            Manage your leads.<br />
            <span className="text-transparent bg-clip-text bg-gradient-to-r from-indigo-300 to-purple-300">
              Close more deals.
            </span>
          </h2>
          <p className="mt-4 text-indigo-200/60 text-sm leading-relaxed max-w-xs">
            Track every lead from first contact to conversion with analytics-powered insights.
          </p>
        </div>

        {/* Stats preview */}
        <div className="relative z-10 px-10 pb-12">
          <div className="grid grid-cols-3 gap-3">
            {[
              { label: 'Leads Tracked', value: '2,400+', icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2' },
              { label: 'Conversion Rate', value: '34%', icon: 'M13 7h8m0 0v8m0-8l-8 8-4-4-6 6' },
              { label: 'Active Reps', value: '18', icon: 'M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0z' },
            ].map((s) => (
              <div key={s.label} className="rounded-xl p-3 bg-white/5 border border-white/5">
                <svg className="w-4 h-4 text-indigo-300 mb-2" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                  <path strokeLinecap="round" strokeLinejoin="round" d={s.icon} />
                </svg>
                <p className="text-white font-bold text-lg leading-tight">{s.value}</p>
                <p className="text-indigo-300/50 text-[10px] mt-0.5">{s.label}</p>
              </div>
            ))}
          </div>

          <div className="mt-6 flex items-center gap-3">
            <div className="flex -space-x-2">
              {['bg-indigo-400', 'bg-purple-400', 'bg-pink-400', 'bg-amber-400'].map((c, i) => (
                <div key={i} className={`w-7 h-7 rounded-full ${c} border-2 border-indigo-900 flex items-center justify-center text-[9px] font-bold text-white`}>
                  {['A', 'M', 'S', 'R'][i]}
                </div>
              ))}
            </div>
            <p className="text-indigo-300/40 text-xs">Team members active today</p>
          </div>
        </div>
      </div>

      {/* ===== RIGHT — Login form ===== */}
      <div className="flex-1 flex items-center justify-center px-6 sm:px-12 lg:px-16 xl:px-24 py-12">
        <div className="w-full max-w-[420px] animate-fade-in">

          {/* Mobile logo */}
          <div className="flex items-center gap-2.5 mb-10 lg:hidden">
            <div className="w-9 h-9 rounded-lg bg-gradient-to-br from-indigo-500 to-purple-600 flex items-center justify-center shadow-lg shadow-indigo-500/20">
              <svg className="w-5 h-5 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
                <path strokeLinecap="round" strokeLinejoin="round" d="M13 10V3L4 14h7v7l9-11h-7z" />
              </svg>
            </div>
            <span className="text-lg font-bold text-gray-900 tracking-tight">LeadCRM</span>
          </div>

          <h1 className="text-2xl sm:text-3xl font-bold text-gray-900">Welcome back</h1>
          <p className="mt-2 text-sm text-gray-500 mb-8">Sign in to your account to continue</p>

          {/* Error */}
          {error && (
            <div className="mb-6 flex items-center gap-2.5 rounded-xl bg-red-50 border border-red-200 px-4 py-3">
              <svg className="h-4 w-4 text-red-500 flex-shrink-0" viewBox="0 0 20 20" fill="currentColor">
                <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
              </svg>
              <p className="text-sm text-red-700">{error}</p>
            </div>
          )}

          <form onSubmit={handleSubmit} className="space-y-5">
            {/* Email */}
            <div>
              <label htmlFor="email" className="block text-sm font-medium text-gray-700 mb-1.5">
                Email address
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none">
                  <svg className="h-4 w-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                    <path strokeLinecap="round" strokeLinejoin="round" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                  </svg>
                </div>
                <input
                  id="email"
                  type="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="block w-full rounded-xl border border-gray-200 bg-white pl-10 pr-4 py-3 text-gray-900 placeholder-gray-400 text-sm outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/20 shadow-sm"
                  placeholder="name@example.com"
                />
              </div>
            </div>

            {/* Password */}
            <div>
              <label htmlFor="password" className="block text-sm font-medium text-gray-700 mb-1.5">
                Password
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-3.5 flex items-center pointer-events-none">
                  <svg className="h-4 w-4 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                    <path strokeLinecap="round" strokeLinejoin="round" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                  </svg>
                </div>
                <input
                  id="password"
                  type={showPassword ? 'text' : 'password'}
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="block w-full rounded-xl border border-gray-200 bg-white pl-10 pr-11 py-3 text-gray-900 placeholder-gray-400 text-sm outline-none focus:border-indigo-500 focus:ring-2 focus:ring-indigo-500/20 shadow-sm"
                  placeholder="••••••••"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute inset-y-0 right-0 pr-3.5 flex items-center text-gray-400 hover:text-gray-600"
                >
                  {showPassword ? (
                    <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                      <path strokeLinecap="round" strokeLinejoin="round" d="M13.875 18.825A10.05 10.05 0 0112 19c-4.478 0-8.268-2.943-9.543-7a9.97 9.97 0 011.563-3.029m5.858.908a3 3 0 114.243 4.243M9.878 9.878l4.242 4.242M9.88 9.88l-3.29-3.29m7.532 7.532l3.29 3.29M3 3l3.59 3.59m0 0A9.953 9.953 0 0112 5c4.478 0 8.268 2.943 9.543 7a10.025 10.025 0 01-4.132 5.411m0 0L21 21" />
                    </svg>
                  ) : (
                    <svg className="h-4 w-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                      <path strokeLinecap="round" strokeLinejoin="round" d="M15 12a3 3 0 11-6 0 3 3 0 016 0z" />
                      <path strokeLinecap="round" strokeLinejoin="round" d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z" />
                    </svg>
                  )}
                </button>
              </div>
            </div>

            {/* Submit */}
            <button
              type="submit"
              disabled={loading}
              className="w-full flex justify-center py-3 px-4 rounded-xl text-sm font-semibold text-white shadow-lg shadow-indigo-500/25 active:scale-[0.98] disabled:opacity-50 disabled:cursor-not-allowed"
              style={{ background: 'linear-gradient(135deg, #4f46e5, #6366f1, #7c3aed)' }}
            >
              {loading ? <Spinner size="sm" /> : 'Sign in'}
            </button>
          </form>

          <p className="mt-10 text-center text-xs text-gray-400">
            Lead Management System &middot; TeamBlue
          </p>
        </div>
      </div>
    </div>
  );
}
