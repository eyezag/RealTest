import type { Metadata } from "next";
import Link from "next/link";
import { AuthPageShell } from "@/components/auth/auth-page-shell";
import { ResetPasswordForm } from "@/components/auth/reset-password-form";

export const metadata: Metadata = {
  title: "Reset password — RealTest",
};

export default async function ResetPasswordPage({
  searchParams,
}: {
  searchParams: Promise<{ email?: string; token?: string }>;
}) {
  const params = await searchParams;

  return (
    <AuthPageShell
      title="Reset your password"
      description="Choose a new password for your account."
      footer={
        <Link href="/login" className="font-medium text-brand-600 hover:text-brand-700">
          Back to log in
        </Link>
      }
    >
      <ResetPasswordForm initialEmail={params.email ?? ""} initialToken={params.token ?? ""} />
    </AuthPageShell>
  );
}
