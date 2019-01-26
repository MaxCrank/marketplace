docker build -t marketplace-test-redis -f ./tests/Core/Dockerfile.tests.redis ./
docker build -t marketplace-test-core:eventbus -f ./tests/Core/Dockerfile.tests.core --build-arg testCategory=EventBus ./
docker network create marketplace-test-network
docker run -d --rm --name marketplace-test-core-redis-job --network=marketplace-test-network --network-alias test-core.redis.com^
 --hostname test-core.redis.com -p 6379 marketplace-test-redis || goto :removeDockerNetwork
docker run -d --rm --name marketplace-test-core-rabbitmq-job --network=marketplace-test-network --network-alias test-core.rabbitmq.com^
 --hostname test-core.rabbitmq.com -p 5672 rabbitmq:alpine || goto :cleanRedis
docker run -d --rm --name marketplace-test-core-zookeeper-job --network=marketplace-test-network --network-alias test-core.zookeeper.com^
 --hostname test-core.zookeeper.com -p 2181 -e ZOOKEEPER_CLIENT_PORT=2181 confluentinc/cp-zookeeper || goto :cleanRabbitMq
docker run -d --rm --name marketplace-test-core-kafka-job --network=marketplace-test-network --network-alias test-core.kafka.com^
 --hostname test-core.kafka.com -p 9092 -e KAFKA_ZOOKEEPER_CONNECT=test-core.zookeeper.com:2181^
 -e KAFKA_ADVERTISED_LISTENERS=PLAINTEXT://kafka:29092,PLAINTEXT_HOST://test-core.kafka.com:9092^
 -e KAFKA_LISTENER_SECURITY_PROTOCOL_MAP=PLAINTEXT:PLAINTEXT,PLAINTEXT_HOST:PLAINTEXT -e KAFKA_INTER_BROKER_LISTENER_NAME=PLAINTEXT^
 -e KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR=1 -e KAFKA_BROKER_ID=1 confluentinc/cp-kafka || goto :cleanZookeeper
docker run --rm --name marketplace-test-core-eventbus-job --network=marketplace-test-network marketplace-test-core:eventbus || goto :cleanKafka

:cleanKafka
docker stop marketplace-test-core-kafka-job
:cleanZookeeper
docker stop marketplace-test-core-zookeeper-job
:cleanRabbitMq
docker stop marketplace-test-core-rabbitmq-job
:cleanRedis
docker stop marketplace-test-core-redis-job
:removeDockerNetwork
docker network rm marketplace-test-network