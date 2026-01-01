set -euo pipefail

BASE_URL="http://localhost:8080"

echo "MRP comprehensive API test against $BASE_URL"

# NOTE: this script requires `jq` for JSON parsing/printing. Install with your package manager.

# Helper: pretty print response
pp() { echo; echo "--- $1 ---"; cat; echo; }

# Helper: perform request and return body and code
http_req() {
  # args: METHOD URL [DATA] [AUTH]
  local METHOD="$1"; shift
  local URL="$1"; shift
  local DATA=""; local AUTH_HDR=""
  if [[ ${1-} != "" && ${1:0:1} == '{' ]]; then
    DATA="$1"; shift
  fi
  if [[ ${1-} != "" ]]; then
    AUTH_HDR="$1"; shift
  fi

  if [[ -n "$DATA" ]]; then
    curl -s -w "\n%{http_code}" -X "$METHOD" "$URL" -H "Content-Type: application/json" ${AUTH_HDR:+-H "$AUTH_HDR"} -d "$DATA"
  else
    # ensure Content-Length header for empty POST/PUT if server expects it
    if [[ "$METHOD" == "POST" || "$METHOD" == "PUT" ]]; then
      curl -s -w "\n%{http_code}" -X "$METHOD" "$URL" ${AUTH_HDR:+-H "$AUTH_HDR"} -H "Content-Length: 0"
    else
      curl -s -w "\n%{http_code}" -X "$METHOD" "$URL" ${AUTH_HDR:+-H "$AUTH_HDR"}
    fi
  fi
}

echo "1) Register users 'alice' and 'bob'"
REG_A=$(http_req POST "$BASE_URL/api/users/register" '{"username":"alice","password":"pw123"}')
REG_A_BODY=$(echo "$REG_A" | sed '$d')
REG_A_CODE=$(echo "$REG_A" | tail -n1)
pp "Register alice (HTTP $REG_A_CODE)" <<<"$REG_A_BODY"

REG_B=$(http_req POST "$BASE_URL/api/users/register" '{"username":"bob","password":"pw123"}')
REG_B_BODY=$(echo "$REG_B" | sed '$d')
REG_B_CODE=$(echo "$REG_B" | tail -n1)
pp "Register bob (HTTP $REG_B_CODE)" <<<"$REG_B_BODY"

echo "2) Login users and capture tokens"
LOGIN_A=$(http_req POST "$BASE_URL/api/users/login" '{"username":"alice","password":"pw123"}')
LOGIN_A_BODY=$(echo "$LOGIN_A" | sed '$d')
LOGIN_A_CODE=$(echo "$LOGIN_A" | tail -n1)
pp "Login alice (HTTP $LOGIN_A_CODE)" <<<"$LOGIN_A_BODY"
TOKEN_A=$(echo "$LOGIN_A_BODY" | jq -r '.Token // .token // empty' || true)

LOGIN_B=$(http_req POST "$BASE_URL/api/users/login" '{"username":"bob","password":"pw123"}')
LOGIN_B_BODY=$(echo "$LOGIN_B" | sed '$d')
LOGIN_B_CODE=$(echo "$LOGIN_B" | tail -n1)
pp "Login bob (HTTP $LOGIN_B_CODE)" <<<"$LOGIN_B_BODY"
TOKEN_B=$(echo "$LOGIN_B_BODY" | jq -r '.Token // .token // empty' || true)

if [[ -z "$TOKEN_A" || -z "$TOKEN_B" ]]; then
  echo "One or both tokens missing; aborting further protected tests." >&2
fi

AUTH_A="Authorization: Bearer $TOKEN_A"
AUTH_B="Authorization: Bearer $TOKEN_B"

echo "3) Create media entries: alice creates 2, bob creates 1"
M1=$(http_req POST "$BASE_URL/api/media" '{"userid":1,"title":"Alice Movie","description":"A test movie","type":"movie","year":2020,"genres":["drama"],"agerating":"FSK0"}' "$AUTH_A")
M1_BODY=$(echo "$M1" | sed '$d')
M1_CODE=$(echo "$M1" | tail -n1)
pp "Create media A1 (HTTP $M1_CODE)" <<<"$M1_BODY"

