"use client";

import { useMutation } from "@tanstack/react-query";
import { KeyRound } from "lucide-react";
import { useState } from "react";
import { PowerLensApi } from "@/lib/api";

export default function LoginPage() {
  const [mode, setMode] = useState<"login" | "register">("login");
  const [nome, setNome] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  const auth = useMutation({
    mutationFn: () => (mode === "login" ? PowerLensApi.login(email, password) : PowerLensApi.register(nome, email, password)),
    onSuccess: (response) => {
      window.localStorage.setItem("powerlens.token", response.token);
      window.location.href = "/";
    }
  });

  return (
    <main className="grid min-h-screen place-items-center px-6">
      <section className="w-full max-w-md rounded-md border border-line bg-white p-6">
        <div className="mb-6 flex items-center gap-3">
          <div className="grid h-10 w-10 place-items-center rounded-md bg-mint text-moss">
            <KeyRound size={20} />
          </div>
          <div>
            <h1 className="text-xl font-bold">PowerLens</h1>
            <p className="text-sm text-ink/65">Acesse seu workspace de documentacao.</p>
          </div>
        </div>

        <div className="mb-4 grid grid-cols-2 rounded-md border border-line p-1 text-sm">
          <button className={`rounded px-3 py-2 ${mode === "login" ? "bg-moss text-white" : ""}`} onClick={() => setMode("login")}>
            Login
          </button>
          <button className={`rounded px-3 py-2 ${mode === "register" ? "bg-moss text-white" : ""}`} onClick={() => setMode("register")}>
            Criar conta
          </button>
        </div>

        <form
          className="space-y-3"
          onSubmit={(event) => {
            event.preventDefault();
            auth.mutate();
          }}
        >
          {mode === "register" ? <input className="w-full rounded-md border border-line px-3 py-2 text-sm" placeholder="Nome" value={nome} onChange={(event) => setNome(event.target.value)} /> : null}
          <input className="w-full rounded-md border border-line px-3 py-2 text-sm" placeholder="Email" value={email} onChange={(event) => setEmail(event.target.value)} />
          <input className="w-full rounded-md border border-line px-3 py-2 text-sm" placeholder="Senha" type="password" value={password} onChange={(event) => setPassword(event.target.value)} />
          {auth.error ? <p className="rounded-md bg-red-50 px-3 py-2 text-sm text-red-700">Nao foi possivel autenticar com estes dados.</p> : null}
          <button className="w-full rounded-md bg-moss px-4 py-2 font-medium text-white" disabled={auth.isPending}>
            {auth.isPending ? "Entrando..." : mode === "login" ? "Entrar" : "Criar conta"}
          </button>
        </form>
      </section>
    </main>
  );
}
