export const LEAD_SOURCES = ['Website', 'Referral', 'ColdCall', 'Event', 'Partner'];
export const LEAD_PRIORITIES = ['Low', 'Medium', 'High'];

export const VALID_TRANSITIONS = {
  New: ['Contacted'],
  Contacted: ['Qualified', 'Unqualified'],
  Qualified: ['Converted'],
  Unqualified: [],
  Converted: [],
};
