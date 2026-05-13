# FactuX Gateway - Guia de integracion para Frontend

Este documento esta pensado para que el equipo frontend pueda consumir el gateway de FactuX e implementar el cliente HTTP, autenticacion JWT, modulos CRUD y manejo de errores sin tener que inspeccionar el backend o la configuracion interna del gateway.

## Objetivo

El frontend debe consumir el gateway usando las mismas rutas publicadas por el backend, bajo el prefijo comun `/api`.

La idea recomendada es:

- autenticar una sola vez con `POST /api/Auth/token`;
- si aplica, completar MFA;
- guardar el `access_token` final;
- consultar `GET /api/Auth/me`;
- adjuntar `Authorization: Bearer <token>` al resto de llamadas;
- construir servicios por modulo;
- reutilizar un cliente HTTP comun;
- respetar el contrato JSON de entrada y salida sin renombrar propiedades.

## URL base

Configura una variable de entorno por ambiente:

- `VITE_API_BASE_URL` si usan Vite
- `NEXT_PUBLIC_API_BASE_URL` si usan Next.js
- `REACT_APP_API_BASE_URL` si usan Create React App

Ejemplo de valor:

```env
VITE_API_BASE_URL=https://localhost:7232
```

Todas las rutas de este documento son relativas a esa URL base.

## Autenticacion

## Obtener token

| Metodo | Ruta | Auth requerida |
|--------|------|----------------|
| `POST` | `/api/Auth/token` | No |

Cuerpo:

```json
{
  "username": "admin",
  "password": "admin123"
}
```

Respuesta esperada:

```json
{
  "access_token": "jwt-token",
  "token_type": "Bearer",
  "expires_in": 3600
}
```

Si el usuario requiere MFA, la respuesta recomendada puede ser:

```json
{
  "requires_mfa": true,
  "mfa_token": "temp-jwt-or-random-token",
  "mfa_type": "totp",
  "expires_in": 300
}
```

## Obtener sesion actual

| Metodo | Ruta | Auth requerida |
|--------|------|----------------|
| `GET` | `/api/Auth/me` | Si |

Respuesta recomendada:

```json
{
  "user": {
    "id": "b7a9d1a4-1111-2222-3333-444444444444",
    "email": "admin@factux.local",
    "nombres": "Administrador",
    "apellidos": "General",
    "display_name": "Administrador General",
    "estado": "ACTIVO"
  },
  "session": {
    "authenticated": true,
    "mfa_required": false,
    "mfa_completed": true,
    "token_expires_in": 3600
  },
  "authorization": {
    "roles": [
      "ADMIN"
    ],
    "permissions": [],
    "issuer_ids": [],
    "is_global_admin": true
  }
}
```

## Verificar MFA

| Metodo | Ruta | Auth requerida |
|--------|------|----------------|
| `POST` | `/api/Auth/mfa/verify` | No |

Cuerpo:

```json
{
  "mfa_token": "temp-jwt-or-random-token",
  "code": "123456"
}
```

Respuesta esperada:

```json
{
  "access_token": "jwt-token",
  "token_type": "Bearer",
  "expires_in": 3600
}
```

## Consultar estado MFA

| Metodo | Ruta | Auth requerida |
|--------|------|----------------|
| `GET` | `/api/Auth/mfa/status` | Si |

Respuesta recomendada:

```json
{
  "enabled": true,
  "type": "totp",
  "recovery_codes_remaining": 8
}
```

## Uso del token

Todas las rutas, excepto `POST /api/Auth/token`, deben enviarse con:

```http
Authorization: Bearer <access_token>
```

Tambien se recomienda enviar:

```http
Content-Type: application/json
```

cuando la operacion tenga cuerpo JSON.

## Envoltura de respuesta

La API responde normalmente con una estructura similar a esta:

```json
{
  "status": 200,
  "esError": false,
  "result": {},
  "descripcion": "Consulta exitosa",
  "mensaje": null
}
```

En error de autenticacion o autorizacion puede responder algo como:

