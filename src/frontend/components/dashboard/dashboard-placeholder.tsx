"use client";

import { useAuth } from "@/components/providers/auth-provider";
import { Card, CardContent } from "@/components/ui/card";

export function DashboardPlaceholder({
  eyebrow,
  title,
  description,
  upcoming,
}: {
  eyebrow: string;
  title: string;
  description: string;
  upcoming: string[];
}) {
  const { user } = useAuth();

  return (
    <div className="mx-auto flex w-full max-w-4xl flex-1 flex-col gap-8 px-4 py-12 sm:px-6 sm:py-16">
      <div>
        <p className="text-sm font-semibold uppercase tracking-wide text-brand-600">{eyebrow}</p>
        <h1 className="mt-2 text-3xl font-semibold tracking-tight text-slate-900 dark:text-slate-50">
          {title}
        </h1>
        <p className="mt-2 max-w-2xl text-slate-600 dark:text-slate-400">{description}</p>
      </div>

      <Card>
        <CardContent className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <div>
            <p className="text-sm text-slate-500 dark:text-slate-400">Signed in as</p>
            <p className="text-lg font-medium text-slate-900 dark:text-slate-50">
              {user?.displayName}
            </p>
            <p className="text-sm text-slate-500 dark:text-slate-400">{user?.email}</p>
          </div>
          <span className="inline-flex w-fit items-center rounded-full bg-brand-50 px-3 py-1 text-xs font-medium text-brand-700 dark:bg-brand-600/10 dark:text-brand-300">
            {user?.accountStatus === "Active" ? "Account active" : "Verification pending"}
          </span>
        </CardContent>
      </Card>

      <Card>
        <CardContent>
          <h2 className="text-base font-semibold text-slate-900 dark:text-slate-50">
            Coming soon
          </h2>
          <p className="mt-1 text-sm text-slate-500 dark:text-slate-400">
            This foundation ships authentication only. The features below are planned for the
            next phase of RealTest.
          </p>
          <ul className="mt-4 flex flex-col gap-2">
            {upcoming.map((item) => (
              <li key={item} className="flex items-center gap-2 text-sm text-slate-700 dark:text-slate-300">
                <span className="h-1.5 w-1.5 shrink-0 rounded-full bg-slate-300 dark:bg-slate-600" />
                {item}
              </li>
            ))}
          </ul>
        </CardContent>
      </Card>
    </div>
  );
}
