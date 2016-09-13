#!/bin/sh

createuser -h localhost -U postgres oidc

psql -h localhost -U postgres -c "alter user oidc with password 'supersecret'"

psql -h localhost -U postgres -c "create database oidc owner oidc"
