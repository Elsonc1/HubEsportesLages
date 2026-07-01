# Requisitos — Autenticação JWT Bearer da API (Onda 1 · item 3)

> O "o quê" e o "porquê". Independente de stack. Esta seção é a fonte da verdade da feature.

## Contexto
A Onda 1 (feature `auth-identity`) introduziu identidade real com ASP.NET Core Identity: usuários
persistidos no Postgres, senha com hash, política forte, lockout e roles `Admin`/`Torcedor`. O
`AddIdentity<ApplicationUser, IdentityRole>()` configura o **cookie** (`IdentityConstants.ApplicationScheme`)
como esquema padrão — perfeito para o **site MVC**, onde o login usa `SignInManager` e o navegador
carrega o cookie automaticamente.

O app mobile **Arena Lages** (MAUI) consome a **API REST** por `HttpClient` e **não usa cookies**:
precisa de um token portável enviado no cabeçalho `Authorization: Bearer <token>`. Este item adiciona
**JWT Bearer** à API **sem** remover o cookie do site: o mesmo `[Authorize]` dos controllers de API passa
a aceitar **os dois esquemas** (cookie para o site, JWT para o mobile), com as roles continuando a valer.

## User stories
- Como **app mobile (torcedor)**, quero **trocar e-mail/senha por um token JWT**, para **acessar os
  endpoints protegidos da API** (ingressos, favoritos, interação) sem depender de cookies.
- Como **torcedor**, quero que meu **token carregue minhas roles**, para que a API aplique o mesmo
  controle de acesso do site (ex.: `[Authorize(Roles="Admin")]`).
- Como **plataforma**, quero **manter o login por cookie do site MVC intacto**, para **não regредir**
  nenhuma tela ou fluxo já existente do organizador/torcedor no navegador.
- Como **responsável pela segurança**, quero que a **chave de assinatura venha de variável de ambiente
  em produção**, para **não versionar segredo** — e que o token respeite o **lockout** do Identity.

## Critérios de aceite (testáveis)
- [x] Dado `POST /api/auth/login` com e-mail e senha corretos, então retorna **200** com
  `{ token, expiraEm, nome, roles }` e o token é um JWT assinado (HMAC-SHA256).
- [x] Dado `POST /api/auth/login` com credenciais inválidas ou usuário inexistente, então retorna **401**.
- [x] Dado um usuário que errou a senha além do limite, quando tenta o login pela API, então recebe **401**
  de conta **bloqueada** (o `CheckPasswordSignInAsync` respeita o lockout do Identity).
- [x] Dado o token retornado no cabeçalho `Authorization: Bearer <token>`, quando chama um endpoint
  `[Authorize]` da API, então é autenticado (esquema JWT) sem precisar de cookie.
- [x] Dado um usuário Admin, quando usa o token em um endpoint `[Authorize(Roles="Admin")]`, então é
  autorizado (as roles vão no token como `ClaimTypes.Role`).
- [x] Dado o site MVC, quando o usuário faz login por formulário, então o cookie do Identity continua
  autenticando os `[Authorize]` (o esquema padrão do site não muda).
- [x] Dado o arquivo `appsettings.json` versionado, então `Jwt:SecretKey` está **vazia**; em Development
  a app usa um **fallback de dev** (>=32 chars) com **aviso no log**; em produção a ausência da chave
  (env `Jwt__SecretKey`) é erro de inicialização.
- [x] Dado o Swagger, então há o esquema **Bearer** (Authorize) para testar os endpoints com o token.

## Fora de escopo
- **Refresh token** / rotação e revogação de tokens (validade fixa por `Jwt:ExpiraMinutos`, default 120).
- Login social / 2FA / confirmação de e-mail (herdados do escopo de `auth-identity`).
- Autenticação/registro de dispositivos (device binding) e claims customizadas além de sub/email/name/roles.
- Emissão de token para o cabeçalho anônimo `X-Torcedor-Id` da torcida (segue anônimo, sem token).
