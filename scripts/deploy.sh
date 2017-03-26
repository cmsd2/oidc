#!/bin/bash

set -u
set -x

DIR=$(readlink -f "$(dirname $0)")

# set the following environment variables:
#CLUSTER_NAME=...
#TASK_FAMILY=...
#SERVICE_NAME=...
#IMAGE_NAME=...
#IMAGE_TAG=...

export DOCKER_REPO="$DOCKER_REGISTRY/$IMAGE_NAME:$IMAGE_TAG"

LOGIN_CMD=$(aws ecr --region $AWS_DEFAULT_REGION get-login)

eval $LOGIN_CMD

docker build -t $IMAGE_NAME . || { echo 'docker build failed'; exit 1;}
wait

docker tag "$IMAGE_NAME" "$DOCKER_REGISTRY/$IMAGE_NAME:$IMAGE_TAG"

docker push "$DOCKER_REPO"

envsubst < $DIR/ecs.template.json > $DIR/ecs.json

aws ecs register-task-definition --cli-input-json file://$DIR/ecs.json

TASK_REVISION=$(aws ecs describe-task-definition --task-definition $TASK_FAMILY | jq -r .taskDefinition.revision)

aws ecs update-service --cluster "$CLUSTER_NAME" --service "$SERVICE_NAME" --task-definition "$TASK_FAMILY:$TASK_REVISION"
