#!/bin/bash
set -e

echo "Building Angular application..."
npm run build

echo "Building Docker image with Podman..."
podman build -t management-app:latest .

echo "Image built successfully: management-app:latest"

