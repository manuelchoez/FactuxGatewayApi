# Deploy en Render

## Variables requeridas

Define estas variables en tu servicio de Render:

- `ASPNETCORE_ENVIRONMENT=Production`
- `ReverseProxy__Clusters__factuxBusinessApi__Destinations__default__Address=https://TU-BUSINESS-API.onrender.com/`
- `Cors__AllowedOrigins__0=https://fatux-web-app.onrender.com`

## Variable opcional

- `PORT`

Render suele inyectar `PORT` automaticamente. El `Dockerfile` ya arranca la app escuchando en ese puerto.

## Notas

- La URL del backend debe ser absoluta y terminar con `/`.
- Si el frontend vive en otro origen, agrega cada origen permitido con `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, etc.
- En `Production`, el gateway no acepta destinos locales como `localhost` o `127.0.0.1`.
- Las rutas del proxy viven en `src/FactuxGateway.API/reverseproxy.json`.
- El cluster y su destino se pueden definir completamente por variables de entorno, sin editar archivos al desplegar.