```json
{
  "status": 401,
  "esError": true,
  "result": null,
  "descripcion": "Error de autenticacion/autorizacion",
  "mensaje": "No autorizado. Token JWT faltante o invalido."
}
```

## Recomendacion para frontend

Definir un tipo base comun:

```ts
export interface ApiResponse<T> {
  status: number;
  esError: boolean;
  result: T;
  descripcion: string;
  mensaje: string | null;
}
```

## Inventario de modulos y rutas

## Auth

| Operacion | Metodo | Ruta |
|-----------|--------|------|
| Obtener token | `POST` | `/api/Auth/token` |
| Obtener sesion actual | `GET` | `/api/Auth/me` |
| Verificar MFA | `POST` | `/api/Auth/mfa/verify` |
| Consultar estado MFA | `GET` | `/api/Auth/mfa/status` |

## Issuers

| Operacion | Metodo | Ruta |
|-----------|--------|------|
| Listar | `GET` | `/api/Issuers` |
| Obtener por id | `GET` | `/api/Issuers/{id}` |
| Crear | `POST` | `/api/Issuers` |
| Actualizar | `PUT` | `/api/Issuers` |
| Eliminar | `DELETE` | `/api/Issuers/{id}` |

## Products

| Operacion | Metodo | Ruta |
|-----------|--------|------|
| Listar | `GET` | `/api/Products` |
| Obtener por id | `GET` | `/api/Products/{id}` |
| Crear | `POST` | `/api/Products` |
| Actualizar | `PUT` | `/api/Products` |
| Eliminar | `DELETE` | `/api/Products/{id}` |

## Users

| Operacion | Metodo | Ruta |
|-----------|--------|------|
| Listar | `GET` | `/api/Users` |
| Obtener por id | `GET` | `/api/Users/{id}` |
| Crear | `POST` | `/api/Users` |
| Actualizar | `PUT` | `/api/Users` |
| Eliminar | `DELETE` | `/api/Users/{id}` |

## Roles

| Operacion | Metodo | Ruta |
|-----------|--------|------|
| Listar | `GET` | `/api/Roles` |
| Obtener por id | `GET` | `/api/Roles/{id}` |
| Crear | `POST` | `/api/Roles` |
| Actualizar | `PUT` | `/api/Roles` |
| Eliminar | `DELETE` | `/api/Roles/{id}` |

## UserRoles

| Operacion | Metodo | Ruta |
|-----------|--------|------|
| Listar | `GET` | `/api/UserRoles` |
| Obtener por id | `GET` | `/api/UserRoles/{id}` |
| Crear | `POST` | `/api/UserRoles` |
| Actualizar | `PUT` | `/api/UserRoles` |
| Eliminar | `DELETE` | `/api/UserRoles/{id}` |

## UserIssuers

| Operacion | Metodo | Ruta |
|-----------|--------|------|
| Listar | `GET` | `/api/UserIssuers` |
| Obtener por id | `GET` | `/api/UserIssuers/{id}` |
| Crear | `POST` | `/api/UserIssuers` |
| Actualizar | `PUT` | `/api/UserIssuers` |
| Eliminar | `DELETE` | `/api/UserIssuers/{id}` |

## SubscriptionPlans

| Operacion | Metodo | Ruta |
|-----------|--------|------|
| Listar | `GET` | `/api/SubscriptionPlans` |
| Obtener por id | `GET` | `/api/SubscriptionPlans/{id}` |
| Crear | `POST` | `/api/SubscriptionPlans` |
| Actualizar | `PUT` | `/api/SubscriptionPlans` |
| Eliminar | `DELETE` | `/api/SubscriptionPlans/{id}` |

## IssuerSubscriptions

| Operacion | Metodo | Ruta |
|-----------|--------|------|
| Listar | `GET` | `/api/IssuerSubscriptions` |
| Obtener por id | `GET` | `/api/IssuerSubscriptions/{id}` |
| Crear | `POST` | `/api/IssuerSubscriptions` |
| Actualizar | `PUT` | `/api/IssuerSubscriptions` |
| Eliminar | `DELETE` | `/api/IssuerSubscriptions/{id}` |

