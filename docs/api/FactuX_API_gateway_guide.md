# FactuX API - Guia de rutas para Gateway

Este documento sirve como referencia para el proyecto de gateway que deba exponer, reenrutar o agrupar las rutas de FactuX API.

El objetivo es que el equipo del gateway pueda crear todas las rutas necesarias sin tener que inspeccionar el backend controlador por controlador.

## Objetivo del gateway

El gateway debe:

- publicar las rutas HTTP de la API actual;
- reenviar headers de autenticacion `Authorization: Bearer <token>`;
- respetar metodos, rutas y cuerpos JSON;
- conservar codigos de estado y respuestas del backend;
- aplicar CORS, rate limiting, logging o politicas adicionales si el proyecto gateway lo requiere.

## Base actual del backend

- Backend actual: `FactuX.API`
- Prefijo comun de rutas: `/api`
- Tipo de autenticacion: JWT Bearer
- Todas las rutas, excepto obtencion de token, requieren autenticacion.

## Autenticacion

### Obtener token

| Metodo | Ruta backend | Requiere JWT |
|--------|--------------|--------------|
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

## Contrato minimo recomendado de sesion

La implementacion actual de `POST /api/Auth/token` entrega un JWT basico, pero para soportar menu dinamico, guards por rol, restriccion por emisor y MFA se recomienda formalizar el siguiente contrato minimo.

### Objetivo

Despues de autenticarse, frontend y gateway deben poder responder de forma deterministica:

- quien es el usuario autenticado;
- que roles tiene;
- a que emisores puede acceder;
- si la sesion ya paso MFA o aun esta pendiente;
- si el usuario puede entrar al backoffice completo o a una vista restringida.

### Claims JWT minimos recomendados

Cuando la sesion quede completamente autenticada, el JWT deberia incluir como minimo:

```json
{
  "sub": "b7a9d1a4-1111-2222-3333-444444444444",
  "email": "admin@factux.local",
  "name": "Administrador General",
  "role": [
    "ADMIN"
  ],
  "issuer_ids": [
    "00000000-0000-0000-0000-000000000000"
  ],
  "mfa": "true",
  "amr": [
    "pwd",
    "otp"
  ],
  "iss": "FactuX.API",
  "aud": "FactuX.Client",
  "exp": 1760000000
}
```

### Significado de claims

| Claim | Obligatorio | Descripcion |
|--------|-------------|-------------|
| `sub` | Si | `userId` real en base de datos. No usar username fijo. |
| `email` | Si | Email del usuario autenticado. |
| `name` | Si | Nombre visible para UI. |
| `role` | Si | Uno o varios roles efectivos del usuario. |
| `issuer_ids` | Si | Lista de emisores asignados. Puede ser vacia para admin global. |
| `mfa` | Si | `true` si la sesion final ya aprobo MFA; `false` si no aplica solo en tokens temporales. |
| `amr` | Recomendado | Authentication methods reference. Ejemplo: `pwd`, `otp`. |
| `iss`, `aud`, `exp` | Si | Claims estandares de emision y expiracion. |

### Roles recomendados

- `ADMIN`: acceso total a backoffice.
- `EMISOR_ADMIN`: administra uno o varios emisores asignados.
- `EMISOR_OPERADOR`: opera productos y procesos del emisor, sin pantallas administrativas globales.

Si un usuario tiene multiples roles, backend debe emitir todos los roles efectivos.

### Endpoint recomendado `GET /api/Auth/me`

Este endpoint debe ser la fuente oficial de verdad para el frontend. El JWT sirve para autorizacion y `Auth/me` sirve para hidratar la sesion UI.

| Metodo | Ruta | Requiere JWT |
|--------|------|--------------|
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

### Reglas de negocio recomendadas para `Auth/me`

- si `roles` contiene `ADMIN`, `issuer_ids` puede ir vacio y `is_global_admin` debe ser `true`;
- si el usuario no es `ADMIN`, backend debe devolver solo emisores activos y asignados;
- `permissions` puede quedar vacio en la primera version si el sistema autorizara solo por rol;
- frontend no debe inferir permisos desde texto libre ni desde nombres de menu.

### MFA: contrato minimo recomendado

Si se habilitara MFA, conviene separar autenticacion primaria y sesion final.

#### Opcion recomendada

