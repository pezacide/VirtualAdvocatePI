import type { Metadata } from "next";
import "./globals.css";
import { AuthProvider } from "@/components/AuthProvider";

export const metadata: Metadata = {
  title: "Virtual Advocate PI",
  description: "Post-2026 PI claim preparation support for veterans.",
};

export default function RootLayout({
  children,
}: Readonly<{
  children: React.ReactNode;
}>) {
  return (
    <html lang="en-AU">
      <body>
        <AuthProvider>{children}</AuthProvider>
      </body>
    </html>
  );
}