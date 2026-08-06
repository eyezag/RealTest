import type { Metadata } from "next";
import Link from "next/link";
import { AuthPageShell } from "@/components/auth/auth-page-shell";
import { ForgotPasswordForm } from "@/components/auth/forgot-password-form";

export const metadata: Metadata = {
  title: "Forgot password — RealTest",
};

export default function ForgotPasswordPage() {
  return (
    <AuthPageShell
      title="Forgot your password?"
      description="Enter your email and we'll send you a link to reset it."
      footer={
        <Link href="/login" className="font-medium text-brand-600 hover:text-brand-700">
          Back to log in
        </Link>
      }
    >
      <ForgotPasswordForm />
    </AuthPageShell>
  );
}
