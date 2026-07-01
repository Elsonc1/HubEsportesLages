# Tarefas — Autenticação JWT Bearer da API

> Quebra acionável. Marque ao concluir. "Done" = critérios de aceite de `requisitos.md` atendidos.

- [x] T1 — Adicionar pacote `Microsoft.AspNetCore.Authentication.JwtBearer` **10.0.9**
  (`src/HubEsportesLages.Web/HubEsportesLages.Web.csproj`)
- [x] T2 — Criar POCO `JwtSettings` (Issuer/Audience/SecretKey/ExpiraMinutos + `SectionName`/`TamanhoMinimoChave`)
  (`src/HubEsportesLages.Web/Identidade/JwtSettings.cs`)
- [x] T3 — Adicionar a seção `Jwt` no `appsettings.json` com `SecretKey` **vazia** (placeholder) e comentário
  (`src/HubEsportesLages.Web/appsettings.json`)
- [x] T4 — `Program.cs`: resolver a `SecretKey` (fallback de dev com aviso; erro em produção; mínimo 32 chars)
  e registrar `JwtSettings` como singleton
  (`src/HubEsportesLages.Web/Program.cs`)
- [x] T5 — `Program.cs`: `AddAuthentication().AddJwtBearer(...)` com `TokenValidationParameters`
  (Issuer/Audience/Lifetime/SigningKey validados, HMAC-SHA256, ClockSkew 30s) — **sem** trocar o padrão cookie
  (`src/HubEsportesLages.Web/Program.cs`)
- [x] T6 — `Program.cs`: `AddAuthorization` com `DefaultPolicy` dupla
  (`IdentityConstants.ApplicationScheme` + `JwtBearerDefaults.AuthenticationScheme`, `RequireAuthenticatedUser`)
  (`src/HubEsportesLages.Web/Program.cs`)
- [x] T7 — `Program.cs`: Swagger `AddSecurityDefinition("Bearer", ...)` + `AddSecurityRequirement(doc => ...)`
  (API v2 do Microsoft.OpenApi 2.7.5)
  (`src/HubEsportesLages.Web/Program.cs`)
- [x] T8 — `AuthApiController`: `POST /api/auth/login` (CheckPasswordSignInAsync com lockout, 401 em erro,
  token com sub/email/name/roles) + `POST /api/auth/registrar` (opcional) + `[ProducesResponseType]`
  (`src/HubEsportesLages.Web/Controllers/Api/AuthApiController.cs`)
- [x] T9 — `dotnet build HubEsportesLages.slnx` verde
- [ ] Testes cobrindo os critérios de aceite (validação manual via Swagger/`curl` prevista ao subir a app;
  sem suíte automatizada nesta onda)
- [x] Spec e código revisados (não divergem)
