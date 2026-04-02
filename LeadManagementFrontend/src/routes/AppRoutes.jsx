import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom';
import { AuthProvider } from '../context/AuthContext';
import ProtectedRoute from './ProtetedRoute';
import AppLayout from '../Components/layout/AppLayout';
import LoginPage from '../pages/LoginPage';
import LeadListPage from '../pages/LeadListPage';
import CreateLeadPage from '../pages/CreateLeadPage';
import EditLeadPage from '../pages/EditLeadPage';
import LeadDetailPage from '../pages/LeadDetailPage';
import AnalyticsDashboard from '../pages/AnalyticsDashboard';

export default function AppRoutes() {
  return (
    <BrowserRouter>
      <AuthProvider>
        <Routes>
          <Route path="/login" element={<LoginPage />} />
          <Route
            path="/leads"
            element={
              <ProtectedRoute>
                <AppLayout><LeadListPage /></AppLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/leads/create"
            element={
              <ProtectedRoute>
                <AppLayout><CreateLeadPage /></AppLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/leads/:id/edit"
            element={
              <ProtectedRoute>
                <AppLayout><EditLeadPage /></AppLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/leads/:id"
            element={
              <ProtectedRoute>
                <AppLayout><LeadDetailPage /></AppLayout>
              </ProtectedRoute>
            }
          />
          <Route
            path="/analytics"
            element={
              <ProtectedRoute>
                <AppLayout><AnalyticsDashboard /></AppLayout>
              </ProtectedRoute>
            }
          />
          <Route path="*" element={<Navigate to="/leads" replace />} />
        </Routes>
      </AuthProvider>
    </BrowserRouter>
  );
}
