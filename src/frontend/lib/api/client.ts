import { getAccessToken, setAccessToken } from "./token-store";
import type { ApiProblemDetails, AuthResponse } from "./types";

const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_URL?.replace(/\/+$/, "") ?? "http://localhost:5205";

export class ApiError extends Error {
  readonly status: number;
  readonly problem: ApiProblemDetails | null;

  constructor(status: number, problem: ApiProblemDetails | null, fallbackMessage: string) {
    super(problem?.detail ?? problem?.title ?? fallbackMessage);
    this.name = "ApiError";
    this.status = status;
    this.problem = problem;
  }

  /** Field-level validation messages, if this error came from a 400 validation-problem response. */
  get fieldErrors(): Record<string, string[]> {
    return this.problem?.errors ?? {};
  }
}

interface RequestOptions {
  method?: "GET" | "POST" | "PUT" | "DELETE";
  body?: unknown;
  /** Skip the automatic refresh-and-retry-once behavior (used by the refresh call itself). */
  skipAuthRetry?: boolean;
  /** Send without an Authorization header even if we have an access token. */
  anonymous?: boolean;
}

// Ensures concurrent 401s trigger only one /auth/refresh call, not one per failed request.
let refreshInFlight: Promise<boolean> | null = null;

async function refreshAccessToken(): Promise<boolean> {
  if (!refreshInFlight) {
    refreshInFlight = (async () => {
      try {
        const response = await fetch(`${API_BASE_URL}/api/v1/auth/refresh`, {
          method: "POST",
          credentials: "include",
        });
        if (!response.ok) {
          setAccessToken(null);
          return false;
        }
        const data = (await response.json()) as AuthResponse;
        setAccessToken(data.accessToken);
        return true;
      } catch {
        setAccessToken(null);
        return false;
      } finally {
        refreshInFlight = null;
      }
    })();
  }
  return refreshInFlight;
}

async function parseProblem(response: Response): Promise<ApiProblemDetails | null> {
  try {
    return (await response.json()) as ApiProblemDetails;
  } catch {
    return null;
  }
}

export async function apiFetch<T>(path: string, options: RequestOptions = {}): Promise<T> {
  const { method = "GET", body, skipAuthRetry = false, anonymous = false } = options;

  const doFetch = async (): Promise<Response> => {
    const headers: Record<string, string> = {};
    if (body !== undefined) {
      headers["Content-Type"] = "application/json";
    }
    const token = getAccessToken();
    if (token && !anonymous) {
      headers.Authorization = `Bearer ${token}`;
    }

    return fetch(`${API_BASE_URL}${path}`, {
      method,
      headers,
      credentials: "include", // carries the HTTP-only refresh-token cookie
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
  };

  let response = await doFetch();

  if (response.status === 401 && !anonymous && !skipAuthRetry) {
    const refreshed = await refreshAccessToken();
    if (refreshed) {
      response = await doFetch();
    }
  }

  if (!response.ok) {
    const problem = await parseProblem(response);
    throw new ApiError(response.status, problem, `Request failed with status ${response.status}`);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}

export { refreshAccessToken, API_BASE_URL };
