import type { UserType } from "@/lib/api/types";

/** Where each role lands after signing in. */
export function dashboardPathForUserType(userType: UserType): string {
  switch (userType) {
    case "Tester":
      return "/tester/dashboard";
    case "CompanyUser":
      return "/company/dashboard";
    case "Administrator":
      return "/admin/dashboard";
  }
}
