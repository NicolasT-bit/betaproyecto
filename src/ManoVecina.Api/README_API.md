# ManoVecina.Api

API con:
- CRUDs (Users, WorkerProfiles, ServiceCategories, ServiceRequests, Reviews)
- JWT + Roles
- CORS
- Swagger
- Integración con Google Distance Matrix (opcional)

## Endpoints (resumen)
- POST /api/auth/register
- POST /api/auth/login
- GET /api/users (Admin)
- GET /api/users/{id} (Auth)
- PUT /api/users/{id} (Auth)
- DELETE /api/users/{id} (Admin)
- GET /api/workers
- GET /api/workers/{id}
- POST /api/workers (Worker/Admin)
- GET /api/workers/nearby?lat=&lng=&radiusKm=
- GET /api/servicecategories
- POST /api/servicecategories (Admin)
- POST /api/servicerequests (Auth)
- GET /api/servicerequests/{id} (Auth)
- PUT /api/servicerequests/{id}/status?status=Accepted|Rejected|Completed (Worker/Admin)
- POST /api/reviews (Auth) — sólo cuando la solicitud está Completed
- GET /api/reviews/worker/{workerUserId}
