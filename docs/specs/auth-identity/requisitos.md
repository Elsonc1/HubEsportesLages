# Requisitos — Autenticação com ASP.NET Core Identity (Onda 1 · segurança/gov)

> O "o quê" e o "porquê". Independente de stack. Esta seção é a fonte da verdade da feature.

## Contexto
A autenticação atual é de demonstração: o login aceita **qualquer** usuário com senha
de 6+ caracteres e a role (`Admin`/`Torcedor`) é decidida por uma lista fixa de e-mails
em código (`AdminsConhecidos`), além de um par fixo `admin`/`lages2026` na área do
organizador. Nada é persistido, nenhuma senha é guardada com hash e não há proteção
contra força bruta. Para a Onda 1 (segurança/governança) precisamos de identidade real,
com senha forte, hash, lockout e **roles persistidas no banco**, mantendo todas as
features atuais funcionando.

## User stories
- Como **torcedor**, quero **criar uma conta com senha forte e fazer login com segurança**,
  para **acompanhar eventos, comprar ingressos e interagir com a torcida** com a minha
  identidade protegida.
- Como **organizador (Admin)**, quero **entrar com credenciais reais validadas pelo Identity**,
  para **publicar eventos e validar ingressos** sem usar senha fixa compartilhada.
- Como **responsável pela governança (Fundação Municipal de Esportes)**, quero **as roles e o
  usuário administrador inicial garantidos no banco**, para **controlar quem tem acesso de Admin**.
- Como **plataforma**, quero **bloquear contas após várias tentativas erradas**, para **mitigar
  ataques de força bruta**.

## Critérios de aceite (testáveis)
- [x] Dado um cadastro novo, quando a senha não atende à política forte (mín. 8, com maiúscula,
  minúscula, dígito e caractere especial), então o cadastro é recusado e os erros aparecem no formulário.
- [x] Dado um cadastro válido, quando concluído, então o usuário é persistido com **hash de senha**,
  recebe a role **Torcedor** e dispara o e-mail de boas-vindas.
- [x] Dado um usuário existente, quando informa a senha correta, então é autenticado via cookie do Identity.
- [x] Dado um usuário existente, quando erra a senha 5 vezes, então a conta é **bloqueada temporariamente** (lockout).
- [x] Dado o e-mail `elsouzalopes@gmail.com`, quando o banco é inicializado, então existe esse usuário
  com a role **Admin** e uma senha inicial forte (configurável; default forte).
- [x] Dado o RBAC atual, quando o banco é recriado, então `AdminController` segue exigindo role **Admin**;
  `Notificacoes`/`Torcida`/`Ingressos` exigem autenticação; e o `_Layout` mostra os links por role.
- [x] Dado o banco recriado do zero, quando a app sobe, então as migrations são aplicadas e as tabelas
  do Identity (AspNet*) coexistem com as tabelas de domínio.

## Fora de escopo
- Confirmação de e-mail / fluxo de recuperação de senha (RequireConfirmedAccount = false nesta onda).
- Tela de gestão de usuários/roles (a elevação para Admin é feita por seed/governança).
- Login social / 2FA.
- Autenticação da API por token (JWT) — permanece o cabeçalho anônimo `X-Torcedor-Id` da torcida.