## Payloads minimos para formularios

Estos ejemplos ayudan a implementar formularios y tipos del frontend.

## Users

### Crear

```json
{
  "email": "admin@factux.com",
  "password_hash": "Admin12345",
  "nombres": "Juan",
  "apellidos": "Perez",
  "telefono": "0999999999",
  "estado": "ACTIVO"
}
```

### Actualizar

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "email": "admin@factux.com",
  "password_hash": "NuevaClave123",
  "nombres": "Juan",
  "apellidos": "Perez",
  "telefono": "0999999999",
  "estado": "ACTIVO"
}
```

## Roles

### Crear

```json
{
  "codigo": "ADMIN",
  "nombre": "Administrador",
  "descripcion": "Usuario con acceso total al sistema",
  "estado": "ACTIVO"
}
```

### Actualizar

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "codigo": "EMISOR",
  "nombre": "Emisor",
  "descripcion": "Usuario asociado a la operacion del emisor",
  "estado": "ACTIVO",
  "actualizado_por": "00000000-0000-0000-0000-000000000000"
}
```

## UserRoles

### Crear

```json
{
  "usuario_id": "00000000-0000-0000-0000-000000000000",
  "rol_id": "00000000-0000-0000-0000-000000000000",
  "estado": "ACTIVO"
}
```

### Actualizar

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "usuario_id": "00000000-0000-0000-0000-000000000000",
  "rol_id": "00000000-0000-0000-0000-000000000000",
  "estado": "ACTIVO",
  "actualizado_por": "00000000-0000-0000-0000-000000000000"
}
```

## UserIssuers

### Crear

```json
{
  "usuario_id": "00000000-0000-0000-0000-000000000000",
  "emisor_id": "00000000-0000-0000-0000-000000000000",
  "estado": "ACTIVO"
}
```

### Actualizar

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "usuario_id": "00000000-0000-0000-0000-000000000000",
  "emisor_id": "00000000-0000-0000-0000-000000000000",
  "estado": "ACTIVO",
  "actualizado_por": "00000000-0000-0000-0000-000000000000"
}
```

## SubscriptionPlans

### Crear

```json
{
  "codigo": "BASICO",
  "nombre": "Plan Basico",
  "descripcion": "Plan inicial con limite bajo de facturas",
  "duracion_dias": 30,
  "max_facturas": 100,
  "precio": 9.99,
  "estado": "ACTIVO"
}
```

### Actualizar

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "codigo": "PRO",
  "nombre": "Plan Pro",
  "descripcion": "Plan intermedio",
  "duracion_dias": 30,
  "max_facturas": 500,
  "precio": 19.99,
  "estado": "ACTIVO",
  "actualizado_por": "00000000-0000-0000-0000-000000000000"
}
```

## IssuerSubscriptions

### Crear

```json
{
  "emisor_id": "00000000-0000-0000-0000-000000000000",
  "plan_suscripcion_id": "00000000-0000-0000-0000-000000000000",
  "fecha_inicio": "2026-05-12",
  "fecha_fin": "2026-06-11",
  "facturas_consumidas": 0,
  "precio_pactado": 9.99,
  "estado": "ACTIVA"
}
```

### Actualizar

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "emisor_id": "00000000-0000-0000-0000-000000000000",
  "plan_suscripcion_id": "00000000-0000-0000-0000-000000000000",
  "fecha_inicio": "2026-05-12",
  "fecha_fin": "2026-06-11",
  "facturas_consumidas": 25,
  "precio_pactado": 9.99,
  "estado": "ACTIVA",
  "actualizado_por": "00000000-0000-0000-0000-000000000000"
}
```

## Issuers

### Crear

