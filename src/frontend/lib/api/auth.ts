import { apiFetch } from "./client";
import type {
  AuthResponse,
  ForgotPasswordRequest,
  LoginRequest,
  MessageResponse,
  RegisterRequest,
  ResendVerificationRequest,
  ResetPasswordRequest,
  UserSummary,
  VerifyEmailRequest,
} from "./types";

export function register(request: RegisterRequest): Promise<AuthResponse> {
  return apiFetch<AuthResponse>("/api/v1/auth/register", {
    method: "POST",
    body: request,
    anonymous: true,
  });
}

export function login(request: LoginRequest): Promise<AuthResponse> {
  return apiFetch<AuthResponse>("/api/v1/auth/login", {
    method: "POST",
    body: request,
    anonymous: true,
  });
}

export function logout(): Promise<void> {
  return apiFetch<void>("/api/v1/auth/logout", { method: "POST", skipAuthRetry: true });
}

export function getCurrentUser(): Promise<UserSummary> {
  return apiFetch<UserSummary>("/api/v1/auth/me");
}

export function verifyEmail(request: VerifyEmailRequest): Promise<MessageResponse> {
  return apiFetch<MessageResponse>("/api/v1/auth/verify-email", {
    method: "POST",
    body: request,
    anonymous: true,
  });
}

export function resendVerification(request: ResendVerificationRequest): Promise<MessageResponse> {
  return apiFetch<MessageResponse>("/api/v1/auth/resend-verification", {
    method: "POST",
    body: request,
    anonymous: true,
  });
}

export function forgotPassword(request: ForgotPasswordRequest): Promise<MessageResponse> {
  return apiFetch<MessageResponse>("/api/v1/auth/forgot-password", {
    method: "POST",
    body: request,
    anonymous: true,
  });
}

export function resetPassword(request: ResetPasswordRequest): Promise<MessageResponse> {
  return apiFetch<MessageResponse>("/api/v1/auth/reset-password", {
    method: "POST",
    body: request,
    anonymous: true,
  });
}
