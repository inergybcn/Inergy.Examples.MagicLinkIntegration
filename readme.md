# Inergy.Examples.MagicLinkIntegration

Este repositorio es un ejemplo de cómo integrarte con SIE utilizando la tecnología MagicLink. Para ello necesitamos:
- Un usuario master
- n Usuarios passwordless, en adelante 'usuarios'

La idea es que los usuarios no hagan login con usuario y contraseña, sino que el master solicite los accesos a los diferentes usuarios.

## Cómo ejecutar este código en VS Code
- Clona el repositorio y abrelo con Visual Studio Code
- Necesitarás las siguientes dependencias:
    - .Net 9 SDK (desde la web https://dotnet.microsoft.com/es-es/download/dotnet/9.0)
    - C# Dev Kit extension (desde la pestaña de extensiones de VSCode)
- Simplemente con F5 se compilará y empezará el debug.

## Configuración
En appsettings necesitas configurar el usuario master con su password y al usuario passwordless que quieras probar. 

Tiene que haber sido creado previamente en SIE y estar dados de alta en el mismo MagicLinkGroup


## Parámetros extra
Si necesitas añadir parámetros extra, como el idioma o algún código para SIE, puedes añadirlo a continuación

``` c#
url = url + "&lang=es&code=12345";
```

# Llamadas HTTP para otros frameworks
Si no quieres utilizar .Net como entorno, puedes encontrar aqui las llamadas http que se utilizan, para integrarlas en tu framework

### Master user login
#### cUrl

``` powershell
curl --location 'https://sie-auth.inergy.online/account/login' \
--header 'Content-Type: application/json' \
--data-raw '{"email":"","password":""}'

```

#### wget
``` powershell
wget --no-check-certificate --quiet \
  --method POST \
  --timeout=0 \
  --header 'Content-Type: application/json' \
  --body-data '{"email":"","password":""}' \
   'https://sie-auth.inergy.online/account/login'
```

#### Response
``` json
{
    "token": "ey...",
    "expiration": "2025-06-19T22:27:10Z"
}
```

### Request magic link

#### cUrl

``` powershell
curl --location --request POST 'https://sie-auth.inergy.online/sie/login_magic_link/{email}' \
--header 'Authorization: ey...'

```

#### wget
``` powershell
wget --no-check-certificate --quiet \
  --method POST \
  --timeout=0 \
  --header 'Authorization: Bearer ey...' \
   'https://sie-auth.inergy.online/sie/login_magic_link/{email}'
```

#### Response
``` json
{
  "token": "ey...",
  "expiration": "2025-06-26T19:39:45Z",
  "url": "https://sie-dev.inergy.online/test/login/magic-link?token=ey..."
}
```
