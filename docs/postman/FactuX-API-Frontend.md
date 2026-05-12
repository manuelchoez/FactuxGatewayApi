# FactuX API - Guia para frontend

Este documento resume el contrato HTTP que usa la coleccion Postman `docs/api/FactuX_API.postman_collection.json`, para que el frontend pueda implementar cliente HTTP, manejo de JWT y formularios de emisor y producto sin depender de Postman.

## URL base

- En Postman la variable `baseUrl` por defecto es `https://localhost:7001`. Ese valor es solo un ejemplo de entorno.
- El gateway de este repositorio (`FactuxGateway.API`) en desarrollo suele exponerse segun `src/FactuxGateway.API/Properties/launchSettings.json`, por ejemplo:
- `http://localhost:5051`
- `https://localhost:7232` (perfil `https`)

Configura en el frontend una variable de entorno, por ejemplo `VITE_API_BASE_URL` o `NEXT_PUBLIC_API_BASE_URL`, con la URL del gateway o de la API que consumas en cada ambiente. Todas las rutas de abajo son relativas a esa base. Idealmente usa una URL sin barra final, o normalizala en tu cliente HTTP.

## Autenticacion (JWT)

### Obtener token

| Campo | Valor |
|--------|--------|
| Metodo | `POST` |
| Ruta | `/api/Auth/token` |
| `Content-Type` | `application/json` |

**Cuerpo (JSON):**

```json
{
  "username": "admin",
  "password": "admin123"
}
```

**Respuesta esperada (segun scripts de Postman):** JSON con propiedad `access_token` de tipo `string`. El frontend debe guardar ese valor y enviarlo en las peticiones protegidas.

### Llamadas protegidas

Incluir en cada request:

```http
Authorization: Bearer <access_token>
```

Los endpoints de `Issuers` y `Products` requieren este encabezado.

## Emisores (Issuers)

Prefijo comun: `/api/Issuers`.

| Operacion | Metodo | Ruta | Notas |
|-----------|--------|------|--------|
| Listar | `GET` | `/api/Issuers` | Bearer obligatorio |
| Obtener por id | `GET` | `/api/Issuers/{issuerId}` | `issuerId`: GUID |
| Crear | `POST` | `/api/Issuers` | Bearer + `Content-Type: application/json` |
| Actualizar | `PUT` | `/api/Issuers` | Bearer + JSON; el cuerpo incluye `id` |
| Eliminar | `DELETE` | `/api/Issuers/{issuerId}` | Bearer |

### Crear emisor - cuerpo de ejemplo

Campos segun la coleccion Postman. `certificado_p12` va en Base64 del archivo `.p12`. La descripcion en Postman indica que `certificado_password` se gestiona hacia almacen seguro (Key Vault) en backend.

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

### Actualizar emisor - cuerpo de ejemplo

Misma idea que crear; incluye `id` del emisor. En la coleccion no se envia `certificado_password` en el ejemplo de actualizacion, pero si `certificado_p12` y `vault_secret_name`.

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

## Productos (Products)

Prefijo comun: `/api/Products`.

Cada producto pertenece a un `Issuer`, por lo que el frontend debe enviar `issuer_id` en crear y actualizar.

| Operacion | Metodo | Ruta | Notas |
|-----------|--------|------|--------|
| Listar | `GET` | `/api/Products` | Bearer obligatorio |
| Obtener por id | `GET` | `/api/Products/{productId}` | `productId`: GUID |
| Crear | `POST` | `/api/Products` | Bearer + `Content-Type: application/json` |
| Actualizar | `PUT` | `/api/Products` | Bearer + JSON; el cuerpo incluye `id` |
| Eliminar | `DELETE` | `/api/Products/{productId}` | Bearer |

### Crear producto - cuerpo de ejemplo

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
  "codigo_descripcion_ice": null,
  "estado": "ACTIVO",
  "version_registro": 1
}
```

### Actualizar producto - cuerpo de ejemplo

```json
{
  "id": "00000000-0000-0000-0000-000000000000",
  "issuer_id": "00000000-0000-0000-0000-000000000000",
  "codigo_principal": "SPTI",
  "codigo_auxiliar": "SPTI-002",
  "nombre": "SERVICIO PROFESIONAL TECNICO INFORMATICO ACTUALIZADO",
  "precio_unitario": 1999.99,
  "informacion_adicional": "Producto actualizado de prueba asociado al emisor",
  "tarifa_iva": "15%",
  "aplica_iva_turismo": false,
  "ice": false,
  "codigo_descripcion_ice": null,
  "estado": "ACTIVO",
  "version_registro": 1
}
```

### Consideraciones de formulario para producto

- `issuer_id`: obligatorio; debe corresponder a un emisor existente.
- `codigo_principal`: maximo 25 caracteres.
- `codigo_auxiliar`: maximo 25 caracteres.
- `nombre`: obligatorio.
- `precio_unitario`: numerico decimal.
- `tarifa_iva`: valor tipo catalogo, por ejemplo `0%`, `5%`, `15%`, `EXENTO IVA`, `NO OBJETO IMPUESTO`.
- `aplica_iva_turismo`: booleano.
- `ice`: booleano.
- `codigo_descripcion_ice`: usarlo cuando `ice = true`.

## Checklist rapido para el cliente HTTP

1. `POST /api/Auth/token` y leer `access_token`.
2. Guardar token y adjuntar `Authorization: Bearer ...` al resto de llamadas.
3. Consumir CRUD de emisores en `/api/Issuers`.
4. Consumir CRUD de productos en `/api/Products`.
5. En formularios de productos, enviar siempre `issuer_id`.
6. Para subir certificado: leer el `.p12` en el navegador, convertir a Base64 y enviar en `certificado_p12`.

## Referencias en el repo

- Coleccion Postman: `docs/api/FactuX_API.postman_collection.json`
- Rutas del proxy: `src/FactuxGateway.API/reverseproxy.json`
- Configuracion local del gateway: `src/FactuxGateway.API/Properties/launchSettings.json`

## CORS y seguridad

Si el frontend se sirve desde otro origen que el API o el gateway, el servidor debe exponer CORS adecuado para ese ambiente. No expongas credenciales en el repositorio; usa variables de entorno en el frontend para URL base y, si aplica, usuarios de prueba solo en desarrollo.
