FROM jenkins/jenkins:alpine

USER root

RUN apk update && \
    apk add --no-cache docker-cli docker-compose curl && \
    KUBE_VERSION=$(curl -s https://storage.googleapis.com/kubernetes-release/release/stable.txt) && \
    curl -LO https://storage.googleapis.com/kubernetes-release/release/${KUBE_VERSION}/bin/linux/amd64/kubectl && \
    chmod +x kubectl && mv kubectl /usr/local/bin/ && \
    if ! getent group docker; then addgroup -S docker; fi && \
    addgroup jenkins docker

USER jenkins