M2=$(http_req POST "$BASE_URL/api/media" '{"userid":1,"title":"Alice Game","description":"A test game","type":"game","year":2020,"genres":["rpg"],"agerating":"FSK16"}' "$AUTH_A")
M2_BODY=$(echo "$M2" | sed '$d')
M2_CODE=$(echo "$M2" | tail -n1)
pp "Create media A2 (HTTP $M2_CODE)" <<<"$M2_BODY"

M3=$(http_req POST "$BASE_URL/api/media" '{"userid":2,"title":"Bob Movie","description":"Bob movie","type":"movie","year":2019,"genres":["action"],"agerating":"FSK0"}' "$AUTH_B")
M3_BODY=$(echo "$M3" | sed '$d')
M3_CODE=$(echo "$M3" | tail -n1)
pp "Create media B1 (HTTP $M3_CODE)" <<<"$M3_BODY"

# Try to extract created IDs (best-effort: services may return message only)
ALL=$(http_req GET "$BASE_URL/api/media" "" "$AUTH_A")
ALL_BODY=$(echo "$ALL" | sed '$d')
ALL_CODE=$(echo "$ALL" | tail -n1)
pp "All Media (HTTP $ALL_CODE)" <<<"$ALL_BODY"

ID_A1=$(echo "$ALL_BODY" | jq -r '.[] | select(.title=="Alice Movie") | .id // .Id' | head -n1 || true)
ID_A2=$(echo "$ALL_BODY" | jq -r '.[] | select(.title=="Alice Game") | .id // .Id' | head -n1 || true)
ID_B1=$(echo "$ALL_BODY" | jq -r '.[] | select(.title=="Bob Movie") | .id // .Id' | head -n1 || true)

echo "IDs: A1=$ID_A1 A2=$ID_A2 B1=$ID_B1"

echo "4) Ownership test: bob tries to update alice's media (should be forbidden)"
if [[ -n "$ID_A1" ]]; then
  # build JSON safely with printf
  JSON_BOB_EDIT=$(printf '{"id":%s,"userid":2,"title":"Malicious Edit","description":"x","type":"movie","year":2022,"genres":["drama"],"agerating":"FSK0"}' "$ID_A1")
  U1=$(http_req PUT "$BASE_URL/api/media/$ID_A1" "$JSON_BOB_EDIT" "$AUTH_B") || true
  U1_BODY=$(echo "$U1" | sed '$d')
  U1_CODE=$(echo "$U1" | tail -n1)
  pp "Bob updates Alice media (HTTP $U1_CODE)" <<<"$U1_BODY"
fi

echo "5) Alice updates her media (should succeed)"
if [[ -n "$ID_A1" ]]; then
  JSON_ALICE_UPD=$(printf '{"id":%s,"userid":1,"title":"Alice Movie (edited)","description":"Updated desc","type":"movie","year":2021,"genres":["action","drama"],"agerating":"FSK0"}' "$ID_A1")
  UPD_A=$(http_req PUT "$BASE_URL/api/media/$ID_A1" "$JSON_ALICE_UPD" "$AUTH_A")
  UPD_A_BODY=$(echo "$UPD_A" | sed '$d')
  UPD_A_CODE=$(echo "$UPD_A" | tail -n1)
  pp "Update Media (HTTP $UPD_A_CODE)" <<<"$UPD_A_BODY"
fi

echo "6) Get single media (alice)"
if [[ -n "$ID_A1" ]]; then
  S1=$(http_req GET "$BASE_URL/api/media/$ID_A1" "" "$AUTH_A")
  S1_BODY=$(echo "$S1" | sed '$d')
  S1_CODE=$(echo "$S1" | tail -n1)
  pp "Single Media (HTTP $S1_CODE)" <<<"$S1_BODY"
