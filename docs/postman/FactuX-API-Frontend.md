# FactuX API — Guía para frontend

Este documento resume el contrato HTTP que usa la colección Postman `docs/api/FactuX_API.postman_collection.json`, para que el frontend pueda implementar cliente HTTP, manejo de JWT y formularios de emisor sin depender de Postman.

## URL base

- En Postman la variable `baseUrl` por defecto es `https://localhost:7001`. Ese valor es solo un ejemplo de entorno.
- El **gateway** de este repositorio (`FactuxGateway.API`) en desarrollo suele exponerse según `src/FactuxGateway.API/Properties/launchSettings.json`, por ejemplo:
  - `http://localhost:5051`
  - o `http://localhost:7232` (perfil `https`)

Configura en el frontend una variable de entorno (por ejemplo `VITE_API_BASE_URL` / `NEXT_PUBLIC_API_BASE_URL`) con la URL del gateway o de la API que consumas en cada ambiente. Todas las rutas de abajo son **relativas** a esa base (sin barra final recomendada, o normaliza en el cliente).

## Autenticación (JWT)

### Obtener token

| Campo | Valor |
|--------|--------|
| Método | `POST` |
| Ruta | `/api/Auth/token` |
| `Content-Type` | `application/json` |

**Cuerpo (JSON):**

```json
{
  "username": "admin",
  "password": "admin123"
}
```

**Respuesta esperada (según scripts de Postman):** JSON con propiedad `access_token` (string). El frontend debe guardar ese valor (memoria segura, `sessionStorage` o flujo que defina el equipo) y enviarlo en las peticiones protegidas.

### Llamadas protegidas

Incluir en cada request:

```http
Authorization: Bearer <access_token>
```

Los endpoints de emisores descritos abajo requieren este encabezado.

## Emisores (Issuers)

Prefijo común: `/api/Issuers`.

| Operación | Método | Ruta | Notas |
|-----------|--------|------|--------|
| Listar | `GET` | `/api/Issuers` | Bearer obligatorio |
| Obtener por id | `GET` | `/api/Issuers/{issuerId}` | `issuerId`: GUID |
| Crear | `POST` | `/api/Issuers` | Bearer + `Content-Type: application/json` |
| Actualizar | `PUT` | `/api/Issuers` | Bearer + JSON; el cuerpo incluye `id` |
| Eliminar | `DELETE` | `/api/Issuers/{issuerId}` | Bearer |

### Crear emisor — cuerpo de ejemplo

Campos según la colección Postman. `certificado_p12` va en **Base64** del archivo `.p12`. La descripción en Postman indica que `certificado_password` se gestiona hacia almacén seguro (Key Vault) en backend.

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
  "numero_contribuyente_especial": null,
  "agente_retencion_resolucion": null,
  "leyenda_rimpe": null,
  "es_gran_contribuyente": false,
  "certificado_p12": "<base64-del-p12>",
  "certificado_password": "<password-del-p12>",
  "vault_secret_name": "issuer-1790012345001-p12-password",
  "fecha_caducidad_cert": "2027-12-31",
  "cert_serial_number": null,
  "cert_sujeto": null,
  "logo_path": null,
  "moneda": "DOLAR",
  "estado": "ACTIVO",
  "version_registro": 1
}
```

### Actualizar emisor — cuerpo de ejemplo

Misma idea que crear; incluye `id` del emisor. En la colección no se envía `certificado_password` en el ejemplo de actualización (sí `certificado_p12` y `vault_secret_name`).

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "ruc": "1790012345001",
  "razon_social": "MI EMPRESA S.A. ACTUALIZADA",
  "nombre_comercial": "MI EMPRESA",
  "direccion_matriz": "Av. Principal 123",
  "direccion_establecimiento": "Sucursal Norte",
  "establecimiento": "001",
  "punto_emision": "001",
  "ambiente": "1",
  "tipo_emision": "1",
  "obligado_contabilidad": "SI",
  "numero_contribuyente_especial": null,
  "agente_retencion_resolucion": null,
  "leyenda_rimpe": null,
  "es_gran_contribuyente": false,
  "certificado_p12": "<base64-del-p12>",
  "vault_secret_name": "issuer-1790012345001-p12-password",
  "fecha_caducidad_cert": "2027-12-31",
  "cert_serial_number": null,
  "cert_sujeto": null,
  "logo_path": null,
  "moneda": "DOLAR",
  "estado": "ACTIVO",
  "version_registro": 1
}
```

## Checklist rápido para el cliente HTTP

1. `POST /api/Auth/token` → leer `access_token`.
2. Guardar token y adjuntar `Authorization: Bearer …` al resto de llamadas.
3. `GET/POST/PUT` en `/api/Issuers` y `GET/DELETE` en `/api/Issuers/{id}` con los cuerpos JSON anteriores cuando aplique.
4. Para subir certificado: leer el `.p12` en el navegador, convertir a Base64 y enviar en `certificado_p12` (payload puede ser grande; valorar límites de tamaño y UX).

## Referencias en el repo

- Colección Postman: `docs/api/FactuX_API.postman_collection.json`
- Rutas del proxy (gateway): `src/FactuxGateway.API/reverseproxy.json`

## CORS y seguridad

Si el frontend se sirve en otro origen que el API/gateway, el servidor debe exponer CORS adecuado; eso se configura en el gateway o en la API de negocio según el despliegue. No exponer credenciales en el repositorio; usar variables de entorno en el frontend para URL base y, si aplica, usuario de prueba solo en desarrollo.
