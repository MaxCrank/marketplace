## Marketplace

Personal pet-project for educational/promo purposes with simpilifed approach (e.g. Core project would be split in parts if several implementations are needed in real solution and so on). 

It's meant to be an abstract microservice-based solution for marketplace that runs in Docker containers and:
- Can use popular message brokers such as RabbitMQ, Redis or Kafka (though Redis isn't considered as a real use-case due to weak guarantees).
- Can use Redis or memcached for caching.
- Has both SQL and NoSQL storages. For the sake of simplicity, the Content Delivery service was designed to use NoSQL storage for user comments, reviews, and catalog images.
- Has API gateway.
- Uses modern web framework on the front side.
- Can be demonstrated and tested locally in a "single click" way using scripts both on Windows and Linux.

## Environment

It's being developed in Windows environment using VS2017 and Linux containers.

## Progress

### **Cache clients** - *Done*
Please, run the following command from the solution root folder to execute corresponding tests.
```
docker-compose -f tests\Core\docker-compose.cache.yml run --rm tests && docker-compose -f tests\Core\docker-compose.cache.yml down
```

### **Event bus clients** - *In Progress*
### **Services** - *In Progress*
### **API gateway** - *Planned*
### **Front-end** - *Planned*