import Link from "next/link";
import { env, getMissingPublicEnvVars } from "@/lib/env";

export default function EnvCheckPage() {
  const missingVars = getMissingPublicEnvVars();
  const isReady = missingVars.length === 0;

  return (
    <main className="min-h-screen bg-slate-950 px-6 py-12 text-white">
      <div className="mx-auto max-w-4xl">
        <Link href="/" className="text-sm text-cyan-300 hover:text-cyan-200">
          ← Back to home
        </Link>

        <section className="mt-10 rounded-2xl border border-white/10 bg-white/5 p-8">
          <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
            Environment check
          </p>

          <h1 className="mt-4 text-3xl font-bold">Web app environment variables</h1>

          <div className="mt-6 rounded-xl border border-white/10 bg-slate-900 p-5">
            <p className={isReady ? "text-green-300" : "text-yellow-300"}>
              {isReady
                ? "Environment variables are configured."
                : "Some environment variables still need values."}
            </p>
          </div>

          <div className="mt-6 space-y-3 text-sm text-slate-300">
            <p>
              API base URL:{" "}
              <span className="font-mono text-cyan-200">
                {env.apiBaseUrl || "Missing"}
              </span>
            </p>

            <p>
              Firebase project ID:{" "}
              <span className="font-mono text-cyan-200">
                {env.firebase.projectId || "Missing"}
              </span>
            </p>

            <p>
              Firebase auth domain:{" "}
              <span className="font-mono text-cyan-200">
                {env.firebase.authDomain || "Missing"}
              </span>
            </p>
          </div>

          {!isReady && (
            <div className="mt-6 rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-5">
              <h2 className="font-semibold text-yellow-200">Missing values</h2>
              <pre className="mt-3 whitespace-pre-wrap text-sm text-yellow-100">
                {missingVars.join("\n")}
              </pre>
            </div>
          )}
        </section>
      </div>
    </main>
  );
}