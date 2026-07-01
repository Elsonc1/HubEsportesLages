# Design — Autenticação JWT Bearer da API

> O "como". Referencie camadas e contratos concretos do repo.

## Visão geral
O JWT é **adicional** ao cookie do Identity, não um substituto. `AddIdentity` continua registrando o
cookie (`IdentityConstants.ApplicationScheme`) como esquema **padrão** do site MVC. Chamamos
`AddAuthentication().AddJwtBearer(...)` — a sobrecarga **sem** argumento **não troca** o esquema padrão,
apenas **adiciona** o esquema `Bearer`. Uma **política de autorização padrão dupla** faz os `[Authorize]`
existentes aceitarem os dois esquemas.

## Backend (.NET 10)

### Domain
- Sem mudanças. Identidade/tokens vivem na Infrastructure/Web, não são entidade de domínio do esporte.

### Application
- Sem novos DTOs/interfaces na camada Application. Seguindo a decisão de `auth-identity` (usar
  `UserManager`/`SignInManager` direto nos controllers), os contratos de request/response da autenticação
  ficam como `record`s **locais** ao `AuthApiController` (`LoginRequisicaoDto`, `RegistrarRequisicaoDto`,
  `LoginRespostaDto`). A API responde em **camelCase** (mesma serialização das demais rotas).

### Infrastructure
- Sem mudanças. Reusa `ApplicationUser`, `UserManager`, `SignInManager`, roles e o lockout já configurados
  em `DependencyInjection.AddInfrastructure`.

### Web
- **Pacote**: `Microsoft.AspNetCore.Authentication.JwtBearer` **10.0.9** (alinhado ao demais 10.0.9 do repo),
  em `src/HubEsportesLages.Web/HubEsportesLages.Web.csproj`.
- **`Identidade/JwtSettings.cs`** — POCO da seção `Jwt`: `Issuer` (default `BoraProJogo`),
  `Audience` (default `BoraProJogoApp`), `SecretKey` (vazia no arquivo versionado), `ExpiraMinutos`
  (default **120**). Constantes `SectionName = "Jwt"` e `TamanhoMinimoChave = 32`.
- **`appsettings.json`** — seção `Jwt` com `Issuer`/`Audience`/`ExpiraMinutos` preenchidos e
  `SecretKey: ""` (placeholder). Comentário `//Jwt` explica a origem da chave.
- **`Program.cs`** (após `ConfigureApplicationCookie`, antes do resto do pipeline de serviços):
  - Lê `JwtSettings` da config. **Resolução da chave**:
    - `SecretKey` vazia **em Development** → usa um **fallback longo** de dev e loga `Warning`.
    - `SecretKey` vazia **em produção** → lança `InvalidOperationException` (exige env `Jwt__SecretKey`).
    - Chave com menos de 32 bytes (UTF-8) → `InvalidOperationException` (exigência do HMAC-SHA256).
  - Registra `JwtSettings` (já com a chave resolvida) como **singleton** para o `AuthApiController`.
  - `AddAuthentication().AddJwtBearer(...)` com `TokenValidationParameters`:
    `ValidateIssuer/Audience/Lifetime/IssuerSigningKey = true`, `ValidIssuer/ValidAudience` das settings,
    `IssuerSigningKey = SymmetricSecurityKey(UTF8(SecretKey))`, `ClockSkew = 30s` (curto).
  - `AddAuthorization` define `DefaultPolicy` via
    `new AuthorizationPolicyBuilder(IdentityConstants.ApplicationScheme, JwtBearerDefaults.AuthenticationScheme)
    .RequireAuthenticatedUser().Build()` — faz os `[Authorize]` aceitarem cookie **e** JWT. Roles seguem
    funcionando via `[Authorize(Roles="Admin")]`.
  - **`AddSwaggerGen`**: `AddSecurityDefinition("Bearer", ...)` (`Type=Http`, `Scheme=bearer`,
    `BearerFormat=JWT`, `In=Header`) + `AddSecurityRequirement(doc => ...)` referenciando o esquema
    `Bearer`. **Nota de versão**: Swashbuckle 10.2.2 usa **Microsoft.OpenApi 2.7.5** — os tipos ficam no
    namespace `Microsoft.OpenApi` (sem `.Models`), o requirement é keyed por `OpenApiSecuritySchemeReference`
    e `AddSecurityRequirement` recebe um **`Func<OpenApiDocument, OpenApiSecurityRequirement>`**.
  - `UseAuthentication`/`UseAuthorization` já existiam — sem mudança de ordem no pipeline.
- **`Controllers/Api/AuthApiController.cs`** (`[ApiController]`, rota `api/auth`, `[Tags("Autenticação")]`):
  - **`POST /api/auth/login`** `{ email, senha }`: resolve o usuário por e-mail **ou** username;
    valida com `SignInManager.CheckPasswordSignInAsync(user, senha, lockoutOnFailure: true)` (respeita
    lockout); em sucesso monta o JWT e retorna `{ token, expiraEm, nome, roles }`. `IsLockedOut` e
    credencial inválida → **401**. `[ProducesResponseType]` para 200/401.
  - **`POST /api/auth/registrar`** `{ nome, email, senha }` (opcional, espelha o site): `UserManager.CreateAsync`
    (política forte validada), `AddToRoleAsync(user, "Torcedor")`, devolve já o token. Erros de validação → **400**.
  - **`GerarToken`**: claims `sub`(=UserId), `jti`, `email`, `ClaimTypes.NameIdentifier`(=UserId),
    `ClaimTypes.Name`(=`NomeCompleto`/`UserName`) e uma claim `ClaimTypes.Role` por role
    (`UserManager.GetRolesAsync`). Assina com `SymmetricSecurityKey` + `HmacSha256`; `expires = UtcNow + ExpiraMinutos`.

## API (contrato camelCase)
- `POST /api/auth/login` → 200 `{ token, expiraEm, nome, roles[] }` · 401 `{ mensagem }`.
- `POST /api/auth/registrar` → 200 `{ token, expiraEm, nome, roles[] }` · 400 `{ mensagem, erros[] }`.
- Endpoints já existentes com `[Authorize]`/`[Authorize(Roles=...)]` passam a aceitar o header
  `Authorization: Bearer <token>` além do cookie — **sem alteração de assinatura**.

## Mobile (Arena Lages, MAUI)
- O `HttpClient` tipado guarda o `token` do login e o envia como `Authorization: Bearer` nas chamadas
  protegidas; renova fazendo novo `login` quando `expiraEm` passar (sem refresh token nesta onda).
- `BaseUrl` conforme AGENTS §2 (`10.0.2.2:5210` no emulador, `localhost:5210` no Windows).

## Decisões e trade-offs / riscos
- **Adicionar, não trocar o esquema**: `AddAuthentication()` sem default preserva o cookie do site; a
  política dupla evita ter que anotar cada controller com `AuthenticationSchemes=...`.
- **Chave só em env na produção**: `appsettings.json` fica sem segredo; o fallback de dev é logado como
  aviso e nunca deve ir para produção (bloqueado por `IsDevelopment()`).
- **Sem refresh token**: simplicidade na Onda 1; validade de 120 min é suficiente para a demonstração.
  Rotação/revogação ficam para uma onda futura.
- **Records locais no controller**: mantém a Application enxuta (consistente com `auth-identity`); se o
  mobile crescer, promover para `Application/DTOs` é trivial.
- **Acoplamento de versão do OpenApi**: a montagem do requirement depende da API v2 do Microsoft.OpenApi
  (2.7.5 via Swashbuckle 10.2.2); documentado acima para evitar regressão em upgrades.
