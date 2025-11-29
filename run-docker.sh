#!/bin/bash

echo
echo "==============================================="
echo "    C# Microservices - Docker Run Script"
echo "==============================================="
echo
echo "This script will:"
echo "1. Build all Docker images"
echo "2. Start all services using Docker Compose"
echo
echo "Services will be available at:"
echo "- Notification Gateway: http://localhost:8080"
echo "- Computing Service: http://localhost:8081"
echo "- RabbitMQ Management: http://localhost:15672"
echo "- Prometheus: http://localhost:9090"
echo "- Grafana: http://localhost:3000"
echo
echo "Press Ctrl+C to stop the services after starting."
echo "==============================================="
echo

# Make sure we're in the correct directory
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
cd "$SCRIPT_DIR"

# Run docker-compose
docker-compose up --build
