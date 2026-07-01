# Tarefas — Autenticação com ASP.NET Core Identity

> Quebra acionável. Marque ao concluir. "Done" = critérios de aceite de `requisitos.md` atendidos.

- [x] T1 — Adicionar pacote `Microsoft.AspNetCore.Identity.EntityFrameworkCore` e `FrameworkReference`
  do ASP.NET Core (`src/HubEsportesLages.Infrastructure/HubEsportesLages.Infrastructure.csproj`)
- [x] T2 — Criar `ApplicationUser : IdentityUser` com `NomeCompleto`
  (`src/HubEsportesLages.Infrastructure/Identidade/ApplicationUser.cs`)
- [x] T3 — `HubDbContext` herda `IdentityDbContext<ApplicationUser>` e chama `base.OnModelCreating` primeiro
  (`src/HubEsportesLages.Infrastructure/Persistence/HubDbContext.cs`)
- [x] T4 — Registrar `AddIdentity` com senha forte + lockout + EF stores + token providers
  (`src/HubEsportesLages.Infrastructure/DependencyInjection.cs`)
- [x] T5 — `IdentidadeSeeder`: roles `Admin`/`Torcedor` e admin `elsouzalopes@gmail.com` (senha de config)
  (`src/HubEsportesLages.Infrastructure/Identidade/IdentidadeSeeder.cs`)
- [x] T6 — `InicializarBancoAsync`: `MigrateAsync()` + seed de identidade + `DataSeeder`
  (`src/HubEsportesLages.Infrastructure/DependencyInjection.cs`)
- [x] T7 — `Program.cs`: remover cookie custom, usar `ConfigureApplicationCookie` (login/logout/access-denied + redirect /admin)
  (`src/HubEsportesLages.Web/Program.cs`)
- [x] T8 — Reescrever `ContaController` (Login/Registrar/Logout via SignInManager/UserManager, role Torcedor, e-mail)
  (`src/HubEsportesLages.Web/Controllers/ContaController.cs`)
- [x] T9 — Reescrever login/registro/logout do `AdminController` via Identity, remover `admin`/`lages2026`
  (`src/HubEsportesLages.Web/Controllers/AdminController.cs`)
- [x] T10 — Adicionar `Microsoft.EntityFrameworkCore.Design` ao projeto Web e instalar tool `dotnet-ef`
  (`src/HubEsportesLages.Web/HubEsportesLages.Web.csproj`)
- [x] T11 — Gerar migration `InicialIdentity` e apagar `hubesportes.db*` (recriar do zero)
  (`src/HubEsportesLages.Infrastructure/Migrations/`)
- [x] T12 — `dotnet build HubEsportesLages.slnx` verde
- [ ] Testes cobrindo os critérios de aceite (validação manual prevista ao subir a app; sem suíte automatizada nesta onda)
- [x] Spec e código revisados (não divergem)
