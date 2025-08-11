# ShopMVP

ASP.NET Core e-commerce MVP (electronics)



First try making a commit via Git Bash
Commit via VS

## Stack:

\*\*.NET 8 LTS\*\*, Windows 11, \*\*Visual Studio 2022 Community\*\*.

How to run locally
<img width="1902" height="639" alt="image" src="https://github.com/user-attachments/assets/9688cd0e-39b2-4402-8da8-b9d2a2cc97c4" />

## CodeStyle
`dotnet format`
`dotnet build`

`.editorconfig` файл расположен в корневой папки вместе с `.sln`

## Swagger (Dev/Prod)

**Dev (по умолчанию из Visual Studio)**
- Открой: `https://localhost:{порт}/swagger`
- JSON-схема: `https://localhost:{порт}/swagger/v1/swagger.json`
- Заголовок: **ShopMVP API**, версия **v1** (описание берётся из XML-комментариев проекта `ShopMVP.Api`)

**Prod (эмуляция локально)**
> В продакшене Swagger отключён. Чтобы проверить это локально, запусти API в среде Production.

PowerShell
```powershell
$env:ASPNETCORE_ENVIRONMENT="Production"
dotnet run --project .\src\ShopMVP.Api --no-launch-profile
```

```cmd
set ASPNETCORE_ENVIRONMENT=Production && dotnet run --project src\ShopMVP.Api --no-launch-profile
```

Git Bash
```
ASPNETCORE_ENVIRONMENT=Production dotnet run --project ./src/ShopMVP.Api --no-launch-profile
```

В логах должно быть: `Hosting environment: Production`
Проверь: `http://localhost:{порт}/swagger` → недоступен (UI и JSON выключены)
Чтобы вернуться в Dev, запускай обычным способом из VS или убери переменную окружения.