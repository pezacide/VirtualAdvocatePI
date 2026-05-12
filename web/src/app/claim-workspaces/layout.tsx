import { ProtectedRoute } from "@/components/ProtectedRoute";

export default function ClaimWorkspacesLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return <ProtectedRoute>{children}</ProtectedRoute>;
}