"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import Link from "next/link";
import { useForm } from "react-hook-form";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { resetPassword } from "@/lib/api/auth";
import { applyServerErrors } from "@/lib/forms/apply-server-errors";
import { resetPasswordSchema, type ResetPasswordFormValues } from "@/lib/validation/auth-schemas";
import { useState } from "react";

export function ResetPasswordForm({
  initialEmail,
  initialToken,
}: {
  initialEmail: string;
  initialToken: string;
}) {
  const [formError, setFormError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    setError,
    formState: { errors },
  } = useForm<ResetPasswordFormValues>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: {
      email: initialEmail,
      token: initialToken,
      newPassword: "",
      confirmNewPassword: "",
    },
  });

  const mutation = useMutation({
    mutationFn: resetPassword,
    onError: (error) => {
      setFormError(applyServerErrors<ResetPasswordFormValues>(error, setError));
    },
  });

  const onSubmit = handleSubmit((values) => {
    setFormError(null);
    mutation.mutate(values);
  });

  if (mutation.isSuccess) {
    return (
      <Alert variant="success">
        {mutation.data.message}{" "}
        <Link href="/login" className="font-medium underline">
          Log in now
        </Link>
        .
      </Alert>
    );
  }

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

      <Input
        label="Reset token"
        required
        hint="Paste the token from your password reset email."
        error={errors.token?.message}
        {...register("token")}
      />

      <Input
        label="New password"
        type="password"
        required
        autoComplete="new-password"
        hint="At least 8 characters, with upper/lowercase letters, a digit, and a symbol."
        error={errors.newPassword?.message}
        {...register("newPassword")}
      />

      <Input
        label="Confirm new password"
        type="password"
        required
        autoComplete="new-password"
        error={errors.confirmNewPassword?.message}
        {...register("confirmNewPassword")}
      />

      <Button type="submit" size="lg" isLoading={mutation.isPending}>
        Reset password
      </Button>
    </form>
  );
}
