#!/bin/bash

echo "🚀 Starting full deployment setup..."

echo "Starting Minikube..."
minikube start --insecure-registry="localhost:5000" --insecure-registry="host.minikube.internal:5000"

echo "Setting up Nexus access from Minikube..."
HOST_IP=$(hostname -I | awk '{print $1}')
minikube ssh "echo '$HOST_IP host.minikube.internal' | sudo tee -a /etc/hosts"

echo "Starting Nexus..."
cd _devops/nexus
docker-compose up -d
cd ../..

echo "Starting Jenkins..."
cd _devops/jenkins
docker-compose up -d
cd ../..

echo "Waiting for services to start..."
sleep 20

echo "Setting up kubeconfig for Jenkins..."
./_devops/jenkins/cp-kubeconfig.sh

echo "========================================"
echo "✅ Setup completed!"
echo ""
echo "📊 Jenkins: http://localhost:8080"
echo "   Get initial password:"
echo "   docker exec \$(docker ps -q -f name=jenkins) cat /var/jenkins_home/secrets/initialAdminPassword"
echo ""
echo "📦 Nexus: http://localhost:8081"
echo "   Login: admin / admin123 (first time)"
echo ""
echo "🐳 Minikube: minikube dashboard"
echo "========================================"