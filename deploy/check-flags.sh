#!/bin/bash
# Lista as flags gravadas em /tmp dentro do container, com conteúdo.
set -euo pipefail
docker exec vaultbank sh -c 'for f in /tmp/*.txt; do [ -e "$f" ] || continue; echo "=== $f ==="; cat "$f"; echo; done'
