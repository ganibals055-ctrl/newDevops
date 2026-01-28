#!/bin/bash
set -e

HOST_USER=$(whoami)
HOST_HOME=$(eval echo "~$HOST_USER")
SCRIPT_DIR="$(pwd)"

if [ ! -f "$HOST_HOME/.kube/config" ]; then
    exit 1
fi

JENKINS_CONFIG="$SCRIPT_DIR/config.jenkins"
cp "$HOST_HOME/.kube/config" "$JENKINS_CONFIG"
sed -i "s|$HOST_HOME/.minikube|/home/jenkins/.minikube|g" "$JENKINS_CONFIG"

if [ ! -d "$HOST_HOME/.minikube" ]; then
    exit 1
fi