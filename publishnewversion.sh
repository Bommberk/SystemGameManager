#!/bin/bash

VERSION=$1
APP_NAME="SystemGameManager"
PUBLISH_DIR="./publish"
RELEASES_DIR="./releases"

if [ -z "$VERSION" ]; then
  echo "❌ Bitte Version angeben: ./publishnewversion.sh v1.0.1"
  exit 1
fi

echo "🚀 Build + Publish startet für Version $VERSION"

# 1. Publish
dotnet publish -c Release -r win-x64 --self-contained true -o $PUBLISH_DIR

# 2. Velopack Pack
vpk pack \
  -u $APP_NAME \
  -v $VERSION \
  -p $PUBLISH_DIR \
  -o $RELEASES_DIR

# 3. Git Tag
git add .
git commit -m "Release $VERSION"
git tag $VERSION

# 4. Push
git push
git push origin $VERSION

echo "✅ Release $VERSION fertig!"