#!/bin/bash
# Local iOS build script — runs without Jenkins or Fastlane Match.
# Copy this file to your Xcode output folder (build/iOS/) and run it.
#
# Prerequisites:
#   - Xcode installed with command-line tools
#   - CocoaPods installed (gem install cocoapods)
#   - Set IOS_TEAM_ID env var or edit the DEVELOPMENT_TEAM below
#
# Usage:
#   cd build/iOS
#   IOS_TEAM_ID=YOUR_TEAM_ID ./local-build.sh
#   # or for a device build:
#   IOS_TEAM_ID=YOUR_TEAM_ID ./local-build.sh device

set -euo pipefail
cd "$(dirname "$0")"

TEAM_ID="${IOS_TEAM_ID:-}"
DESTINATION="${1:-generic/platform=iOS}"

if [ -z "$TEAM_ID" ]; then
    echo "ERROR: Set IOS_TEAM_ID environment variable to your Apple Developer Team ID."
    echo "  export IOS_TEAM_ID=XXXXXXXXXX"
    echo "  ./local-build.sh"
    exit 1
fi

echo "=== Installing CocoaPods ==="
pod install --repo-update

echo "=== Building Xcode project ==="
xcodebuild \
  -workspace Unity-iPhone.xcworkspace \
  -scheme Unity-iPhone \
  -configuration Debug \
  -destination "$DESTINATION" \
  -allowProvisioningUpdates \
  CODE_SIGN_STYLE=Automatic \
  DEVELOPMENT_TEAM="$TEAM_ID" \
  build

echo "=== Build completed ==="
