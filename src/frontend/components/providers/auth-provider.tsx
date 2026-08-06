"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useState,
  type ReactNode,
} from "react";
import { getCurrentUser, logout as apiLogout } from "@/lib/api/auth";
import { refreshAccessToken } from "@/lib/api/client";
import { setAccessToken } from "@/lib/api/token-store";
import type { UserSummary } from "@/lib/api/types";

type AuthStatus = "loading" | "authenticated" | "unauthenticated";

interface AuthContextValue {
  user: UserSummary | null;
  status: AuthStatus;
  /** Called by login/register flows once they have a fresh access token + user summary. */
  applySession: (accessToken: string, user: UserSummary) => void;
  signOut: () => Promise<void>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<UserSummary | null>(null);
  const [status, setStatus] = useState<AuthStatus>("loading");

  useEffect(() => {
    let cancelled = false;

    async function bootstrap() {
      const refreshed = await refreshAccessToken();
      if (cancelled) return;

      if (!refreshed) {
        setStatus("unauthenticated");
        return;
      }

      try {
        const me = await getCurrentUser();
        if (cancelled) return;
        setUser(me);
        setStatus("authenticated");
      } catch {
        if (!cancelled) {
          setAccessToken(null);
          setStatus("unauthenticated");
        }
      }
    }

    void bootstrap();
    return () => {
      cancelled = true;
    };
  }, []);

  const applySession = useCallback((accessToken: string, nextUser: UserSummary) => {
    setAccessToken(accessToken);
    setUser(nextUser);
    setStatus("authenticated");
  }, []);

  const signOut = useCallback(async () => {
    try {
      await apiLogout();
    } catch {
      // Logout is best-effort client-side regardless of network/API outcome.
    } finally {
      setAccessToken(null);
      setUser(null);
      setStatus("unauthenticated");
    }
  }, []);

  return (
    <AuthContext.Provider value={{ user, status, applySession, signOut }}>
      {children}
    </AuthContext.Provider>
  );
}

export function useAuth(): AuthContextValue {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within an AuthProvider.");
  }
  return context;
}
