// Import the AppRoutes component — it decides which page to show based on the URL
import AppRoutes from './routes/AppRoutes';

// The App component is the top-level component of the entire application
// It simply renders AppRoutes, which handles all the page navigation
function App() {
  return <AppRoutes />;
}

// Export App so that main.jsx can import and use it
export default App;

/*
  FILE SUMMARY — App.jsx
  This is the root component of the React application. It acts as a simple wrapper
  that delegates all the work to AppRoutes, which controls page navigation.
  Every component in the app is eventually a child of this one. It's kept minimal
  on purpose — routing and layout logic live in dedicated files to keep things organized.
*/
