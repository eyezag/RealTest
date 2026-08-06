import type { Metadata } from "next";
import { ProtectedRoute } from "@/components/auth/protected-route";
import { DashboardPlaceholder } from "@/components/dashboard/dashboard-placeholder";

export const metadata: Metadata = {
  title: "Admin dashboard — RealTest",
};

export default function AdminDashboardPage() {
  return (
    <ProtectedRoute allowedUserTypes={["Administrator"]}>
      <DashboardPlaceholder
        eyebrow="Administrator dashboard"
        title="Platform oversight"
        description="Manage users, moderate content, and keep the marketplace healthy."
        upcoming={[
          "User and account management",
          "Dispute resolution tools",
          "Platform-wide analytics",
          "Content moderation queue",
        ]}
      />
    </ProtectedRoute>
  );
}
