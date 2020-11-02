FROM alpine
WORKDIR /app

COPY . .
RUN chmod +x "./docker-run.sh" && \
    apk add icu-libs krb5-libs libgcc libintl libssl1.1 libstdc++ zlib tzdata

ENTRYPOINT ["./docker-run.sh"]