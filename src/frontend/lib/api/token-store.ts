// The access token lives in memory only — never in localStorage or a JS-readable cookie.
// It's lost on a full page reload by design; ApiProvider/AuthProvider silently calls
// /auth/refresh (using the HTTP-only refresh-token cookie) on mount to recover a session.
// See README.md "Security notes" for the reasoning.

let accessToken: string | null = null;

export function getAccessToken(): string | null {
  return accessToken;
}

export function setAccessToken(token: string | null): void {
  accessToken = token;
}
