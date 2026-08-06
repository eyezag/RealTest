"use client";

import { useRouter } from "next/navigation";
import { useEffect, type ReactNode } from "react";
import { useAuth } from "@/components/providers/auth-provider";
import type { UserType } from "@/lib/api/types";

export function ProtectedRoute({
  children,
  allowedUserTypes,
}: {
  children: ReactNode;
  /** Roles allowed to view this route. Omit to require only that the caller is signed in. */
  allowedUserTypes?: UserType[];
}) {
  const { user, status } = useAuth();
  const router = useRouter();

  const isAllowed = user !== null && (!allowedUserTypes || allowedUserTypes.includes(user.userType));

  useEffect(() => {
    if (status === "loading") return;

    if (status === "unauthenticated") {
      router.replace("/login");
      return;
    }

    if (status === "authenticated" && !isAllowed) {
      router.replace("/unauthorized");
    }
  }, [status, isAllowed, router]);

  if (status === "loading" || status === "unauthenticated" || !isAllowed) {
    return (
      <div className="flex flex-1 items-center justify-center py-24">
        <div
          className="h-8 w-8 animate-spin rounded-full border-2 border-slate-300 border-t-brand-600"
          role="status"
          aria-label="Checking your session"
        />
      </div>
    );
  }

  return <>{children}</>;
}