fi

echo "7) Rating flows: bob rates alice media, duplicates, like tests, confirm comment"
RATING_B1=""
if [[ -n "$ID_A1" ]]; then
  # Use C# property casing (Stars, Comment) to match deserializer
  RATE_JSON=$(printf '{"Stars":5,"Comment":"Great!"}')
  R1=$(http_req POST "$BASE_URL/api/media/$ID_A1/rate" "$RATE_JSON" "$AUTH_B")
  R1_BODY=$(echo "$R1" | sed '$d')
  R1_CODE=$(echo "$R1" | tail -n1)
  pp "Bob rates Alice media (HTTP $R1_CODE)" <<<"$R1_BODY"
  RATING_B1=$(echo "$R1_BODY" | jq -r '.ratingId // .id // empty' || true)
  if [[ -z "$RATING_B1" ]]; then
    echo "Note: rating id not returned by service; some follow-up ops may be skipped." >&2
  fi

  # Duplicate rating attempt
  R1_DUP=$(http_req POST "$BASE_URL/api/media/$ID_A1/rate" "$RATE_JSON" "$AUTH_B")
  R1_DUP_BODY=$(echo "$R1_DUP" | sed '$d')
  R1_DUP_CODE=$(echo "$R1_DUP" | tail -n1)
  pp "Bob duplicate rate (HTTP $R1_DUP_CODE)" <<<"$R1_DUP_BODY"
fi

echo "8) Like rating: alice likes bob's rating (if id available)"
if [[ -n "$RATING_B1" ]]; then
  LIKE1=$(http_req POST "$BASE_URL/api/ratings/$RATING_B1/like" "" "$AUTH_A")
  LIKE1_BODY=$(echo "$LIKE1" | sed '$d')
  LIKE1_CODE=$(echo "$LIKE1" | tail -n1)
  pp "Alice likes rating (HTTP $LIKE1_CODE)" <<<"$LIKE1_BODY"

  # second like should fail (already liked)
  LIKE2=$(http_req POST "$BASE_URL/api/ratings/$RATING_B1/like" "" "$AUTH_A")
  LIKE2_BODY=$(echo "$LIKE2" | sed '$d')
  LIKE2_CODE=$(echo "$LIKE2" | tail -n1)
  pp "Alice likes rating again (HTTP $LIKE2_CODE)" <<<"$LIKE2_BODY"
fi

echo "9) Confirm comment visibility: ensure unconfirmed comments are hidden until confirmed"
if [[ -n "$RATING_B1" ]]; then
  # Check rating history for alice (should not show bob's unconfirmed comment)
  HIST_A=$(http_req GET "$BASE_URL/api/users/alice/rate/history" "" "$AUTH_A")
  HIST_A_BODY=$(echo "$HIST_A" | sed '$d')
  HIST_A_CODE=$(echo "$HIST_A" | tail -n1)
  pp "Rating history before confirm (HTTP $HIST_A_CODE)" <<<"$HIST_A_BODY"

  # Confirm comment by bob (owner of the rating)
  CONF=$(http_req POST "$BASE_URL/api/ratings/$RATING_B1/confirm" '{}' "$AUTH_B")
  CONF_BODY=$(echo "$CONF" | sed '$d')
  CONF_CODE=$(echo "$CONF" | tail -n1)
  pp "Confirm comment (HTTP $CONF_CODE)" <<<"$CONF_BODY"

  # Check history again (should now include comment)
  HIST_A2=$(http_req GET "$BASE_URL/api/users/alice/rate/history" "" "$AUTH_A")
  HIST_A2_BODY=$(echo "$HIST_A2" | sed '$d')
  HIST_A2_CODE=$(echo "$HIST_A2" | tail -n1)
  pp "Rating history after confirm (HTTP $HIST_A2_CODE)" <<<"$HIST_A2_BODY"
fi

