import { type ReactNode } from "react";
import { cn } from "@/lib/cn";

type AlertVariant = "error" | "success" | "info";

const variantClasses: Record<AlertVariant, string> = {
  error: "bg-danger-50 text-danger-700 border-danger-200 dark:bg-danger-600/10 dark:border-danger-600/30 dark:text-red-300",
  success:
    "bg-success-50 text-success-700 border-green-200 dark:bg-success-600/10 dark:border-success-600/30 dark:text-green-300",
  info: "bg-brand-50 text-brand-800 border-brand-200 dark:bg-brand-600/10 dark:border-brand-600/30 dark:text-brand-200",
};

export function Alert({
  variant = "info",
  children,
  className,
}: {
  variant?: AlertVariant;
  children: ReactNode;
  className?: string;
}) {
  return (
    <div
      role={variant === "error" ? "alert" : "status"}
      className={cn(
        "rounded-lg border px-4 py-3 text-sm leading-relaxed",
        variantClasses[variant],
        className,
      )}
    >
      {children}
    </div>
  );
}
