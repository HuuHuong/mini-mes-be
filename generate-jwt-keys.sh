#!/bin/bash
# ─────────────────────────────────────────────────────────────────
#  generate-jwt-keys.sh
#  Tự động sinh JWT SecretKey cho môi trường Development và Production
#  và ghi vào file appsettings tương ứng.
#
#  Cách dùng:
#    chmod +x generate-jwt-keys.sh
#    ./generate-jwt-keys.sh
# ─────────────────────────────────────────────────────────────────

set -e

DEV_FILE="appsettings.Development.json"
PROD_FILE="appsettings.Production.json"

generate_key() {
  openssl rand -base64 64 | tr -d '\n'
}

update_secret() {
  local file="$1"
  local key="$2"

  if [ ! -f "$file" ]; then
    echo "⚠️  $file không tồn tại, bỏ qua."
    return
  fi

  # Dùng python3 để cập nhật JSON an toàn (có sẵn trên macOS/Linux)
  python3 - <<PYEOF
import json, sys

with open("$file", "r") as f:
    data = json.load(f)

data.setdefault("Jwt", {})["SecretKey"] = "$key"

with open("$file", "w") as f:
    json.dump(data, f, indent=2, ensure_ascii=False)
    f.write("\n")

print("  ✅ $file đã được cập nhật.")
PYEOF
}

echo ""
echo "🔑 Generating JWT secret keys..."
echo ""

DEV_KEY=$(generate_key)
PROD_KEY=$(generate_key)

update_secret "$DEV_FILE" "$DEV_KEY"
update_secret "$PROD_FILE" "$PROD_KEY"

echo ""
echo "  Dev  key: ${DEV_KEY:0:20}... (đã ghi vào $DEV_FILE)"
echo "  Prod key: ${PROD_KEY:0:20}... (đã ghi vào $PROD_FILE)"
echo ""
echo "⚠️  Lưu ý: $PROD_FILE đã được thêm vào .gitignore — KHÔNG commit file này lên git!"
echo ""
