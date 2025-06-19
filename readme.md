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