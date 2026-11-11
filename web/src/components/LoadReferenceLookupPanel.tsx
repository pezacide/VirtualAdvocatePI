"use client";

import { useEffect, useMemo, useState } from "react";
import { useAuth } from "@/components/AuthProvider";
import { LoadReferenceItem, getLoadReferenceItems } from "@/lib/api";

const categoryLabels: Record<string, string> = {
  WEBBING_BODY_ARMOUR: "Webbing and body armour",
  PACKS: "Packs",
  AMMUNITION_ORDNANCE: "Ammunition and ordnance",
  NAVAL_DAMAGE_CONTROL: "Naval and damage control",
  FIELD_STORES: "Field stores",
  TOOLS_EQUIPMENT: "Tools and equipment",
  OTHER: "Other",
};

export function LoadReferenceLookupPanel() {
  const { user, loading, getIdToken } = useAuth();

  const [items, setItems] = useState<LoadReferenceItem[]>([]);
  const [note, setNote] = useState("");
  const [search, setSearch] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [errorMessage, setErrorMessage] = useState("");

  useEffect(() => {
    async function load() {
      if (loading || !user) {
        return;
      }

      setIsLoading(true);
      setErrorMessage("");

      try {
        const token = await getIdToken();

        if (!token) {
          setErrorMessage("No Firebase ID token is available. Please sign in again.");
          return;
        }

        const response = await getLoadReferenceItems(token);
        setItems(response.items);
        setNote(response.note);
      } catch (error) {
        setErrorMessage(
          error instanceof Error ? error.message : "Could not load the reference library.",
        );
      } finally {
        setIsLoading(false);
      }
    }

    load();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [loading, user]);

  const filtered = useMemo(() => {
    const term = search.trim().toLowerCase();

    if (!term) {
      return items;
    }

    return items.filter(
      (item) =>
        item.itemName.toLowerCase().includes(term) ||
        (item.serviceContext ?? "").toLowerCase().includes(term) ||
        (categoryLabels[item.category] ?? item.category).toLowerCase().includes(term),
    );
  }, [items, search]);

  return (
    <section className="rounded-2xl border border-white/10 bg-white/5 p-8">
      <p className="text-sm font-semibold uppercase tracking-[0.25em] text-cyan-300">
        Equipment weight reference
      </p>

      <h2 className="mt-4 text-2xl font-bold">RAN/ADF equipment weight library</h2>

      <p className="mt-3 max-w-3xl text-sm leading-6 text-slate-300">
        Approximate typical weights to help you fill in the load exposure builder. Reference
        guidance only, not a measurement of your service.
      </p>

      {note && (
        <div className="mt-6 rounded-xl border border-yellow-300/30 bg-yellow-300/10 p-4 text-sm leading-6 text-yellow-100">
          {note}
        </div>
      )}

      <input
        type="text"
        value={search}
        onChange={(event) => setSearch(event.target.value)}
        placeholder="Search items, categories or context"
        className="mt-6 w-full rounded-xl border border-white/10 bg-slate-900 px-4 py-3 text-white outline-none focus:border-cyan-300"
      />

      {errorMessage && (
        <div className="mt-4 rounded-xl border border-red-300/30 bg-red-300/10 p-4 text-sm text-red-100">
          {errorMessage}
        </div>
      )}

      {isLoading ? (
        <p className="mt-6 text-slate-300">Loading reference library...</p>
      ) : filtered.length === 0 ? (
        <p className="mt-6 text-slate-300">
          {items.length === 0
            ? "No reference items are loaded. An admin can run the reference seed."
            : "No items match your search."}
        </p>
      ) : (
        <div className="mt-6 overflow-x-auto">
          <table className="w-full min-w-[640px] border-collapse text-left text-sm">
            <thead className="text-xs uppercase tracking-wide text-slate-400">
              <tr>
                <th className="border-b border-white/10 py-3 pr-4">Item</th>
                <th className="border-b border-white/10 py-3 pr-4">Category</th>
                <th className="border-b border-white/10 py-3 pr-4">Typical (kg)</th>
                <th className="border-b border-white/10 py-3 pr-4">Range (kg)</th>
                <th className="border-b border-white/10 py-3">Context / notes</th>
              </tr>
            </thead>
            <tbody>
              {filtered.map((item) => (
                <tr key={item.id} className="align-top text-slate-300">
                  <td className="border-b border-white/5 py-3 pr-4 font-medium text-white">
                    {item.itemName}
                  </td>
                  <td className="border-b border-white/5 py-3 pr-4">
                    {categoryLabels[item.category] ?? item.category}
                  </td>
                  <td className="border-b border-white/5 py-3 pr-4">
                    {item.typicalWeightKg ?? "—"}
                  </td>
                  <td className="border-b border-white/5 py-3 pr-4">{item.weightRangeKg ?? "—"}</td>
                  <td className="border-b border-white/5 py-3">
                    {[item.serviceContext, item.notes].filter(Boolean).join(" — ") || "—"}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}
    </section>
  );
}
