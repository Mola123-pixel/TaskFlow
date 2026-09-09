Task 3

Solution: Operationalisation, Azure, and Mobile (Design Notes)

1) Business needs (summary)
- Deploy API and web app to Azure App Services for managed hosting, auto-scaling, and CI/CD integration.
- Secure configuration and secrets management, staged rollout (staging -> production) and simple rollback.
- Authentication via Azure AD / Microsoft Entra ID for both API and web front end.
- Mobile packaging via Capacitor for Android (priority) and optional iOS; capture device context on updates for diagnostics.
- Prefer a local-first data approach (SQLite) for mobile and small deployments.

2) Azure App Services deployment approach
- App model
  - Two App Service instances (or App Service Plans): one for the API (ASP.NET Core) and one for the web UI (static SPA or server-rendered).
  - Use separate slots per app: production + staging slot for safe rollouts.

- Environment configuration
  - Use App Settings (application settings) per App Service for non-secret configuration (connection strings, feature flags).
  - Use separate App Service configurations for environments (dev, staging, prod) via configuration slots or separate resource groups/subscriptions.

- Handling secrets
  - Store secrets (DB connection strings, API keys, certificates) in Azure Key Vault.
  - Grant App Services managed identity access to Key Vault (Key Vault access policies or RBAC) and inject secrets via Key Vault references in App Settings.
  - Use Managed Identity for resource access (SQL, Storage, Key Vault) to avoid embedded credentials.

- Rollout strategy (simple flow)
  - Developer pushes to main -> CI builds -> deploys to staging slot.
  - QA smoke tests against staging slot (automated/end-to-end tests) -> swap slots to promote to production.
  - Rollback: swap back to previous slot or redeploy last stable build.
  - Diagram (textual): Git -> CI -> Staging slot -> QA tests -> Slot swap -> Production

3) High-level CI/CD approach (stages and key steps)
- Stages
  - Build: compile, run unit tests, build artifacts (API package, front-end bundle), publish build artifact.
  - Scan: static analysis, security scans, dependency checks (optional but recommended).
  - Deploy-Staging: deploy artifact to App Service staging slot; run integration tests and smoke tests.
  - Approve: manual or automated gating after test pass.
  - Promote: slot swap to production or deploy to production slot.
  - Monitor & Rollback: health checks; automated rollback on failure.

- Key steps
  - Use pipeline YAML (Azure Pipelines or GitHub Actions): restore, build, test, publish artifact, deploy to App Service using built-in tasks or az webapp commands.
  - Use environment secrets from Azure Key Vault via pipeline integration.
  - Use feature flags (App Configuration or LaunchDarkly) for controlled feature rollout.

4) Authentication with Azure AD / Microsoft Entra ID
- Web app (SPA or server-rendered)
  - Register a single-page or web application in Entra ID; configure redirect URIs for web and localhost for dev.
  - Use MSAL.js (for SPA) or Microsoft.Identity.Web (for server apps) to sign in users and obtain ID tokens.
  - Protect routes and acquire access tokens for API calls.

- API
  - Register API as an app in Entra ID; define scopes/roles (API permissions).
  - Validate incoming JWT access tokens (issuer, audience, signature) using middleware (e.g., Microsoft.Identity.Web or JwtBearer in ASP.NET Core).
  - Use role-based or scope-based authorization for endpoints.

- Key operational notes
  - Use separate app registrations for web and API; grant the web app permission to call the API.
  - Use managed identity for server-to-server auth to Azure resources.

5) Mobile path (Capacitor) and device context capture
- Packaging with Capacitor
  - Wrap the existing web app in an Ionic/Capacitor shell to produce Android and iOS projects.
  - Keep web app as the single source of UI; Capacitor builds native shells and webview-based app.

- Device context capture (on updates)
  - On app startup and on app update events, capture device context: OS, OS version, device model, app version, locale, network status.
  - Use native Capacitor plugins or community plugins (Capacitor Device plugin) to obtain device details (device.model, device.platform, device.osVersion).
  - Persist context locally (SQLite) and include in telemetry events sent to backend for diagnostics.

- Release approach
  - Android-first local validation
	- Use Capacitor to build Android (Gradle) and run on emulator or local device for quick validation.
	- Use internal testing track (Google Play Console) for staged release after local validation.
  - iOS (optional)
	- Build via Xcode, test on simulator and TestFlight for distribution.
	- iOS requires macOS build environment; automate via cloud builders or GitHub Actions macOS runners.

6) Storage and local-first guidance
- Mobile local-first
  - Use SQLite (via a Capacitor plugin or native driver) for offline-first storage and a sync strategy to the API when online.
  - Keep status history truncated locally to recent subset (e.g., last N entries) to limit storage.

- Server-side
  - Use Azure SQL for production; keep EF models aligned with DB using migrations.

7) Minimal functional scope recommendations
- Keep search simple: a single server-side text search endpoint (e.g., filter by title or assignedTo) with optional pagination.
- Limit status history returned by API to most recent M entries (configurable) and provide endpoint for full history if needed.

8) Monitoring & operations
- Application Insights for telemetry, traces, and alerts (integrated with App Services).
- Configure health probes and autoscale rules based on CPU/requests/queue length.
- Use Log Analytics / Alerts for error budgets and incident routing.

9) Security & compliance notes
- Enforce HTTPS, HSTS, and secure cookie settings.
- Use Key Vault for certs and secrets; rotate secrets and manage access via RBAC.
- Least-privilege for managed identities and DB credentials.

10) Rollout checklist
- CI green, unit/integration tests passed.
- Staging deployed and smoke-tested.
- Feature flags off until verified.
- Manual approval for production swap.

End of design notes.