echo "10) Favorites: alice favorites bob's media and duplicate test"
if [[ -n "$ID_B1" ]]; then
  FAV1=$(http_req POST "$BASE_URL/api/media/$ID_B1/favorite" '{}' "$AUTH_A")
  FAV1_BODY=$(echo "$FAV1" | sed '$d')
  FAV1_CODE=$(echo "$FAV1" | tail -n1)
  pp "Alice favorites Bob media (HTTP $FAV1_CODE)" <<<"$FAV1_BODY"

  # duplicate favorite attempt
  FAV2=$(http_req POST "$BASE_URL/api/media/$ID_B1/favorite" '{}' "$AUTH_A")
  FAV2_BODY=$(echo "$FAV2" | sed '$d')
  FAV2_CODE=$(echo "$FAV2" | tail -n1)
  pp "Alice favorites again (HTTP $FAV2_CODE)" <<<"$FAV2_BODY"

  # Get favorites list
  FAVLIST=$(http_req GET "$BASE_URL/api/users/alice/favorite" "" "$AUTH_A")
  FAVLIST_BODY=$(echo "$FAVLIST" | sed '$d')
  FAVLIST_CODE=$(echo "$FAVLIST" | tail -n1)
  pp "Favorites List (HTTP $FAVLIST_CODE)" <<<"$FAVLIST_BODY"
fi

echo "11) Recommendations, profile and leaderboard"
REC=$(http_req GET "$BASE_URL/api/users/alice/recommendations" "" "$AUTH_A")
REC_BODY=$(echo "$REC" | sed '$d')
REC_CODE=$(echo "$REC" | tail -n1)
pp "Recommendations (HTTP $REC_CODE)" <<<"$REC_BODY"

PROF=$(http_req GET "$BASE_URL/api/alice/profile" "" "$AUTH_A")
PROF_BODY=$(echo "$PROF" | sed '$d')
PROF_CODE=$(echo "$PROF" | tail -n1)
pp "Profile (HTTP $PROF_CODE)" <<<"$PROF_BODY"

LB=$(http_req GET "$BASE_URL/api/users/leaderboard" "" "$AUTH_A")
LB_BODY=$(echo "$LB" | sed '$d')
LB_CODE=$(echo "$LB" | tail -n1)
pp "Leaderboard (HTTP $LB_CODE)" <<<"$LB_BODY"

echo "12) Negative tests / error checks"
echo "- Bob attempts to delete Alice's media (should be forbidden)"
if [[ -n "$ID_A2" ]]; then
  DEL_B=$(http_req DELETE "$BASE_URL/api/media/$ID_A2" "" "$AUTH_B") || true
  DEL_B_BODY=$(echo "$DEL_B" | sed '$d')
  DEL_B_CODE=$(echo "$DEL_B" | tail -n1)
  pp "Bob delete Alice media (HTTP $DEL_B_CODE)" <<<"$DEL_B_BODY"
fi

echo "- Alice attempts to like the same rating twice (should be prevented)"
if [[ -n "$RATING_B1" ]]; then
  # Already tested earlier; report status
  echo "(see earlier like tests)"
fi

echo "13) Logout both users"
LOG_A=$(http_req POST "$BASE_URL/api/users/logout" "" "$AUTH_A")
LOG_A_BODY=$(echo "$LOG_A" | sed '$d')
LOG_A_CODE=$(echo "$LOG_A" | tail -n1)
pp "Logout alice (HTTP $LOG_A_CODE)" <<<"$LOG_A_BODY"

LOG_B=$(http_req POST "$BASE_URL/api/users/logout" "" "$AUTH_B")
LOG_B_BODY=$(echo "$LOG_B" | sed '$d')
LOG_B_CODE=$(echo "$LOG_B" | tail -n1)
pp "Logout bob (HTTP $LOG_B_CODE)" <<<"$LOG_B_BODY"

echo "Script finished. Review the printed responses for any unexpected status codes (4xx/5xx)."