1. `POST /api/Auth/token` valida usuario y clave.
2. Si el usuario no requiere MFA, devuelve `access_token` final.
3. Si el usuario requiere MFA, no devuelve sesion final todavia.
4. Devuelve un `mfa_token` temporal de corta vida y el tipo de desafio.
5. Frontend envia el codigo MFA a un endpoint de verificacion.
6. Backend devuelve el `access_token` final y desde ahi `GET /api/Auth/me`.

#### Respuesta recomendada cuando MFA es requerido

```json
{
  "requires_mfa": true,
  "mfa_token": "temp-jwt-or-random-token",
  "mfa_type": "totp",
  "expires_in": 300
}
```

#### Endpoint recomendado para verificacion MFA

| Metodo | Ruta | Requiere JWT final |
|--------|------|--------------------|
| `POST` | `/api/Auth/mfa/verify` | No |

Cuerpo recomendado:

```json
{
  "mfa_token": "temp-jwt-or-random-token",
  "code": "123456"
}
```

Respuesta recomendada:

```json
{
  "access_token": "jwt-token",
  "token_type": "Bearer",
  "expires_in": 3600
}
```

### Endpoint recomendado para estado de MFA

Si se necesita configurar UI de seguridad de cuenta:

| Metodo | Ruta | Requiere JWT |
|--------|------|--------------|
| `GET` | `/api/Auth/mfa/status` | Si |

Respuesta recomendada:

```json
{
  "enabled": true,
  "type": "totp",
  "recovery_codes_remaining": 8
}
```

### Flujo recomendado para frontend

1. `POST /api/Auth/token`.
2. Si responde `requires_mfa = true`, mostrar paso MFA.
3. `POST /api/Auth/mfa/verify`.
4. Guardar `access_token` final.
5. `GET /api/Auth/me`.
6. Construir sesion local con `user`, `roles`, `permissions`, `issuer_ids`, `mfa_completed`.

### Modelo de sesion recomendado para frontend

```json
{
  "accessToken": "jwt-token",
  "user": {
    "id": "b7a9d1a4-1111-2222-3333-444444444444",
    "email": "admin@factux.local",
    "displayName": "Administrador General"
  },
  "roles": [
    "ADMIN"
  ],
  "permissions": [],
  "assignedIssuerIds": [],
  "isGlobalAdmin": true,
  "mfaEnabled": true,
  "mfaCompleted": true
}
```

### Recomendacion de compatibilidad

Para una primera version estable:

- mantener `POST /api/Auth/token`;
- agregar `GET /api/Auth/me`;
- agregar `POST /api/Auth/mfa/verify` solo si MFA se implementa ya;
- enriquecer el JWT con `sub`, `email`, `role` e `issuer_ids`;
- evitar exponer al frontend la necesidad de consultar `Users`, `UserRoles` y `UserIssuers` para armar sesion.

## Convencion sugerida para el gateway

Si no hay una regla distinta en el proyecto gateway, la opcion mas simple y segura es exponer exactamente las mismas rutas del backend:

- gateway `/api/...` -> backend `/api/...`

Eso evita transformaciones innecesarias y simplifica frontend, Postman y documentacion.

## Inventario completo de rutas

## Auth

| Metodo | Ruta |
|--------|------|
| `POST` | `/api/Auth/token` |

## Issuers

| Metodo | Ruta |
|--------|------|
| `GET` | `/api/Issuers` |
| `GET` | `/api/Issuers/{id}` |
| `POST` | `/api/Issuers` |
| `PUT` | `/api/Issuers` |
| `DELETE` | `/api/Issuers/{id}` |

## Products

| Metodo | Ruta |
|--------|------|
| `GET` | `/api/Products` |
| `GET` | `/api/Products/{id}` |
| `POST` | `/api/Products` |
| `PUT` | `/api/Products` |
| `DELETE` | `/api/Products/{id}` |

## Users

| Metodo | Ruta |
|--------|------|
| `GET` | `/api/Users` |
| `GET` | `/api/Users/{id}` |
| `POST` | `/api/Users` |
| `PUT` | `/api/Users` |
| `DELETE` | `/api/Users/{id}` |

## Roles

| Metodo | Ruta |
|--------|------|
| `GET` | `/api/Roles` |
| `GET` | `/api/Roles/{id}` |
| `POST` | `/api/Roles` |
| `PUT` | `/api/Roles` |
| `DELETE` | `/api/Roles/{id}` |

## UserRoles

