# Design — Autenticação com ASP.NET Core Identity

> O "como". Referencie camadas e contratos concretos do repo.

## Backend (.NET 10)

### Domain
- Sem mudanças. O usuário de identidade vive na Infrastructure (não é entidade de domínio do esporte).

### Application
- Sem novos DTOs/interfaces. O fluxo usa `UserManager`/`SignInManager` direto nos controllers Web,
  e o `IEmailService` existente para o e-mail de boas-vindas.

### Infrastructure
- **Pacote**: `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (10.0.9) no projeto Infrastructure.
  Como o projeto é uma class library (`Microsoft.NET.Sdk`), foi adicionado
  `<FrameworkReference Include="Microsoft.AspNetCore.App" />` para expor `AddIdentity`/`UserManager`.
- **`Identidade/ApplicationUser.cs`** — `ApplicationUser : IdentityUser` com campo extra opcional `NomeCompleto`.
- **`Persistence/HubDbContext.cs`** — passa a herdar `IdentityDbContext<ApplicationUser>`. Em
  `OnModelCreating` chama **`base.OnModelCreating(builder)` PRIMEIRO** e mantém toda a Fluent API
  existente do domínio (incluindo os `Ignore` das propriedades calculadas). Todos os DbSets atuais permanecem.
- **`DependencyInjection.cs`**:
  - `AddIdentity<ApplicationUser, IdentityRole>(...)` com **política de senha forte**
    (RequiredLength=8, RequireDigit, RequireUppercase, RequireLowercase, RequireNonAlphanumeric),
    **Lockout** habilitado (5 tentativas, 15 min), `RequireConfirmedAccount=false`, `RequireUniqueEmail=true`.
  - `.AddEntityFrameworkStores<HubDbContext>().AddDefaultTokenProviders()`.
  - `InicializarBancoAsync` troca `EnsureCreatedAsync` por **`MigrateAsync()`**; depois roda
    `IdentidadeSeeder.SeedAsync(...)` e por fim o `DataSeeder` demonstrativo.
- **`Identidade/IdentidadeSeeder.cs`** — garante as roles **`Admin`** e **`Torcedor`**; garante o usuário
  **`elsouzalopes@gmail.com`** com role **Admin** e senha lida de `Admin:SenhaInicial`
  (default forte **`Admin@Lages2026`**). A senha usada é logada uma única vez no momento da criação.

### Web
- **`Program.cs`**: remove o `AddAuthentication(...).AddCookie(...)` custom. O cookie passa a ser o do
  Identity (registrado em `AddInfrastructure`), ajustado por **`ConfigureApplicationCookie`**:
  `LoginPath="/conta/login"`, `LogoutPath="/conta/logout"`, `AccessDeniedPath="/conta/login"`, expiração 2h,
  e eventos `OnRedirectToLogin`/`OnRedirectToAccessDenied` que mandam `/admin/...` para `/admin/login`.
  `UseAuthentication`/`UseAuthorization` permanecem.
- **`ContaController`** (torcedor): `Login` usa `SignInManager.PasswordSignInAsync(..., lockoutOnFailure: true)`
  (aceita e-mail ou username); `Registrar` usa `UserManager.CreateAsync` (política validada pelo Identity,
  erros traduzidos no `ModelState`), `AddToRoleAsync(user, "Torcedor")` e e-mail de boas-vindas;
  `Logout` usa `SignInManager.SignOutAsync`.
- **`AdminController`** (organizador): remove o par fixo `admin`/`lages2026`. `Login` valida via Identity e
  exige que o usuário esteja na role **Admin**; `Registrar` cria conta **Torcedor** (sem auto-elevação);
  `Logout` usa `SignOutAsync`. A classe segue com `[Authorize(Roles="Admin")]`.
- **`_Layout.cshtml`**, `NotificacoesController`, `TorcidaController`, `IngressosController`: sem mudanças —
  o RBAC por `[Authorize]`/`User.IsInRole(...)` continua funcionando sobre o principal do Identity.
- **`Web.csproj`**: adiciona `Microsoft.EntityFrameworkCore.Design` (necessário no projeto de startup
  para a tooling `dotnet ef`).

### EF Migrations
- Tooling `dotnet-ef` 10.0.9 (instalável via `dotnet tool install --global dotnet-ef`).
- Migration inicial **`InicialIdentity`** em `src/HubEsportesLages.Infrastructure/Migrations/` cobrindo
  as 7 tabelas `AspNet*` + todas as tabelas de domínio (21 no total). O `hubesportes.db` é apagado para
  recriar do zero; `MigrateAsync` recria na próxima execução.

## API (contrato camelCase)
- Sem alterações de contrato nesta onda. As rotas web de auth (`/conta/*`, `/admin/*`) permanecem.

## Mobile (Arena Lages, MAUI)
- Sem impacto direto agora. A autenticação por token para o mobile fica para uma onda futura
  (hoje o mobile só consome endpoints públicos da API).

## Decisões e trade-offs / riscos
- **Username = e-mail**: simplifica o login e mantém `User.Identity.Name` (usado em Ingressos/torcida)
  como identificador estável.
- **Sem confirmação de e-mail** nesta onda para não travar o fluxo de demonstração; previsto para depois.
- **Senha inicial do admin no log**: aceitável só em dev; a senha real deve vir de `Admin:SenhaInicial`
  (variável de ambiente / user-secrets) e ser trocada no primeiro acesso.
- **FrameworkReference na Infrastructure**: acopla a camada ao ASP.NET Core; aceito por ser onde o
  Identity (UserManager/roles) é configurado, mantendo o registro de DI junto dos demais serviços.
