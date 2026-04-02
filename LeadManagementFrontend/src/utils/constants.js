// The list of places a lead can come from (how did we find this potential customer?)
export const LEAD_SOURCES = ['Website', 'Referral', 'ColdCall', 'Event', 'Partner'];

// The priority levels for a lead — how important or urgent this lead is
export const LEAD_PRIORITIES = ['Low', 'Medium', 'High'];

// This defines which status a lead can move to next
// For example, a "New" lead can only move to "Contacted" — it can't skip ahead
// "Converted" and "Unqualified" are final states — they can't change to anything else
export const VALID_TRANSITIONS = {
  New: ['Contacted'],
  Contacted: ['Qualified', 'Unqualified'],
  Qualified: ['Converted'],
  Unqualified: [],
  Converted: [],
};

/*
 * FILE SUMMARY:
 * This file stores fixed values (constants) that are used across the entire app.
 * It defines where leads can come from, how important they are, and the rules for
 * changing a lead's status. By keeping these values in one place, every part of the
 * app stays consistent — if a new source or status is added, we only change it here.
 */
