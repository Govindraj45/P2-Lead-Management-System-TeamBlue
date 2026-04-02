// This component shows a red error message box on the screen
// It takes two inputs: "message" (the error text) and "onDismiss" (a function to close the alert)
export default function ErrorAlert({ message, onDismiss }) {
  // If there is no error message, don't show anything at all
  if (!message) return null;
  return (
    // The outer red-tinted box that holds the error alert
    <div className="rounded-md bg-red-50 border border-red-200 p-4 mb-4">
      <div className="flex">
        <div className="flex-shrink-0">
          {/* SVG icon for error (circle with an X) */}
          <svg className="h-5 w-5 text-red-400" viewBox="0 0 20 20" fill="currentColor">
            <path fillRule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clipRule="evenodd" />
          </svg>
        </div>
        {/* This section displays the actual error message text in red */}
        <div className="ml-3 flex-1">
          <p className="text-sm text-red-700">{message}</p>
        </div>
        {/* If a dismiss function was provided, show a close (X) button on the right side */}
        {onDismiss && (
          <button onClick={onDismiss} className="ml-3 text-red-400 hover:text-red-600">
            {/* SVG icon for close button (X shape) */}
            <svg className="h-5 w-5" viewBox="0 0 20 20" fill="currentColor">
              <path fillRule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clipRule="evenodd" />
            </svg>
          </button>
        )}
      </div>
    </div>
  );
}

/*
 * FILE SUMMARY: ErrorAlert.jsx
 *
 * This file creates a reusable red error alert box that appears when something goes wrong
 * (like a failed API call or a validation error). It shows the error message text and
 * optionally includes a close button so the user can dismiss it. If no error message
 * exists, the component hides itself completely. It is used throughout the app wherever
 * errors need to be displayed to the user.
 */
