#!/bin/bash
# user-data para Amazon Linux 2023 (x86_64).
# Prepara a instância descartável do lab VaultBank: docker, compose, ngrok e swap.
# NÃO baixa o código: a versão atual do lab só existe na máquina do professor e o
# writeup não deve ir pra repo público. O código sobe via rsync (ver deploy/README.md).
# NÃO coloca o authtoken do ngrok aqui: user-data é legível por qualquer usuário da
# instância (e via IMDS). O token entra depois, via SSH, em /etc/ngrok/ngrok.yml.
set -euxo pipefail

APP_DIR="/opt/vaultbank"

# 1) Swap: t3.micro tem 1 GB de RAM e o `dotnet publish` não cabe sem isso.
if [ ! -f /swapfile ]; then
  dd if=/dev/zero of=/swapfile bs=1M count=2048
  chmod 600 /swapfile
  mkswap /swapfile
  swapon /swapfile
  echo '/swapfile none swap sw 0 0' >> /etc/fstab
fi

# 2) Pacotes base
dnf -y update
dnf -y install docker git rsync

mkdir -p /usr/libexec/docker/cli-plugins
curl -fsSL https://github.com/docker/compose/releases/latest/download/docker-compose-linux-x86_64 \
  -o /usr/libexec/docker/cli-plugins/docker-compose
chmod +x /usr/libexec/docker/cli-plugins/docker-compose

systemctl enable --now docker
usermod -aG docker ec2-user

# 3) ngrok (binário estável, sem depender do repo rpm)
curl -fsSL https://bin.equinox.io/c/bNyj1mQVY4c/ngrok-v3-stable-linux-amd64.tgz -o /tmp/ngrok.tgz
tar -xzf /tmp/ngrok.tgz -C /usr/local/bin
chmod +x /usr/local/bin/ngrok
rm -f /tmp/ngrok.tgz

useradd --system --no-create-home --shell /usr/sbin/nologin ngrok || true
mkdir -p /etc/ngrok
chown ngrok:ngrok /etc/ngrok
chmod 700 /etc/ngrok

# 4) Diretório da app, pronto pro rsync do professor
mkdir -p "$APP_DIR"
chown ec2-user:ec2-user "$APP_DIR"

# 5) Serviço do túnel (fica falhando em loop até o token ser gravado — é esperado)
cat > /etc/systemd/system/ngrok.service <<'UNIT'
[Unit]
Description=ngrok tunnel para o lab VaultBank
After=network-online.target docker.service
Wants=network-online.target

[Service]
User=ngrok
Group=ngrok
ExecStart=/usr/local/bin/ngrok start --all --config /etc/ngrok/ngrok.yml
Restart=always
RestartSec=10
NoNewPrivileges=true
PrivateTmp=true
ProtectSystem=strict
ProtectHome=true

[Install]
WantedBy=multi-user.target
UNIT

systemctl daemon-reload
systemctl enable ngrok
