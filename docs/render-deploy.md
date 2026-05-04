# Deploy en Render

## Variables requeridas

Define estas variables en tu servicio de Render:

- `ASPNETCORE_ENVIRONMENT=Production`
- `ReverseProxy__Clusters__factuxBusinessApi__Destinations__default__Address=https://TU-BUSINESS-API.onrender.com/`

## Variable opcional

- `PORT`

Render suele inyectar `PORT` automaticamente. El `Dockerfile` ya arranca la app escuchando en ese puerto.

## Notas

- La URL del backend debe ser absoluta y terminar con `/`.
- En `Production`, el gateway no acepta destinos locales como `localhost` o `127.0.0.1`.
- Las rutas del proxy viven en `src/FactuxGateway.API/reverseproxy.json`.
- El cluster y su destino se pueden definir completamente por variables de entorno, sin editar archivos al desplegar.
