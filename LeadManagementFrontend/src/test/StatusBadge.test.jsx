// Import testing utilities: render (draws the component), screen (finds elements)
import { render, screen } from '@testing-library/react';
// Import test functions from Vitest
import { describe, it, expect } from 'vitest';
// Import the StatusBadge component that we are testing
import StatusBadge from '../components/ui/StatusBadge';

// Group all StatusBadge tests together
describe('StatusBadge', () => {
  // TEST: The badge should display the correct text for every possible lead status
  it('renders the correct label for each status', () => {
    // List of all valid lead statuses in the system
    const statuses = ['New', 'Contacted', 'Qualified', 'Unqualified', 'Converted'];
    // Loop through each status, render the badge, check the text, then clean up
    statuses.forEach((status) => {
      const { unmount } = render(<StatusBadge status={status} />);
      // The rendered badge should show the status text
      expect(screen.getByText(status)).toBeInTheDocument();
      // Remove the component before rendering the next one
      unmount();
    });
  });

  // TEST: The "New" status badge should have blue background and text colors
  it('applies correct color classes for New status', () => {
    render(<StatusBadge status="New" />);
    // Find the badge element by its text
    const badge = screen.getByText('New');
    // Check that it has the correct Tailwind CSS color classes
    expect(badge.className).toContain('bg-blue-100');
    expect(badge.className).toContain('text-blue-800');
  });

  // TEST: An unknown status value should not crash — it should still render the text
  it('falls back gracefully for unknown status', () => {
    // Pass a status that is not in the predefined list
    render(<StatusBadge status="Unknown" />);
    // It should still display the text without crashing
    expect(screen.getByText('Unknown')).toBeInTheDocument();
  });
});

/*
 * FILE SUMMARY:
 * This file tests the StatusBadge component, which displays a colored label showing a lead's
 * current status (New, Contacted, Qualified, Unqualified, or Converted). It verifies that
 * each status renders the correct text, that the "New" status has the right blue color classes,
 * and that unknown status values do not crash the component. The StatusBadge is used throughout
 * the app wherever a lead's status needs to be shown as a visual indicator.
 */
