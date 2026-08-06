import type { Metadata } from "next";
import Link from "next/link";
import { AuthPageShell } from "@/components/auth/auth-page-shell";
import { LoginForm } from "@/components/auth/login-form";

export const metadata: Metadata = {
  title: "Log in — RealTest",
};

export default function LoginPage() {
  return (
    <AuthPageShell
      title="Welcome back"
      description="Log in to continue testing products or managing your campaigns."
      footer={
        <>
          Don&apos;t have an account?{" "}
          <Link href="/register" className="font-medium text-brand-600 hover:text-brand-700">
            Sign up
          </Link>
        </>
      }
    >
      <LoginForm />
    </AuthPageShell>
  );
}
