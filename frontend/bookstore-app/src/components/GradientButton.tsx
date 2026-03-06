/**
 * GradientButton Component
 *
 * Primary action button with gradient background and hover effect.
 * Used for main CTAs (Create, Save, Submit, etc.)
 */

import { forwardRef } from 'react';
import type React from 'react';
import { Icon } from './Icon';

interface GradientButtonProps {
  children: React.ReactNode;
  icon?: string; // Optional icon (Bootstrap icon name or Font Awesome class)
  onClick?: () => void;
  type?: 'button' | 'submit' | 'reset';
  disabled?: boolean;
  className?: string;
  variant?: 'primary' | 'danger' | 'success' | 'warning';
}

export const GradientButton = forwardRef<HTMLButtonElement, GradientButtonProps>(({
  children,
  icon,
  onClick,
  type = 'button',
  disabled = false,
  className = ''
}, ref) => {
  return (
    <button
      ref={ref}
      type={type}
      className={`btn-gradient ${className}`}
      onClick={onClick}
      disabled={disabled}
    >
      {icon && <Icon name={icon} />}
      {children}
    </button>
  );
});

export default GradientButton;
