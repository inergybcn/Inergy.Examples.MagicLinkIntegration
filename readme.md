# Inergy.Examples.MagicLinkIntegration

Este repositorio es un ejemplo de cómo integrar un aplicación con **SIE** utilizando un **Magic Link** que ofrece el servicio de **Autenticación** de **Inergy**.
Se trata de un proyecto **ASP .NET Core**.

Para poder ejecutar el proyecto se necesita:
- Un usuario **Administrador** *username + password* que es único por aplicación a integrar y es que tiene los permisos para crear **Magic Link**. Proporcionado por **Inergy**.
- Usuarios *passwordless*, en adelante 'usuarios', previamente registrados en **SIE** con un *username* equivalente a su correo electrónico. 
    - Estos usuarios deben crearse previamente en SIE y estar dados de alta en un mismo MagicLinkGroup. Responsabilidad de **Inergy**.

Los usuarios no acceden a **SIE** utilizando *username + password*, sino que el usuario **Administrador** solicita los accesos a los diferentes usuarios generando una **URL** de acceso *passwordless*.

## Cómo ejecutar este código en Visual Studio Code
- Clonar el repositorio y abrirlo con *Visual Studio Code*.
- Añadir las siguientes dependencias:
    - [.Net 9 SDK](https://dotnet.microsoft.com/es-es/download/dotnet/9.0)
    - *C# Dev Kit extension*, instalable desde la pestaña de extensiones de *Visual Studio Code*.
- Simplemente con **F5** se compilará y empezará el *debug*.

## Cómo ejecutar este código en Visual Studio 2022
- Tener actualizado *Visual Studio 2022*.
- Clonar el repositorio desde la interfaz de *Visual Studio 2022*.
- Simplemente con **F5** se compilará y empezará el *debug*.

## Configuración
- En **appsettings.json** añadir el valor de la clave *MasterUser.Email* del usuario **Administrador**.
- Para pruebas se puede añadir la clave *MasterUser.Password* del usuario **Administrador**, pero se recomienda el uso de un servicio de gestión de secretos.
- La clave *PasswordlessUser.Email* es el *username* del usaurio que accede a **SIE** con el **Magic Link**. Esta configuración sólo sirve para pruebas y en producción debe obtenerse la información del usuario de la aplicación que se integra en **SIE**.

## Parámetros extra
Se pueden añadir parámetros extra de carácter opcional:
- **Idioma**: Parámetro *lang*. Si no se añade se toma el valor por defecto para **catalán**. Valores disponibles:

| Lang | Language |
| -- | -- |
| ca | Catalá (default) |
| cs | Czech |
| el | Greek |
| en | English |
| es | Castellano |
| eu | Euskera |

- **Code**: Parámetro *Code*. Entero opcional que equivale al nº de factura de un suministro, que habilita el acceso directo a la **Ficha de Suministro**.
- Estos parametros, que son opcionales, se pueden añadir al **Magic Link** de la siguiente manera:

``` c#
url = url + "&lang=es&code=12345";
```

# Llamadas HTTP para otros frameworks
Si no quiere utilizar **.NET**, se pueden encontrar aqui las llamadas **HTTP** que se utilizan, para integrarlas en el *stack* de la aplicación a integrar en **SIE**.

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
