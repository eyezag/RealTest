import type { Metadata } from "next";
import { ProtectedRoute } from "@/components/auth/protected-route";
import { DashboardPlaceholder } from "@/components/dashboard/dashboard-placeholder";

export const metadata: Metadata = {
  title: "Company dashboard — RealTest",
};

export default function CompanyDashboardPage() {
  return (
    <ProtectedRoute allowedUserTypes={["CompanyUser"]}>
      <DashboardPlaceholder
        eyebrow="Company dashboard"
        title="Recruit real testers"
        description="Set up your workspace and start getting structured feedback from real users."
        upcoming={[
          "Campaign creation and tester recruitment",
          "Company workspace and team management",
          "Bug report review and triage",
          "Feedback and ratings analytics",
        ]}
      />
    </ProtectedRoute>
  );
}
