import React from "react";

export default function 04_iGRC_25() {
  return (
    <div className="min-h-screen bg-slate-50">
      <header className="sticky top-0 bg-white border-b border-slate-200 py-3 px-6 flex items-center justify-between">
        <div className="text-sm text-slate-600">07/11/2025, 10:05 • EN/EL • PLATINUM</div>
        <div className="text-sm font-semibold">Skyworks AI Suite</div>
      </header>

      <div className="grid grid-cols-12 gap-6 p-6 max-w-7xl mx-auto">
        <aside className="col-span-12 md:col-span-3 xl:col-span-3">
          <nav className="space-y-2">
            {["Dashboard","Mission Planner","Drone Library","Airspace & Maps","GRC Engine","ARC Engine","SAIL & OSOs","Templates","Reports","Knowledge Base","AI Training","Subscriptions","Settings","Audit & Logs"].map((x)=> (
              <div key={x} className="rounded-xl border border-slate-200 bg-white px-3 py-2 text-sm">{x}</div>
            ))}
          </nav>
        </aside>

        <main className="col-span-12 md:col-span-9 xl:col-span-9 space-y-6">
          <h1 className="text-2xl font-semibold tracking-tight">2. iGRC (SORA 2.5) — Πληθυσμιακή Πυκνότητα & UA</h1>
          {/* TODO: Replace with the exact widgets from the spec for this screen */}
          <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-3 gap-4">
            <div className="rounded-2xl border border-slate-200 bg-white p-4 min-h-[120px]">Card A</div>
            <div className="rounded-2xl border border-slate-200 bg-white p-4 min-h-[120px]">Card B</div>
            <div className="rounded-2xl border border-slate-200 bg-white p-4 min-h-[120px]">Card C</div>
          </div>
        </main>
      </div>
    </div>
  );
}