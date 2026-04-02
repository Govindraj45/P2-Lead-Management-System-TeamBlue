// This component shows a spinning loading circle while data is being fetched
// It takes an optional "size" input: 'sm' (small), 'md' (medium, the default), or 'lg' (large)
export default function Spinner({ size = 'md' }) {
  // Pick the height and width based on the size chosen
  const sizeClass = size === 'sm' ? 'h-5 w-5' : size === 'lg' ? 'h-12 w-12' : 'h-7 w-7';
  // Pick the border thickness — smaller spinner gets a thinner border
  const borderClass = size === 'sm' ? 'border-2' : 'border-[3px]';
  return (
    // Center the spinner on the screen with some padding
    <div className="flex items-center justify-center p-4">
      {/* A spinning circle made with a round div and colored borders */}
      <div className={`${sizeClass} animate-spin rounded-full ${borderClass} border-indigo-200 border-t-indigo-600 border-r-purple-500`} />
    </div>
  );
}

/*
 * FILE SUMMARY: Spinner.jsx
 *
 * This file creates a simple animated loading spinner that spins in a circle.
 * It is shown to the user while data is being loaded from the server, so they
 * know something is happening. The spinner comes in three sizes (small, medium,
 * large) and is used across many pages in the app whenever loading is needed.
 */
