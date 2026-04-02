// Import navigation tools from React Router and our authentication context
import { Link, useNavigate, useLocation } from 'react-router-dom';
import { useAuth } from '../../context/AuthContext';

// Define the main navigation links shown in the top bar
// Each item has a label (display name), a URL path, and an SVG icon path
const NAV_ITEMS = [
  { label: 'Leads', path: '/leads', icon: 'M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2' },
  { label: 'Analytics', path: '/analytics', icon: 'M9 19v-6a2 2 0 00-2-2H5a2 2 0 00-2 2v6a2 2 0 002 2h2a2 2 0 002-2zm0 0V9a2 2 0 012-2h2a2 2 0 012 2v10m-6 0a2 2 0 002 2h2a2 2 0 002-2m0 0V5a2 2 0 012-2h2a2 2 0 012 2v14a2 2 0 01-2 2h-2a2 2 0 01-2-2z' },
];

// This is the main layout component that wraps every page in the app
// It provides the top navigation bar and renders the page content inside it
export default function AppLayout({ children }) {
  // Get the logged-in user's info and the logout function from AuthContext
  const { user, logout } = useAuth();
  // useNavigate lets us redirect the user to another page programmatically
  const navigate = useNavigate();
  // useLocation tells us which page the user is currently on
  const location = useLocation();

  // When the user clicks "Logout", clear their session and send them to the login page
  const handleLogout = () => {
    logout();
    navigate('/login');
  };

  return (
    // The full-page wrapper with a light gradient background
    <div className="min-h-screen" style={{ background: 'linear-gradient(135deg, #f8fafc 0%, #eef2ff 50%, #f5f3ff 100%)' }}>
      {/* The top navigation bar — it sticks to the top of the screen when scrolling */}
      <nav className="sticky top-0 z-50 border-b border-white/20 shadow-lg shadow-black/5"
        style={{ background: 'linear-gradient(135deg, #1e1b4b, #312e81, #3730a3)' }}>
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex justify-between h-16">
            {/* Left side of the nav bar: logo and navigation links */}
            <div className="flex items-center gap-8">
              {/* App logo and name — clicking it goes to the Leads page */}
              <Link to="/leads" className="flex items-center gap-2 group">
                <div className="w-8 h-8 rounded-lg bg-gradient-to-br from-indigo-400 to-purple-500 flex items-center justify-center shadow-md shadow-indigo-500/30 group-hover:shadow-indigo-500/50 group-hover:scale-105">
                  {/* SVG icon for the app logo (lightning bolt) */}
                  <svg className="w-4 h-4 text-white" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2.5}>
                    <path strokeLinecap="round" strokeLinejoin="round" d="M13 10V3L4 14h7v7l9-11h-7z" />
                  </svg>
                </div>
                <span className="text-lg font-bold text-white">LeadCRM</span>
              </Link>
              {/* Navigation links (Leads, Analytics) — hidden on very small screens */}
              <div className="hidden sm:flex gap-1">
                {NAV_ITEMS.map((item) => {
                  // Check if this nav link matches the current page URL to highlight it
                  const isActive = item.path === '/leads'
                    ? location.pathname === '/leads'
                    : location.pathname.startsWith(item.path);
                  return (
                    <Link
                      key={item.path}
                      to={item.path}
                      // Active link gets a brighter background; inactive links are dimmer
                      className={`flex items-center gap-1.5 px-3 py-2 rounded-lg text-sm font-medium ${
                        isActive
                          ? 'bg-white/15 text-white shadow-inner'
                          : 'text-indigo-200 hover:text-white hover:bg-white/10'
                      }`}
                    >
                      {/* SVG icon for this navigation item */}
                      <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                        <path strokeLinecap="round" strokeLinejoin="round" d={item.icon} />
                      </svg>
                      {item.label}
                    </Link>
                  );
                })}
              </div>
            </div>
            {/* Right side of the nav bar: user info and logout button */}
            <div className="flex items-center gap-3">
              {/* User avatar and info section — shows email and role */}
              <div className="flex items-center gap-2 px-3 py-1.5 rounded-lg bg-white/10 backdrop-blur-sm">
                {/* Circle avatar showing the first letter of the user's email */}
                <div className="w-7 h-7 rounded-full bg-gradient-to-br from-indigo-400 to-purple-400 flex items-center justify-center text-[10px] font-bold text-white uppercase">
                  {user?.email?.charAt(0) || 'U'}
                </div>
                {/* User's email and role — hidden on small screens to save space */}
                <div className="hidden md:block">
                  <p className="text-xs font-medium text-white leading-tight">{user?.email}</p>
                  <p className="text-[10px] text-indigo-300">{user?.role}</p>
                </div>
              </div>
              {/* Logout button — logs the user out and redirects to login */}
              <button
                onClick={handleLogout}
                className="flex items-center gap-1 px-3 py-1.5 rounded-lg text-sm text-indigo-200 hover:text-white hover:bg-white/10 font-medium"
              >
                {/* SVG icon for logout (door with arrow) */}
                <svg className="w-4 h-4" fill="none" viewBox="0 0 24 24" stroke="currentColor" strokeWidth={2}>
                  <path strokeLinecap="round" strokeLinejoin="round" d="M17 16l4-4m0 0l-4-4m4 4H7m6 4v1a3 3 0 01-3 3H6a3 3 0 01-3-3V7a3 3 0 013-3h4a3 3 0 013 3v1" />
                </svg>
                Logout
              </button>
            </div>
          </div>
        </div>
      </nav>
      {/* The main content area — this is where each page's content gets inserted */}
      <main className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8 animate-fade-in">
        {children}
      </main>
    </div>
  );
}

/*
 * FILE SUMMARY: AppLayout.jsx
 *
 * This is the main layout wrapper that appears on every page after the user logs in.
 * It provides the dark purple navigation bar at the top with the LeadCRM logo, links to
 * the Leads and Analytics pages, the logged-in user's avatar/email/role, and a Logout button.
 * The actual page content (passed as "children") is rendered below the nav bar. This layout
 * ensures a consistent look and navigation experience across the entire application.
 */
