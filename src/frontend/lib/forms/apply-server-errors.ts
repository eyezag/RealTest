import type { FieldValues, Path, UseFormSetError } from "react-hook-form";
import { ApiError } from "@/lib/api/client";

/**
 * Maps a failed mutation's ApiError field errors onto react-hook-form. Returns a
 * form-level message to show in an <Alert> when the error has no field-level errors
 * to attach (e.g. 401/403/409/500 responses), or null when field errors covered it.
 */
export function applyServerErrors<T extends FieldValues>(
  error: unknown,
  setError: UseFormSetError<T>,
  fieldNameMap: Partial<Record<string, Path<T>>> = {},
): string | null {
  if (!(error instanceof ApiError)) {
    return error instanceof Error ? error.message : "Something went wrong. Please try again.";
  }

  const entries = Object.entries(error.fieldErrors);
  if (entries.length === 0) {
    return error.message;
  }

  let unmapped = false;
  for (const [field, messages] of entries) {
    const targetField = (fieldNameMap[field] ?? (field as Path<T>)) as Path<T> | undefined;
    if (targetField) {
      setError(targetField, { type: "server", message: messages.join(" ") });
    } else {
      unmapped = true;
    }
  }

  return unmapped ? error.message : null;
}
