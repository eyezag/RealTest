import type { NextConfig } from "next";

const nextConfig: NextConfig = {
  // Produces a minimal, self-contained server bundle (only the files needed at runtime) —
  // keeps the production Docker image small. See src/frontend/Dockerfile.
  output: "standalone",
};

export default nextConfig;
