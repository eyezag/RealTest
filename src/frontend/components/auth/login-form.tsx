"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { useAuth } from "@/components/providers/auth-provider";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { login } from "@/lib/api/auth";
import { dashboardPathForUserType } from "@/lib/auth/dashboard-path";
import { applyServerErrors } from "@/lib/forms/apply-server-errors";
import { loginSchema, type LoginFormValues } from "@/lib/validation/auth-schemas";

export function LoginForm() {
  const router = useRouter();
  const { applySession } = useAuth();
  const [formError, setFormError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<LoginFormValues>({
    resolver: zodResolver(loginSchema),
    defaultValues: { email: "", password: "" },
  });

  const mutation = useMutation({
    mutationFn: login,
    onSuccess: (data) => {
      applySession(data.accessToken, data.user);
      router.push(dashboardPathForUserType(data.user.userType));
    },
    onError: (error) => {
      setFormError(applyServerErrors<LoginFormValues>(error, setError));
    },
  });

  const onSubmit = handleSubmit((values) => {
    setFormError(null);
    mutation.mutate(values);
  });

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-6">
      {formError && <Alert variant="error">{formError}</Alert>}

      <Input
        label="Email address"
        type="email"
        required
        autoComplete="email"
        error={errors.email?.message}
        {...register("email")}
      />

      <div className="flex flex-col gap-1.5">
        <Input
          label="Password"
          type="password"
          required
          autoComplete="current-password"
          error={errors.password?.message}
          {...register("password")}
        />
        <Link href="/forgot-password" className="self-end text-sm font-medium text-brand-600 hover:text-brand-700">
          Forgot your password?
        </Link>
      </div>

      <Button type="submit" size="lg" isLoading={mutation.isPending}>
        Log in
      </Button>
    </form>
  );
}
