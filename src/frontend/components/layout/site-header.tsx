"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useAuth } from "@/components/providers/auth-provider";
import { Button } from "@/components/ui/button";
import { dashboardPathForUserType } from "@/lib/auth/dashboard-path";

export function SiteHeader() {
  const { user, status, signOut } = useAuth();
  const router = useRouter();

  async function handleSignOut() {
    await signOut();
    router.push("/");
  }

  return (
    <header className="sticky top-0 z-40 border-b border-slate-200 bg-white/80 backdrop-blur-sm dark:border-slate-800 dark:bg-slate-950/80">
      <div className="mx-auto flex h-16 max-w-6xl items-center justify-between px-4 sm:px-6">
        <Link href="/" className="flex items-center gap-2 text-lg font-semibold tracking-tight text-slate-900 dark:text-slate-50">
          <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-brand-600 text-sm font-bold text-white">
            RT
          </span>
          RealTest
        </Link>

        <nav className="flex items-center gap-2 sm:gap-3">
          {status === "authenticated" && user ? (
            <>
              <Link
                href={dashboardPathForUserType(user.userType)}
                className="hidden text-sm font-medium text-slate-600 hover:text-slate-900 sm:inline dark:text-slate-300 dark:hover:text-white"
              >
                Dashboard
              </Link>
              <span className="hidden text-sm text-slate-500 sm:inline dark:text-slate-400">
                {user.displayName}
              </span>
              <Button variant="outline" size="sm" onClick={handleSignOut}>
                Sign out
              </Button>
            </>
          ) : status === "loading" ? (
            <div className="h-9 w-40 animate-pulse rounded-lg bg-slate-100 dark:bg-slate-800" />
          ) : (
            <>
              <Link href="/login">
                <Button variant="ghost" size="sm">
                  Log in
                </Button>
              </Link>
              <Link href="/register">
                <Button variant="primary" size="sm">
                  Get started
                </Button>
              </Link>
            </>
          )}
        </nav>
      </div>
    </header>
  );
}
