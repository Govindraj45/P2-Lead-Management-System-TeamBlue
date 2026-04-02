// Check if an email address looks valid (has text, then @, then a domain like "gmail.com")
export function validateEmail(email) {
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

// Check if a phone number looks valid (only digits, spaces, dashes, plus signs, and parentheses allowed)
// If no phone is provided, that's okay — phone is optional
export function validatePhone(phone) {
  if (!phone) return true; // optional
  return /^[\d\s\-+()]{7,20}$/.test(phone);
}

// Check the entire lead form for errors before sending it to the server
// Returns an object with error messages for each field that has a problem
// If the returned object is empty, the form is valid and ready to submit
export function validateLeadForm(form) {
  const errors = {};

  // Name must be filled in (not empty or just spaces)
  if (!form.name?.trim()) errors.name = 'Name is required';

  // Email must be filled in AND be a valid email format
  if (!form.email?.trim()) errors.email = 'Email is required';
  else if (!validateEmail(form.email)) errors.email = 'Invalid email format';

  // Phone is optional, but if provided it must look like a real phone number
  if (form.phone && !validatePhone(form.phone)) errors.phone = 'Invalid phone format';

  // Source (where the lead came from) must be selected
  if (!form.source) errors.source = 'Source is required';

  // Priority (how important the lead is) must be selected
  if (!form.priority) errors.priority = 'Priority is required';

  return errors;
}

/*
 * FILE SUMMARY:
 * This file contains helper functions that check if user input is correct before sending
 * it to the server. It validates email addresses, phone numbers, and the entire lead form.
 * By catching mistakes on the frontend first (like a missing name or bad email), we give
 * the user instant feedback without making them wait for the server to reject the data.
 * This improves user experience and reduces unnecessary network requests.
 */
