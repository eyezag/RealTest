"use client";

import { zodResolver } from "@hookform/resolvers/zod";
import { useMutation } from "@tanstack/react-query";
import { useRouter } from "next/navigation";
import { useState } from "react";
import { useForm } from "react-hook-form";
import { register as registerAccount } from "@/lib/api/auth";
import { useAuth } from "@/components/providers/auth-provider";
import { Alert } from "@/components/ui/alert";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { dashboardPathForUserType } from "@/lib/auth/dashboard-path";
import { applyServerErrors } from "@/lib/forms/apply-server-errors";
import { registerSchema, type RegisterFormValues } from "@/lib/validation/auth-schemas";

const accountKindOptions = [
  {
    value: "tester" as const,
    title: "I want to test products",
    description: "Complete tasks, share feedback, and get rewarded for your time.",
  },
  {
    value: "company" as const,
    title: "I want users to test my product",
    description: "Recruit real people to test your website, app, or prototype.",
  },
];

export function RegisterForm() {
  const router = useRouter();
  const { applySession } = useAuth();
  const [formError, setFormError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    setError,
    watch,
    formState: { errors },
  } = useForm<RegisterFormValues>({
    resolver: zodResolver(registerSchema),
    defaultValues: {
      firstName: "",
      lastName: "",
      email: "",
      password: "",
      confirmPassword: "",
      acceptTerms: false,
    },
  });

  const accountKind = watch("accountKind");

  const mutation = useMutation({
    mutationFn: registerAccount,
    onSuccess: (data) => {
      applySession(data.accessToken, data.user);
      router.push(dashboardPathForUserType(data.user.userType));
    },
    onError: (error) => {
      const message = applyServerErrors<RegisterFormValues>(error, setError, {
        userType: "accountKind",
      });
      setFormError(message);
    },
  });

  const onSubmit = handleSubmit((values) => {
    setFormError(null);
    mutation.mutate({
      firstName: values.firstName,
      lastName: values.lastName,
      email: values.email,
      password: values.password,
      confirmPassword: values.confirmPassword,
      userType: values.accountKind === "tester" ? "Tester" : "CompanyUser",
      acceptTerms: values.acceptTerms,
    });
  });

  return (
    <form onSubmit={onSubmit} noValidate className="flex flex-col gap-6">
      {formError && <Alert variant="error">{formError}</Alert>}

      <fieldset className="flex flex-col gap-3">
        <legend className="text-sm font-medium text-slate-800 dark:text-slate-200">
          What brings you to RealTest? <span className="text-danger-600">*</span>
        </legend>
        <div className="grid gap-3 sm:grid-cols-2">
          {accountKindOptions.map((option) => (
            <label
              key={option.value}
              className={`flex cursor-pointer items-start gap-3 rounded-xl border p-4 transition-colors ${
                accountKind === option.value
                  ? "border-brand-600 bg-brand-50 dark:bg-brand-600/10"
                  : "border-slate-200 hover:border-slate-300 dark:border-slate-700 dark:hover:border-slate-600"
              }`}
            >
              <input
                type="radio"
                value={option.value}
                className="mt-1 h-4 w-4 shrink-0 text-brand-600 focus-visible:ring-2 focus-visible:ring-brand-500"
                {...register("accountKind")}
              />
              <span className="flex flex-col gap-1">
                <span className="text-sm font-semibold text-slate-900 dark:text-slate-50">
                  {option.title}
                </span>
                <span className="text-sm text-slate-500 dark:text-slate-400">{option.description}</span>
              </span>
            </label>
          ))}
        </div>
        {errors.accountKind && (
          <p className="text-sm text-danger-600" role="alert">
            {errors.accountKind.message}
          </p>
        )}
      </fieldset>

      <div className="grid gap-4 sm:grid-cols-2">
        <Input
          label="First name"
          required
          autoComplete="given-name"
          error={errors.firstName?.message}
          {...register("firstName")}
        />
        <Input
          label="Last name"
          required
          autoComplete="family-name"
          error={errors.lastName?.message}
          {...register("lastName")}
        />
      </div>

      <Input
        label="Email address"
        type="email"
        required
        autoComplete="email"
        error={errors.email?.message}
        {...register("email")}
      />

      <Input
        label="Password"
        type="password"
        required
        autoComplete="new-password"
        hint="At least 8 characters, with upper/lowercase letters, a digit, and a symbol."
        error={errors.password?.message}
        {...register("password")}
      />

      <Input
        label="Confirm password"
        type="password"
        required
        autoComplete="new-password"
        error={errors.confirmPassword?.message}
        {...register("confirmPassword")}
      />

      <label className="flex items-start gap-3 text-sm text-slate-600 dark:text-slate-300">
        <input
          type="checkbox"
          className="mt-0.5 h-4 w-4 rounded border-slate-300 text-brand-600 focus-visible:ring-2 focus-visible:ring-brand-500"
          {...register("acceptTerms")}
        />
        <span>
          I agree to the Terms of Service and Privacy Policy.
        </span>
      </label>
      {errors.acceptTerms && (
        <p className="-mt-4 text-sm text-danger-600" role="alert">
          {errors.acceptTerms.message}
        </p>
      )}

      <Button type="submit" size="lg" isLoading={mutation.isPending}>
        Create account
      </Button>
    </form>
  );
}
