#!/usr/bin/env bash
set -euo pipefail

BASE_URL="http://localhost:8080"
GREEN='\033[0;32m'
RED='\033[0;31m'
NC='\033[0m'

http_req() {
    local METHOD="$1"
    local URL="$2"
    local DATA=""
    local AUTH_USER=""

    # Argument-Fix: 3 args = No Body, 4 args = With Body
    if [ "$#" -eq 4 ]; then
        DATA="$3"
        AUTH_USER="$4"
    elif [ "$#" -eq 3 ]; then
        AUTH_USER="$3"
    fi

    # Header mit Content-Length Fix
    local -a HEADERS=( -H "Content-Type: application/json" )
    
    if [[ -n "$AUTH_USER" ]]; then
        HEADERS+=( -H "Authorization: Bearer ${AUTH_USER}-mrptoken" )
    fi

    local RESP
    if [[ -n "$DATA" ]]; then
        # Normaler POST mit Daten
        RESP=$(curl -s -w "\n%{http_code}" -X "$METHOD" "$URL" "${HEADERS[@]}" -d "$DATA")
    else
        # Fix für HTTP 411: Content-Length mitsenden
        HEADERS+=( -H "Content-Length: 0" )
        RESP=$(curl -s -w "\n%{http_code}" -X "$METHOD" "$URL" "${HEADERS[@]}")
    fi

    local BODY=$(echo "$RESP" | sed '$d')
    local CODE=$(echo "$RESP" | tail -n1)

    if [[ "$CODE" -ge 200 && "$CODE" -lt 300 ]]; then
        echo -e "   [${GREEN}HTTP $CODE${NC}] $METHOD $URL" >&2
    else
        echo -e "   [${RED}HTTP $CODE${NC}] $METHOD $URL -> $BODY" >&2
    fi
    echo "$BODY"
}

echo -e "${GREEN}=== MRP ULTIMATE SCENARIO TEST (FIXED 411) ===${NC}"

# 1. ACCOUNTS
echo "1) Creating 4 accounts..."
for u in user1 user2 user3 user4; do
    http_req POST "$BASE_URL/api/users/register" "{\"username\":\"$u\",\"password\":\"pw\"}" > /dev/null
done

# 2. MEDIA (3/5/7/0)
echo "2) Distributing Media..."
for i in {1..3}; do http_req POST "$BASE_URL/api/media" "{\"title\":\"U1-M$i\",\"media_type\":\"movie\",\"release_year\":2026}" "user1" > /dev/null; done
for i in {1..5}; do http_req POST "$BASE_URL/api/media" "{\"title\":\"U2-M$i\",\"media_type\":\"movie\",\"release_year\":2026}" "user2" > /dev/null; done
for i in {1..7}; do http_req POST "$BASE_URL/api/media" "{\"title\":\"U3-M$i\",\"media_type\":\"movie\",\"release_year\":2026}" "user3" > /dev/null; done

# 3. IDs EXTRAHIEREN
echo "3) Extracting IDs..."
ALL_JSON=$(http_req GET "$BASE_URL/api/media" "user1")
M_ID_1=$(echo "$ALL_JSON" | jq -r '.[0].id // empty')
M_ID_2=$(echo "$ALL_JSON" | jq -r '.[5].id // empty')

if [[ -z "$M_ID_1" || "$M_ID_1" == "null" ]]; then
    echo -e "${RED}ERROR: Media creation failed. Check 411/Bearer errors above.${NC}"
    exit 1
fi

# 4. FAVORITEN
echo "4) Cross-Favorites..."
http_req POST "$BASE_URL/api/media/$M_ID_2/favorite" "user1" > /dev/null
http_req POST "$BASE_URL/api/media/$M_ID_1/favorite" "user2" > /dev/null

# 5. SECURITY
echo "5) Security Check (User 4)..."
http_req DELETE "$BASE_URL/api/media/$M_ID_1" "user4"

# 6. RECOMMENDATIONS
echo "6) Recommendations..."
http_req GET "$BASE_URL/api/users/user1/recommendations" "user1" > /dev/null
http_req GET "$BASE_URL/api/users/user4/recommendations" "user4" > /dev/null

# 7. BUSINESS LOGIC
echo "7) Rating..."
RATE_JSON=$(http_req POST "$BASE_URL/api/media/$M_ID_1/rate" '{"Stars":5,"Comment":"Nice"}' "user2")
RID=$(echo "$RATE_JSON" | jq -r '.id // .ratingId // empty')

if [[ -n "$RID" && "$RID" != "null" ]]; then
    http_req POST "$BASE_URL/api/ratings/$RID/confirm" "user1" > /dev/null
fi

# 8. STATS
echo "8) Leaderboard..."
http_req GET "$BASE_URL/api/users/leaderboard" "user1" > /dev/null

echo -e "${GREEN}=== ALL TESTS FINISHED ===${NC}"