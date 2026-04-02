import { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth } from '../context/AuthContext';
import ErrorAlert from '../components/ui/ErrorAlert';
import Spinner from '../components/ui/Spinner';

/* Mini sparkline SVG for the dashboard preview cards */
const Sparkline = ({ d, color }) => (
  <svg viewBox="0 0 80 32" className="w-16 h-8" fill="none">
    <path d={d} stroke={color} strokeWidth="2" strokeLinecap="round" fill="none" />
  </svg>
);

export default function LoginPage() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPassword, setShowPassword] = useState(false);
  const [rememberMe, setRememberMe] = useState(false);
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
    <div className="min-h-screen flex flex-col lg:flex-row">
      {/* =========  LEFT PANEL – Blue promotional  ========= */}
      <div
        className="relative hidden lg:flex lg:w-[50%] flex-col justify-center px-12 xl:px-20 py-16 overflow-hidden"
        style={{ background: '#2563EB' }}
      >
        {/* Decorative circles */}
        <div className="absolute -top-32 -left-32 w-[500px] h-[500px] rounded-full border border-white/10" />
        <div className="absolute -bottom-40 -right-40 w-[600px] h-[600px] rounded-full border border-white/10" />
        <div className="absolute top-1/2 left-1/4 w-[350px] h-[350px] rounded-full bg-white/5" />

        {/* Logo */}
        <div className="relative z-10 mb-12">
          <div className="w-12 h-12 rounded-full border-[3px] border-white flex items-center justify-center">
            <svg className="w-6 h-6 text-white" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
              <path d="M12 2a10 10 0 0 1 10 10" />
              <path d="M12 2a10 10 0 0 0-6.93 17.07" />
            </svg>
          </div>
        </div>

        {/* Promotional copy */}
        <div className="relative z-10 max-w-md">
          <h2 className="text-4xl xl:text-5xl font-bold text-white leading-tight">
            Lead Management<br />System
          </h2>
          <p className="mt-4 text-blue-100/80 text-base leading-relaxed">
            View all the analytics and grow your business from anywhere!
          </p>

        </div>

        {/* Floating dashboard preview card */}
        <div className="relative z-10 mt-12 max-w-lg">
          <div className="rounded-2xl overflow-hidden shadow-2xl shadow-black/30 border border-white/10"
            style={{ background: '#0f1117' }}>
            {/* Title bar dots */}
            <div className="flex items-center gap-1.5 px-4 pt-3 pb-2">
              <span className="w-2.5 h-2.5 rounded-full bg-red-500" />
              <span className="w-2.5 h-2.5 rounded-full bg-yellow-500" />
              <span className="w-2.5 h-2.5 rounded-full bg-green-500" />
              <span className="ml-4 text-[10px] text-gray-500 font-mono">app.leadcrm.io</span>
            </div>

            <div className="flex">
              {/* Mini sidebar */}
              <div className="w-28 border-r border-white/5 px-2 py-3 space-y-1 text-[10px]">
                {['Leads', 'Pipeline', 'Contacts', 'Analytics', 'Sales Reps', 'Reports'].map((item, i) => (
                  <div key={item} className={`px-2 py-1.5 rounded-md flex items-center gap-1.5 ${i === 0 ? 'bg-blue-600 text-white' : 'text-gray-500'}`}>
                    <span className="w-3 h-3 rounded bg-current opacity-40 inline-block" />
                    {item}
                  </div>
                ))}
              </div>

              {/* Mini main content */}
              <div className="flex-1 px-4 py-3">
                <p className="text-white font-bold text-sm">Lead Pipeline</p>
                <p className="text-gray-500 text-[10px] mt-0.5 mb-3">This Month</p>

                {/* Metric cards */}
                <div className="grid grid-cols-3 gap-2 mb-3">
                  {[
                    { label: 'New Leads', val: '104', spark: 'M0 24 L10 20 L20 22 L30 14 L40 16 L50 8 L60 12 L70 6 L80 8', color: '#60a5fa' },
                    { label: 'Converted', val: '24', spark: 'M0 20 L15 18 L30 24 L45 10 L60 16 L80 6', color: '#34d399' },
                    { label: 'Interactions', val: '281', spark: 'M0 16 L20 20 L40 12 L60 22 L80 10', color: '#fbbf24' },
                  ].map((m, i) => (
                    <div key={i} className="rounded-lg p-2" style={{ background: '#1a1d27' }}>
                      <p className="text-gray-500 text-[8px] mb-0.5">{m.label}</p>
                      <p className="text-white text-sm font-bold">{m.val}</p>
                      <Sparkline d={m.spark} color={m.color} />
                    </div>
                  ))}
                </div>

                {/* Mini conversion trend chart */}
                <div className="rounded-lg p-3" style={{ background: '#1a1d27' }}>
                  <p className="text-gray-500 text-[8px] mb-1">Conversion Trend</p>
                  <svg viewBox="0 0 260 60" className="w-full h-12" fill="none">
                    <path d="M0 50 L30 42 L60 48 L90 30 L120 36 L150 20 L180 28 L210 12 L240 18 L260 8"
                      stroke="#60a5fa" strokeWidth="2" strokeLinecap="round" />
                    <path d="M0 45 L40 40 L80 38 L120 32 L160 28 L200 22 L240 20 L260 16"
                      stroke="#fbbf24" strokeWidth="1.5" strokeLinecap="round" strokeDasharray="4 3" />
                    <path d="M0 50 L30 42 L60 48 L90 30 L120 36 L150 20 L180 28 L210 12 L240 18 L260 8 L260 60 L0 60Z"
                      fill="url(#chartGrad)" opacity="0.15" />
                    <defs>
                      <linearGradient id="chartGrad" x1="0" y1="0" x2="0" y2="1">
                        <stop offset="0%" stopColor="#3b82f6" />
                        <stop offset="100%" stopColor="transparent" />
                      </linearGradient>
                    </defs>
                  </svg>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* =========  RIGHT PANEL – Dark login form  ========= */}
      <div
        className="flex-1 flex items-center justify-center px-6 sm:px-12 lg:px-16 xl:px-24 py-12"
        style={{ background: '#0a0a0a' }}
      >
        <div className="w-full max-w-md animate-fade-in">
          <h1 className="text-3xl sm:text-4xl font-bold text-white mb-10">Log in</h1>

          <ErrorAlert message={error} />

          <form onSubmit={handleSubmit} className="space-y-6">
            {/* Email */}
            <div>
              <label htmlFor="email" className="block text-sm font-medium text-gray-400 mb-2">
                Email address
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                  <svg className="h-4 w-4 text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                    <path strokeLinecap="round" strokeLinejoin="round" d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z" />
                  </svg>
                </div>
                <input
                  id="email"
                  type="email"
                  required
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                  className="block w-full rounded-xl border border-white/10 bg-white/5 pl-11 pr-4 py-3 text-white placeholder-gray-600 focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:bg-white/8 text-sm outline-none"
                  placeholder="name@example.com"
                />
              </div>
            </div>

            {/* Password */}
            <div>
              <label htmlFor="password" className="block text-sm font-medium text-gray-400 mb-2">
                Password
              </label>
              <div className="relative">
                <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                  <svg className="h-4 w-4 text-gray-500" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={1.5}>
                    <path strokeLinecap="round" strokeLinejoin="round" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z" />
                  </svg>
                </div>
                <input
                  id="password"
                  type={showPassword ? 'text' : 'password'}
                  required
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  className="block w-full rounded-xl border border-white/10 bg-white/5 pl-11 pr-11 py-3 text-white placeholder-gray-600 focus:border-blue-500 focus:ring-1 focus:ring-blue-500 focus:bg-white/8 text-sm outline-none"
                  placeholder="••••••••"
                />
                <button
                  type="button"
                  onClick={() => setShowPassword(!showPassword)}
                  className="absolute inset-y-0 right-0 pr-4 flex items-center text-gray-500 hover:text-gray-300"
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

            {/* Remember password */}
            <div className="flex items-center">
              <input
                id="remember"
                type="checkbox"
                checked={rememberMe}
                onChange={(e) => setRememberMe(e.target.checked)}
                className="h-4 w-4 rounded border-gray-600 bg-transparent text-blue-600 focus:ring-blue-500 focus:ring-offset-0"
              />
              <label htmlFor="remember" className="ml-2 text-sm text-gray-500">
                Remember password
              </label>
            </div>

            {/* Sign in button */}
            <button
              type="submit"
              disabled={loading}
              className="w-full flex justify-center py-3.5 px-4 rounded-xl text-sm font-semibold text-white bg-blue-600 hover:bg-blue-500 disabled:opacity-50 disabled:cursor-not-allowed active:scale-[0.98] shadow-lg shadow-blue-600/25"
            >
              {loading ? <Spinner size="sm" /> : 'Sign in'}
            </button>
          </form>


        </div>
      </div>
    </div>
  );
}