```json
{
  "ruc": "1790012345001",
  "razon_social": "MI EMPRESA S.A.",
  "nombre_comercial": "MI EMPRESA",
  "direccion_matriz": "Av. Principal 123",
  "direccion_establecimiento": "Sucursal Norte",
  "establecimiento": "001",
  "punto_emision": "001",
  "ambiente": "1",
  "tipo_emision": "1",
  "obligado_contabilidad": "SI",
  "certificado_p12": "<base64-del-p12>",
  "certificado_password": "<password-del-p12>",
  "fecha_caducidad_cert": "2027-12-31",
  "moneda": "DOLAR",
  "estado": "ACTIVO"
}
```

## Products

### Crear

```json
{
  "issuer_id": "00000000-0000-0000-0000-000000000000",
  "codigo_principal": "SPTI",
  "codigo_auxiliar": "SPTI-001",
  "nombre": "SERVICIO PROFESIONAL TECNICO INFORMATICO",
  "precio_unitario": 1840.00,
  "informacion_adicional": "Producto de prueba asociado al emisor",
  "tarifa_iva": "15%",
  "aplica_iva_turismo": false,
  "ice": false,
  "estado": "ACTIVO"
}
```

## Implementacion sugerida en frontend

## 1. Cliente HTTP base

Se recomienda centralizar:

- URL base
- `Authorization`
- serializacion JSON
- manejo uniforme de errores

Ejemplo con `fetch`:

```ts
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

type HttpMethod = "GET" | "POST" | "PUT" | "DELETE";

export interface ApiResponse<T> {
  status: number;
  esError: boolean;
  result: T;
  descripcion: string;
  mensaje: string | null;
}

export async function apiRequest<T>(
  path: string,
  method: HttpMethod = "GET",
  body?: unknown
): Promise<ApiResponse<T>> {
  const token = localStorage.getItem("access_token");

  const response = await fetch(`${API_BASE_URL}${path}`, {
    method,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {})
    },
    body: body ? JSON.stringify(body) : undefined
  });

  const data = await response.json();

  if (!response.ok || data.esError) {
    throw new Error(data?.mensaje || data?.descripcion || "Error de API");
  }

  return data;
}
```

## 2. Servicio de autenticacion

```ts
interface TokenResponse {
  access_token: string;
  token_type: string;
  expires_in: number;
}

interface MfaChallengeResponse {
  requires_mfa: true;
  mfa_token: string;
  mfa_type: string;
  expires_in: number;
}

interface SessionResponse {
  user: {
    id: string;
    email: string;
    nombres: string;
    apellidos: string;
    display_name: string;
    estado: string;
  };
  session: {
    authenticated: boolean;
    mfa_required: boolean;
    mfa_completed: boolean;
    token_expires_in: number;
  };
  authorization: {
    roles: string[];
    permissions: string[];
    issuer_ids: string[];
    is_global_admin: boolean;
  };
}

export async function login(username: string, password: string) {
  const response = await fetch(`${API_BASE_URL}/api/Auth/token`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({
      username,
      password
    })
  });

  const data: TokenResponse | MfaChallengeResponse = await response.json();

  if ("requires_mfa" in data && data.requires_mfa) {
    return data;
  }

  if (!response.ok || !data.access_token) {
    throw new Error("No se pudo obtener el token");
  }

  localStorage.setItem("access_token", data.access_token);
  return data;
}

export async function verifyMfa(mfaToken: string, code: string) {
  const response = await fetch(`${API_BASE_URL}/api/Auth/mfa/verify`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json"
    },
    body: JSON.stringify({
      mfa_token: mfaToken,
      code
    })
  });

  const data: TokenResponse = await response.json();

  if (!response.ok || !data.access_token) {
    throw new Error("No se pudo verificar MFA");
  }

  localStorage.setItem("access_token", data.access_token);
  return data;
}

export async function getCurrentSession() {
  return apiRequest<SessionResponse>("/api/Auth/me");
}

export async function getMfaStatus() {
  return apiRequest<{
    enabled: boolean;
    type: string;
    recovery_codes_remaining: number;
  }>("/api/Auth/mfa/status");
}
```

Nota importante:

