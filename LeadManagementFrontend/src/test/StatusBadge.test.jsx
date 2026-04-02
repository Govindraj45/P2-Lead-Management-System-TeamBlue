import { render, screen } from '@testing-library/react';
import { describe, it, expect } from 'vitest';
import StatusBadge from '../components/ui/StatusBadge';

describe('StatusBadge', () => {
  it('renders the correct label for each status', () => {
    const statuses = ['New', 'Contacted', 'Qualified', 'Unqualified', 'Converted'];
    statuses.forEach((status) => {
      const { unmount } = render(<StatusBadge status={status} />);
      expect(screen.getByText(status)).toBeInTheDocument();
      unmount();
    });
  });

  it('applies correct color classes for New status', () => {
    render(<StatusBadge status="New" />);
    const badge = screen.getByText('New');
    expect(badge.className).toContain('bg-blue-100');
    expect(badge.className).toContain('text-blue-800');
  });

  it('falls back gracefully for unknown status', () => {
    render(<StatusBadge status="Unknown" />);
    expect(screen.getByText('Unknown')).toBeInTheDocument();
  });
});
