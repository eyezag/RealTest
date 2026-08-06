// Mirrors RealTest.Contracts on the backend. Keep in sync with:
// src/backend/RealTest.Contracts/Auth/*.cs and RealTest.Contracts/Common/*.cs

export type UserType = "Tester" | "CompanyUser" | "Administrator";

export type AccountStatus =
  | "PendingVerification"
  | "Active"
  | "Suspended"
  | "Disabled";

export interface UserSummary {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  displayName: string;
  userType: UserType;
  accountStatus: AccountStatus;
}

export interface AuthResponse {
  accessToken: string;
  accessTokenExpiresAtUtc: string;
  user: UserSummary;
}

export interface MessageResponse {
  message: string;
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  confirmPassword: string;
  userType: "Tester" | "CompanyUser";
  acceptTerms: boolean;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface VerifyEmailRequest {
  email: string;
  token: string;
}

export interface ResendVerificationRequest {
  email: string;
}

export interface ForgotPasswordRequest {
  email: string;
}

export interface ResetPasswordRequest {
  email: string;
  token: string;
  newPassword: string;
  confirmNewPassword: string;
}

/** Shape of the ProblemDetails/validation-problem responses the API returns on error. */
export interface ApiProblemDetails {
  type?: string;
  title?: string;
  status?: number;
  detail?: string;
  traceId?: string;
  errors?: Record<string, string[]>;
}