| Metodo | Ruta |
|--------|------|
| `GET` | `/api/UserRoles` |
| `GET` | `/api/UserRoles/{id}` |
| `POST` | `/api/UserRoles` |
| `PUT` | `/api/UserRoles` |
| `DELETE` | `/api/UserRoles/{id}` |

## UserIssuers

| Metodo | Ruta |
|--------|------|
| `GET` | `/api/UserIssuers` |
| `GET` | `/api/UserIssuers/{id}` |
| `POST` | `/api/UserIssuers` |
| `PUT` | `/api/UserIssuers` |
| `DELETE` | `/api/UserIssuers/{id}` |

## SubscriptionPlans

| Metodo | Ruta |
|--------|------|
| `GET` | `/api/SubscriptionPlans` |
| `GET` | `/api/SubscriptionPlans/{id}` |
| `POST` | `/api/SubscriptionPlans` |
| `PUT` | `/api/SubscriptionPlans` |
| `DELETE` | `/api/SubscriptionPlans/{id}` |

## IssuerSubscriptions

| Metodo | Ruta |
|--------|------|
| `GET` | `/api/IssuerSubscriptions` |
| `GET` | `/api/IssuerSubscriptions/{id}` |
| `POST` | `/api/IssuerSubscriptions` |
| `PUT` | `/api/IssuerSubscriptions` |
| `DELETE` | `/api/IssuerSubscriptions/{id}` |

## Payloads minimos por modulo

El gateway normalmente no deberia transformar payloads. Aun asi, este resumen ayuda a validar contratos.

## Users

### `POST /api/Users`

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

Nota:

- En la implementacion actual, `password_hash` llega como texto plano y el backend lo transforma a hash antes de guardar.

### `PUT /api/Users`

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

### `POST /api/Roles`

```json
{
  "codigo": "ADMIN",
  "nombre": "Administrador",
  "descripcion": "Usuario con acceso total al sistema",
  "estado": "ACTIVO"
}
```

### `PUT /api/Roles`

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

### `POST /api/UserRoles`

```json
{
  "usuario_id": "00000000-0000-0000-0000-000000000000",
  "rol_id": "00000000-0000-0000-0000-000000000000",
  "estado": "ACTIVO"
}
```

### `PUT /api/UserRoles`

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

### `POST /api/UserIssuers`

```json
{
  "usuario_id": "00000000-0000-0000-0000-000000000000",
  "emisor_id": "00000000-0000-0000-0000-000000000000",
  "estado": "ACTIVO"
}
```

### `PUT /api/UserIssuers`

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

### `POST /api/SubscriptionPlans`

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

### `PUT /api/SubscriptionPlans`

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

### `POST /api/IssuerSubscriptions`

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

### `PUT /api/IssuerSubscriptions`

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

### `POST /api/Issuers`

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

### `POST /api/Products`

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

## Seguridad y headers

- Reenviar `Authorization` sin alteraciones hacia el backend.
- Mantener `Content-Type: application/json` en operaciones con cuerpo.
- Si el gateway agrega validaciones o filtros, no debe modificar la forma del JSON de entrada ni de salida sin coordinación previa.

## Respuesta esperada del backend

La API usa una envoltura de respuesta similar a esta:

```json
{
  "status": 200,
  "esError": false,
  "result": {},
  "descripcion": "Consulta exitosa",
  "mensaje": null
}
```

En errores de autenticacion/autorizacion puede responder algo equivalente a:

```json
{
  "status": 401,
  "esError": true,
  "result": null,
  "descripcion": "Error de autenticacion/autorizacion",
  "mensaje": "No autorizado. Token JWT faltante o invalido."
}
```

## Recomendaciones para el equipo gateway

- Mantener las mismas rutas del backend mientras no exista una necesidad real de versionado o agregacion.
- Publicar un prefijo unico, por ejemplo `/api`, para evitar romper clientes actuales.
- No transformar nombres de propiedades JSON.
- Si el gateway centraliza autenticacion en el futuro, alinear primero el flujo JWT con este backend.
- Actualizar tambien la coleccion Postman y la documentacion frontend si el gateway expone rutas distintas.

## Referencias

- Guia frontend actual: [FactuX_API_frontend_guide.md](D:/Repositorio/FactuX/doc/api/FactuX_API_frontend_guide.md:1)
- Coleccion Postman actual: [FactuX_API.postman_collection.json](D:/Repositorio/FactuX/doc/postman/FactuX_API.postman_collection.json:1)
