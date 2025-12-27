import Link from "next/link";

export default function HomePage() {
  return (
    <main className="page">
      <section className="card">
        <h1>Portal Web</h1>
        <p>Autenticación via Keycloak y API protegida.</p>
        <ul>
          <li>
            <strong>OIDC issuer:</strong> {process.env.NEXT_PUBLIC_OIDC_ISSUER ?? "not set"}
          </li>
          <li>
            <strong>Client Id:</strong> {process.env.NEXT_PUBLIC_OIDC_CLIENT_ID ?? "not set"}
          </li>
          <li>
            <strong>API base:</strong> {process.env.NEXT_PUBLIC_API_BASE_URL ?? "not set"}
          </li>
        </ul>
        <Link href="/api/health">Health</Link>
      </section>
    </main>
  );
}

