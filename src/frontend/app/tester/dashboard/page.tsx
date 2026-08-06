import type { Metadata } from "next";
import { ProtectedRoute } from "@/components/auth/protected-route";
import { DashboardPlaceholder } from "@/components/dashboard/dashboard-placeholder";

export const metadata: Metadata = {
  title: "Tester dashboard — RealTest",
};

export default function TesterDashboardPage() {
  return (
    <ProtectedRoute allowedUserTypes={["Tester"]}>
      <DashboardPlaceholder
        eyebrow="Tester dashboard"
        title="Ready to test"
        description="Browse testing opportunities, complete tasks, and get rewarded for honest feedback."
        upcoming={[
          "Testing campaigns matched to your profile",
          "Task submissions with evidence uploads",
          "Bug reporting tools",
          "Rewards and payout tracking",
        ]}
      />
    </ProtectedRoute>
  );
}
