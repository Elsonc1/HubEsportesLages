#!/usr/bin/env bash
# Sobe o PostgreSQL dentro do WSL (Ubuntu) para o Bora pro Jogo / Hub Esportes Lages.
# O WSL2 encaminha localhost, então o app .NET no Windows conecta em localhost:5432.
#
# Uso (a partir do Windows):
#   wsl -d Ubuntu-24.04 -u root -- bash /mnt/c/Users/elson.lopes/source/repos/hubesporteslages/scripts/wsl-postgres-setup.sh
set -e
export DEBIAN_FRONTEND=noninteractive

if ! command -v psql >/dev/null 2>&1; then
  echo "[1/3] instalando postgresql..."
  apt-get update -qq
  apt-get install -y -qq postgresql
fi

echo "[2/3] iniciando o cluster..."
CLUSTER="$(ls /etc/postgresql 2>/dev/null | head -1)"
pg_ctlcluster "$CLUSTER" main start 2>/dev/null || service postgresql start || true
sleep 2

echo "[3/3] configurando usuario/banco..."
sudo -u postgres psql -tAc "ALTER USER postgres PASSWORD 'hub';"
sudo -u postgres psql -tAc "SELECT 1 FROM pg_database WHERE datname='hubesportes'" | grep -q 1 \
  || sudo -u postgres createdb hubesportes

echo "PRONTO: postgres $(sudo -u postgres psql -tAc 'show server_version') | db=hubesportes | listen=$(sudo -u postgres psql -tAc 'show listen_addresses') | porta=$(sudo -u postgres psql -tAc 'show port')"
