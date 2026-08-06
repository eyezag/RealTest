import type { Metadata } from "next";
import Link from "next/link";
import { Button } from "@/components/ui/button";

export const metadata: Metadata = {
  title: "Access denied — RealTest",
};

export default function UnauthorizedPage() {
  return (
    <div className="flex flex-1 flex-col items-center justify-center gap-4 px-4 py-24 text-center">
      <span className="flex h-14 w-14 items-center justify-center rounded-full bg-warning-50 text-2xl dark:bg-warning-600/10">
        🔒
      </span>
      <h1 className="text-2xl font-semibold text-slate-900 dark:text-slate-50">Access denied</h1>
      <p className="max-w-md text-slate-600 dark:text-slate-400">
        Your account doesn&apos;t have permission to view this page. If you think this is a
        mistake, contact support.
      </p>
      <Link href="/">
        <Button variant="primary">Back to home</Button>
      </Link>
    </div>
  );
}
