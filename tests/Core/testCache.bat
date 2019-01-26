docker build -t maxcrank/memcached:sasl -f ./src/Core/Cache/Dockerfiles/Dockerfile.memcached ./src/Core/Cache/Dockerfiles/
docker build -t marketplace-test-redis -f ./tests/Core/Dockerfile.tests.redis ./
docker build -t marketplace-test-core:cache -f ./tests/Core/Dockerfile.tests.core --build-arg testCategory=Cache ./
docker network create marketplace-test-network
docker run -d --rm --name marketplace-test-core-redis-job --network=marketplace-test-network --network-alias test-core.redis.com^
 --hostname test-core.redis.com -p 6379 marketplace-test-redis || goto :removeDockerNetwork
docker run -d --rm --name marketplace-test-core-memcached-job --network=marketplace-test-network --network-alias test-core.memcached.com^
 --hostname test-core.memcached.com -p 11211 -e SASL_PASSWORD=testCorePassword maxcrank/memcached:sasl || goto :cleanRedis
docker run --rm --name marketplace-test-core-cache-job --network=marketplace-test-network marketplace-test-core:cache || goto :cleanMemcached

:cleanMemcached
docker stop marketplace-test-core-memcached-job
:cleanRedis
docker stop marketplace-test-core-redis-job
:removeDockerNetwork
docker network rm marketplace-test-network