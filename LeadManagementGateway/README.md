# LeadManagementGateway (YARP API Gateway)

This project uses **YARP (Yet Another Reverse Proxy)** as the API Gateway.

## Gateway routes

- `/gateway/leads/{**catch-all}` -> forwards to `/api/leads/{**catch-all}`
- `/gateway/reps/{**catch-all}` -> forwards to `/api/reps/{**catch-all}`
- `/gateway/interactions/{**catch-all}` -> forwards to `/api/interactions/{**catch-all}`
- `/gateway/reports/{**catch-all}` -> forwards to `/api/reports/{**catch-all}`
- `/gateway/downstream-health` -> forwards to `/api/health`
- `/gateway/consul/services` -> discovered healthy services from Consul

All routes currently point to the cluster destination:

- `http://localhost:5001/`

Update destination in:

- `appsettings.json`
- `appsettings.Development.json`

## Run

1. Start your downstream Lead Management API at `http://localhost:5001`.
2. Start gateway:

```bash
dotnet run --project LeadManagementGateway
```

3. Test health endpoint:

```bash
curl http://localhost:5000/health
```

4. Test a proxied endpoint example:

```bash
curl http://localhost:5000/gateway/leads
```

The health endpoint returns:

- overall system status
- database status
- customer service latency status
- active backend servers
- per-server availability and response time

## Consul demo

1. Start Consul dev agent (Docker):

```bash
docker run --rm -p 8500:8500 -p 8600:8600/udp consul agent -dev -client=0.0.0.0
```

2. Start backend API:

```bash
dotnet run --project LeadManagementApp --urls http://localhost:5001
```

3. Start gateway:

```bash
dotnet run --project LeadManagementGateway --urls http://localhost:5000
```

4. Verify Consul discovery through gateway:

```bash
curl http://localhost:5000/gateway/consul/services
curl http://localhost:5000/health
```

5. Optional scaling demo (register second instance):

```bash
dotnet run --project LeadManagementApp --urls http://localhost:5002 -- Consul:ServiceId=lead-management-service-5002 Consul:ServicePort=5002
```
