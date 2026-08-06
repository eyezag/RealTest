import type { Metadata } from "next";
import Link from "next/link";
import { AuthPageShell } from "@/components/auth/auth-page-shell";
import { RegisterForm } from "@/components/auth/register-form";

export const metadata: Metadata = {
  title: "Create your account — RealTest",
};

export default function RegisterPage() {
  return (
    <AuthPageShell
      title="Create your account"
      description="Join RealTest as a tester or bring your product to real users."
      footer={
        <>
          Already have an account?{" "}
          <Link href="/login" className="font-medium text-brand-600 hover:text-brand-700">
            Log in
          </Link>
        </>
      }
    >
      <RegisterForm />
    </AuthPageShell>
  );
}
