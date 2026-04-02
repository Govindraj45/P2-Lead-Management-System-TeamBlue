// Import Navigate — used to automatically redirect users to a different page
import { Navigate } from 'react-router-dom';
// Import useAuth — our custom hook that tells us who is currently logged in
import { useAuth } from '../context/AuthContext';

// ProtectedRoute is a "guard" component — it wraps pages that should only be visible to logged-in users
// "children" is the actual page content that we want to protect
// "roles" is an optional list of allowed roles (e.g., only Admin or SalesManager)
export default function ProtectedRoute({ children, roles }) {
  // Get the current user's info from the auth context
  const { user } = useAuth();

  // If there is no user (not logged in), send them to the login page
  if (!user) {
    return <Navigate to="/login" replace />;
  }

  // If specific roles are required and the user's role is not in the allowed list,
  // redirect them to the leads page (they're logged in but don't have permission)
  if (roles && !roles.includes(user.role)) {
    return <Navigate to="/leads" replace />;
  }

  // If the user is logged in (and has the right role if required), show the actual page
  return children;
}

/*
  FILE SUMMARY — ProtectedRoute.jsx
  This component acts as a security gate for pages in the app. It checks two things:
  (1) Is the user logged in? If not, they get sent to the login page.
  (2) Does the user have the right role? If roles are specified and the user's role
  doesn't match, they get redirected to the leads page. Only if both checks pass
  does the user see the actual page content. This is how the app prevents
  unauthorized access to sensitive pages like analytics or lead editing.
*/
