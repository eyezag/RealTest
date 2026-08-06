"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { useForm } from "react-hook-form";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { forgotPassword } from "@/lib/api/auth";
import {
  forgotPasswordSchema,
  type ForgotPasswordFormValues,
} from "@/lib/validation/auth-schemas";

export function ForgotPasswordForm() {
  const {
    register,
    handleSubmit,
    formState: { errors },
  } = useForm<ForgotPasswordFormValues>({
    resolver: zodResolver(forgotPasswordSchema),
    defaultValues: { email: "" },
  });

  const mutation = useMutation({ mutationFn: forgotPassword });

  const onSubmit = handleSubmit((values) => mutation.mutate(values));

  if (mutation.isSuccess) {
    return (
      <Alert variant="success">
        {mutation.data.message} Check your inbox for further instructions.
      </Alert>
    );
  }

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-6">
      {mutation.isError && (
        <Alert variant="error">
          Something went wrong sending your reset link. Please try again.
        </Alert>
      )}

      <Input
        label="Email address"
        type="email"
        required
        autoComplete="email"
        error={errors.email?.message}
        {...register("email")}
      />

      <Button type="submit" size="lg" isLoading={mutation.isPending}>
        Send reset link
      </Button>
    </form>
  );
}
