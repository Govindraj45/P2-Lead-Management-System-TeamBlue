import { render, screen, fireEvent, waitFor } from '@testing-library/react';
import { describe, it, expect, vi } from 'vitest';
import InteractionForm from '../components/forms/InteractionForm';

describe('InteractionForm', () => {
  it('renders all form fields', () => {
    render(<InteractionForm onSubmit={vi.fn()} loading={false} disabled={false} />);
    expect(screen.getByText('Type *')).toBeInTheDocument();
    expect(screen.getByText('Date *')).toBeInTheDocument();
    expect(screen.getByText('Details *')).toBeInTheDocument();
    expect(screen.getByText('Follow-up Date')).toBeInTheDocument();
    expect(screen.getByText('Add Interaction')).toBeInTheDocument();
  });

  it('shows validation errors when submitting empty form', async () => {
    render(<InteractionForm onSubmit={vi.fn()} loading={false} disabled={false} />);
    fireEvent.click(screen.getByText('Add Interaction'));

    await waitFor(() => {
      expect(screen.getByText('Type is required')).toBeInTheDocument();
      expect(screen.getByText('Date is required')).toBeInTheDocument();
      expect(screen.getByText('Details are required')).toBeInTheDocument();
    });
  });

  it('disables button when loading', () => {
    render(<InteractionForm onSubmit={vi.fn()} loading={true} disabled={false} />);
    expect(screen.getByText('Adding...')).toBeDisabled();
  });

  it('disables all inputs when disabled prop is true', () => {
    render(<InteractionForm onSubmit={vi.fn()} loading={false} disabled={true} />);
    const selects = document.querySelectorAll('select');
    const inputs = document.querySelectorAll('input');
    const textareas = document.querySelectorAll('textarea');
    [...selects, ...inputs, ...textareas].forEach((el) => {
      expect(el.disabled).toBe(true);
    });
  });
});
