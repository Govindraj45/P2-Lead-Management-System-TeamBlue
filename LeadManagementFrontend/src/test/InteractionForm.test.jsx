// Import testing utilities: render (draws the component), screen (finds elements),
// fireEvent (simulates clicks/typing), waitFor (waits for async changes)
import { render, screen, fireEvent, waitFor } from '@testing-library/react';
// Import test functions from Vitest: describe (groups tests), it (one test),
// expect (checks results), vi (creates fake functions)
import { describe, it, expect, vi } from 'vitest';
// Import the InteractionForm component that we are testing
import InteractionForm from '../components/forms/InteractionForm';

// Group all InteractionForm tests together under one label
describe('InteractionForm', () => {
  // TEST: The form should show all required fields when it first loads
  it('renders all form fields', () => {
    // Render the form with a fake onSubmit function, not loading, not disabled
    render(<InteractionForm onSubmit={vi.fn()} loading={false} disabled={false} />);
    // Check that all the expected labels and the submit button are visible
    expect(screen.getByText('Type *')).toBeInTheDocument();
    expect(screen.getByText('Date *')).toBeInTheDocument();
    expect(screen.getByText('Details *')).toBeInTheDocument();
    expect(screen.getByText('Follow-up Date')).toBeInTheDocument();
    expect(screen.getByText('Add Interaction')).toBeInTheDocument();
  });

  // TEST: Clicking submit without filling anything should show validation errors
  it('shows validation errors when submitting empty form', async () => {
    render(<InteractionForm onSubmit={vi.fn()} loading={false} disabled={false} />);
    // Click the submit button without filling in any fields
    fireEvent.click(screen.getByText('Add Interaction'));

    // Wait for the error messages to appear, then check they are displayed
    await waitFor(() => {
      expect(screen.getByText('Type is required')).toBeInTheDocument();
      expect(screen.getByText('Date is required')).toBeInTheDocument();
      expect(screen.getByText('Details are required')).toBeInTheDocument();
    });
  });

  // TEST: When the form is in a loading state, the button should say "Adding..." and be disabled
  it('disables button when loading', () => {
    // Pass loading={true} to simulate a form submission in progress
    render(<InteractionForm onSubmit={vi.fn()} loading={true} disabled={false} />);
    // The button text changes to "Adding..." and it should be disabled
    expect(screen.getByText('Adding...')).toBeDisabled();
  });

  // TEST: When the disabled prop is true, ALL form inputs should be disabled (no typing allowed)
  it('disables all inputs when disabled prop is true', () => {
    // Pass disabled={true} to lock the entire form (e.g., for converted leads)
    render(<InteractionForm onSubmit={vi.fn()} loading={false} disabled={true} />);
    // Find all select dropdowns, text inputs, and textareas in the form
    const selects = document.querySelectorAll('select');
    const inputs = document.querySelectorAll('input');
    const textareas = document.querySelectorAll('textarea');
    // Check that every single form element is disabled
    [...selects, ...inputs, ...textareas].forEach((el) => {
      expect(el.disabled).toBe(true);
    });
  });
});

/*
 * FILE SUMMARY:
 * This file tests the InteractionForm component, which is the form used to log new interactions
 * (calls, emails, meetings) with a lead. It verifies that all form fields render correctly,
 * that validation errors appear when submitting an empty form, that the button is disabled
 * during loading, and that all inputs are locked when the disabled prop is true.
 * These tests ensure the form behaves correctly in normal, loading, and read-only states.
 */
