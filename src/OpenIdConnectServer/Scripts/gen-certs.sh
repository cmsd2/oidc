#!/bin/sh

openssl genrsa -out private.pem 2048
openssl req -x509 -new -key private.pem -out public.pem
openssl pkcs12 -export -in public.pem -inkey private.pem -out cert.pfx
