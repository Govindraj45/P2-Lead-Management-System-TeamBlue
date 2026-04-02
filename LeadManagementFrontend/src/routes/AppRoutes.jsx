// Import routing tools from React Router:
// BrowserRouter — enables URL-based navigation in the app
// Routes — a container that holds all the individual route definitions
// Route — defines a single URL path and what component to show for it
// Navigate — automatically redirects the user to a different page
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
// Import AuthProvider — wraps the app to provide login/logout functionality everywhere
import { AuthProvider } from '../context/AuthContext';
// Import ProtectedRoute — a guard that blocks pages from users who are not logged in
import ProtectedRoute from './ProtetedRoute';
// Import AppLayout — the shared page frame (navbar, sidebar, etc.) that wraps every page
import AppLayout from '../components/layout/AppLayout';

// Import all the page components — each one represents a full page in the app
import LoginPage from '../pages/LoginPage';
import LeadListPage from '../pages/LeadListPage';
import CreateLeadPage from '../pages/CreateLeadPage';
import EditLeadPage from '../pages/EditLeadPage';
import LeadDetailPage from '../pages/LeadDetailPage';
import AnalyticsDashboard from '../pages/AnalyticsDashboard';

// This component defines every URL in the app and which page component to show for each one
export default function AppRoutes() {
  return (
    // BrowserRouter enables the browser's back/forward buttons and URL bar to work with React
    <BrowserRouter>
      {/* AuthProvider wraps everything so any page can access login/logout/user info */}
      <AuthProvider>
        {/* Routes is the container — React checks each Route and shows the one that matches the current URL */}
        <Routes>
          {/* The login page — this is the only page you can visit without being logged in */}
          <Route path="/login" element={<LoginPage />} />

          {/* The leads list page — protected, so you must be logged in to see it */}
          <Route
            path="/leads"
            element={
              <ProtectedRoute>
                <AppLayout><LeadListPage /></AppLayout>
              </ProtectedRoute>
            }
          />

          {/* The page for creating a new lead — also protected */}
          <Route
            path="/leads/create"
            element={
              <ProtectedRoute>
                <AppLayout><CreateLeadPage /></AppLayout>
              </ProtectedRoute>
            }
          />

          {/* The page for editing an existing lead — :id in the URL is replaced by the actual lead's ID */}
          <Route
            path="/leads/:id/edit"
            element={
              <ProtectedRoute>
                <AppLayout><EditLeadPage /></AppLayout>
              </ProtectedRoute>
            }
          />

          {/* The page for viewing a single lead's details — :id is the lead's unique identifier */}
          <Route
            path="/leads/:id"
            element={
              <ProtectedRoute>
                <AppLayout><LeadDetailPage /></AppLayout>
              </ProtectedRoute>
            }
          />

          {/* The analytics/dashboard page — shows charts and reports about leads */}
          <Route
            path="/analytics"
            element={
              <ProtectedRoute>
                <AppLayout><AnalyticsDashboard /></AppLayout>
              </ProtectedRoute>
            }
          />

          {/* Catch-all route — if someone visits any unknown URL, redirect them to the leads page */}
          <Route path="*" element={<Navigate to="/leads" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}

/*
  FILE SUMMARY — AppRoutes.jsx
  This file is the "traffic controller" of the application. It maps every URL
  (like /login, /leads, /leads/create, /analytics) to the correct page component.
  Most pages are wrapped in ProtectedRoute, which prevents access unless the user
  is logged in. It also wraps protected pages in AppLayout to give them a consistent
  look (shared navbar, sidebar, etc.). If someone visits a URL that doesn't exist,
  they get automatically redirected to the leads list page.
*/