- este endpoint puede responder distinto al resto, por eso conviene manejar login como caso separado;
- los demas modulos si pueden reutilizar `apiRequest<T>()` con la envoltura `ApiResponse<T>`.

## 3. Servicios por modulo

Ejemplo para `Users`:

```ts
export const usersApi = {
  list: () => apiRequest<any[]>("/api/Users"),
  getById: (id: string) => apiRequest<any>(`/api/Users/${id}`),
  create: (payload: unknown) => apiRequest<any>("/api/Users", "POST", payload),
  update: (payload: unknown) => apiRequest<any>("/api/Users", "PUT", payload),
  remove: (id: string) => apiRequest<any>(`/api/Users/${id}`, "DELETE")
};
```

Ese mismo patron se puede repetir para:

- `rolesApi`
- `issuersApi`
- `productsApi`
- `userRolesApi`
- `userIssuersApi`
- `subscriptionPlansApi`
- `issuerSubscriptionsApi`

## 4. Estructura sugerida

```text
src/
  api/
    http.ts
    auth.api.ts
    users.api.ts
    roles.api.ts
    issuers.api.ts
    products.api.ts
    userRoles.api.ts
    userIssuers.api.ts
    subscriptionPlans.api.ts
    issuerSubscriptions.api.ts
  types/
    api.ts
    auth.ts
    users.ts
    roles.ts
    issuers.ts
    products.ts
```

## 5. Manejo recomendado de errores

- si llega `401`, limpiar token y redirigir a login;
- si llega `403`, mostrar mensaje de permisos insuficientes;
- si `esError = true`, mostrar `mensaje` o `descripcion`;
- si falla red, mostrar mensaje generico de conectividad.

## 6. Orden sugerido de implementacion

1. Login y almacenamiento del token.
2. Flujo MFA si el backend lo requiere.
3. Cliente HTTP comun con interceptor de `Authorization`.
4. `GET /api/Auth/me` para hidratar sesion, roles y emisores asignados.
5. CRUD de `Users` y `Roles`.
6. CRUD de `Issuers` y `Products`.
7. CRUD de `UserRoles` y `UserIssuers`.
8. CRUD de `SubscriptionPlans` e `IssuerSubscriptions`.

## Consideraciones de UI

- usar tablas para `list`;
- usar formularios separados para `create` y `update`, o uno reutilizable;
- validar GUIDs antes de enviar rutas por id;
- usar selects para relaciones como `usuario_id`, `rol_id`, `emisor_id` y `plan_suscripcion_id`;
- tratar fechas como `YYYY-MM-DD` en `IssuerSubscriptions`;
- no renombrar propiedades JSON en el request.

## Checklist para desarrollo frontend

1. Configurar variable de entorno para la URL base del gateway.
2. Implementar login contra `/api/Auth/token`.
3. Soportar respuesta `requires_mfa` si aplica.
4. Guardar `access_token` final.
5. Consultar `/api/Auth/me` para construir la sesion local.
6. Adjuntar `Authorization: Bearer ...` en requests protegidos.
7. Crear un servicio por modulo.
8. Tipar `ApiResponse<T>`.
9. Implementar manejo comun de errores.
10. Probar primero `Auth/me`, `Users`, `Roles`, `Issuers` y `Products`.

## Referencias del repo

- Guia tecnica de rutas del gateway: [FactuX_API_gateway_guide.md](/d:/Repositorio/FactuxGatewayApi/docs/api/FactuX_API_gateway_guide.md:1)
- Configuracion de rutas del gateway: [reverseproxy.json](/d:/Repositorio/FactuxGatewayApi/src/FactuxGateway.API/reverseproxy.json:1)
- Coleccion Postman actual: [FactuX_API.postman_collection.json](/d:/Repositorio/FactuxGatewayApi/docs/api/FactuX_API.postman_collection.json:1)
- Guia frontend previa: [FactuX-API-Frontend.md](/d:/Repositorio/FactuxGatewayApi/docs/postman/FactuX-API-Frontend.md:1)
