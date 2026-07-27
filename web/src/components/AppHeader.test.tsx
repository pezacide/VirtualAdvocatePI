import { describe, expect, it, vi } from "vitest";
import { render, screen } from "@testing-library/react";
import { AppHeader } from "./AppHeader";
import { useAuth } from "@/components/AuthProvider";

vi.mock("@/components/AuthProvider", () => ({
  useAuth: vi.fn(),
}));

const mockedUseAuth = vi.mocked(useAuth);

describe("AppHeader", () => {
  it("shows a sign-in link when no user is signed in", () => {
    mockedUseAuth.mockReturnValue({
      user: null,
      loading: false,
      signIn: vi.fn(),
      register: vi.fn(),
      signOutUser: vi.fn(),
      getIdToken: vi.fn(),
    });

    render(<AppHeader />);

    expect(screen.getByRole("link", { name: /sign in/i })).toBeInTheDocument();
    expect(screen.queryByText(/sign out/i)).not.toBeInTheDocument();
  });

  it("shows the signed-in user's email and a sign-out button", () => {
    mockedUseAuth.mockReturnValue({
      user: { email: "veteran@example.test" } as never,
      loading: false,
      signIn: vi.fn(),
      register: vi.fn(),
      signOutUser: vi.fn(),
      getIdToken: vi.fn(),
    });

    render(<AppHeader />);

    expect(screen.getByText("veteran@example.test")).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /sign out/i })).toBeInTheDocument();
  });

  it("shows a loading state while the session is being checked", () => {
    mockedUseAuth.mockReturnValue({
      user: null,
      loading: true,
      signIn: vi.fn(),
      register: vi.fn(),
      signOutUser: vi.fn(),
      getIdToken: vi.fn(),
    });

    render(<AppHeader />);

    expect(screen.getByText(/checking session/i)).toBeInTheDocument();
  });
});
