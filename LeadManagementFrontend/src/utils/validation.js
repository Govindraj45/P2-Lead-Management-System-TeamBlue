export function validateEmail(email) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

export function validatePhone(phone) {
  if (!phone) return true; // optional
  return /^[\d\s\-+()]{7,20}$/.test(phone);
}

export function validateLeadForm(form) {
  const errors = {};

  if (!form.name?.trim()) errors.name = 'Name is required';
  if (!form.email?.trim()) errors.email = 'Email is required';
  else if (!validateEmail(form.email)) errors.email = 'Invalid email format';
  if (form.phone && !validatePhone(form.phone)) errors.phone = 'Invalid phone format';
  if (!form.source) errors.source = 'Source is required';
  if (!form.priority) errors.priority = 'Priority is required';

  return errors;
}
