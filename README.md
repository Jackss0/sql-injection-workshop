# SQL Injection Workshop - API de Demostración

Esta API está diseñada específicamente para fines educativos y demuestra tanto vulnerabilidades de SQL Injection como sus soluciones seguras.

## ⚠️ ADVERTENCIA IMPORTANTE

**Esta API contiene vulnerabilidades intencionales para fines educativos. NUNCA uses este código en producción.**

## 🚀 Inicio Rápido

### Prerrequisitos
- .NET 8.0 SDK
- Visual Studio Code o Visual Studio

### Ejecutar la Aplicación

```bash
# Navegar al directorio del proyecto
cd SqlInjectionWorkshop

# Restaurar paquetes
dotnet restore

# Ejecutar la aplicación
dotnet run
```

La API estará disponible en:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `http://localhost:5000` (en desarrollo)

## 📚 Endpoints de Demostración

### Endpoints Vulnerables (`/api/vulnerable`)

#### 1. POST `/api/vulnerable/login` - Autenticación Vulnerable
**Vulnerabilidad**: Concatenación directa de strings en SQL

**Ejemplo de ataque**:
```json
{
  "username": "admin' OR '1'='1' --",
  "password": "cualquier_cosa"
}
```

#### 2. GET `/api/vulnerable/search?searchTerm=...` - Búsqueda Vulnerable
**Vulnerabilidad**: Concatenación directa en LIKE

**Ejemplo de ataque**:
```
GET /api/vulnerable/search?searchTerm=%' UNION SELECT Username, Password, Email, IsAdmin FROM Users --
```

#### 3. POST `/api/vulnerable/comments` - Inserción Vulnerable
**Vulnerabilidad**: Concatenación directa en INSERT

**Ejemplo de ataque**:
```json
{
  "content": "'); DROP TABLE Comments; --",
  "author": "hacker"
}
```

#### 4. POST `/api/vulnerable/admin/update` - Actualización Vulnerable
**Vulnerabilidad**: Concatenación directa en UPDATE

**Ejemplo de ataque**:
```json
{
  "username": "admin'; UPDATE Users SET IsAdmin = 1 WHERE Username = 'user1'; --",
  "isAdmin": true
}
```

### Endpoints Seguros (`/api/secure`)

#### 1. POST `/api/secure/login` - Autenticación Segura
**Protección**: Entity Framework con parámetros automáticos

#### 2. GET `/api/secure/search?searchTerm=...` - Búsqueda Segura
**Protección**: Entity Framework con LINQ

#### 3. POST `/api/secure/comments` - Inserción Segura
**Protección**: Entity Framework con objetos

#### 4. POST `/api/secure/admin/update` - Actualización Segura
**Protección**: Entity Framework con LINQ

#### 5. GET `/api/secure/products/expensive?minPrice=...` - SQL Crudo Seguro
**Protección**: FromSqlInterpolated con parámetros

## 🛠️ Características del Workshop

### Datos de Prueba Incluidos
- **Usuarios**:
  - `admin` / `admin123` (IsAdmin: true)
  - `user1` / `password123` (IsAdmin: false)
  - `test` / `test123` (IsAdmin: false)

- **Productos**:
  - Laptop ($999.99)
  - Mouse ($29.99)
  - Teclado ($89.99)

- **Comentarios**:
  - Varios comentarios de ejemplo

### Logging Detallado
La aplicación incluye logging detallado que muestra:
- ⚠️ Intentos de ataque en endpoints vulnerables
- ✅ Operaciones seguras
- 🔍 Consultas SQL ejecutadas
- 🚨 Resultados de ataques exitosos

### Base de Datos en Memoria
- Se reinicia con cada ejecución
- Datos de prueba se cargan automáticamente
- Perfecto para demostraciones

## 🎯 Objetivos del Workshop

1. **Identificar vulnerabilidades**: Reconocer código vulnerable a SQL Injection
2. **Ejecutar ataques**: Practicar técnicas de SQL Injection
3. **Implementar soluciones**: Aprender técnicas de prevención
4. **Analizar logs**: Entender cómo detectar intentos de ataque

## 🔒 Mejores Prácticas Demostradas

### ❌ Lo que NO hacer (Vulnerable)
```csharp
// ❌ VULNERABLE - Concatenación directa
var query = $"SELECT * FROM Users WHERE Username = '{username}'";
```

### ✅ Lo que SÍ hacer (Seguro)
```csharp
// ✅ SECURE - Entity Framework con parámetros
var user = await _context.Users
    .Where(u => u.Username == username)
    .FirstOrDefaultAsync();
```

## 📖 Recursos Adicionales

- [OWASP SQL Injection Prevention](https://owasp.org/www-community/attacks/SQL_Injection)
- [Entity Framework Security](https://docs.microsoft.com/en-us/ef/core/querying/raw-sql)
- [ASP.NET Core Security](https://docs.microsoft.com/en-us/aspnet/core/security/)

## 🚨 Notas de Seguridad

- Esta API es solo para fines educativos
- No contiene autenticación real
- No implementa autorización
- Los datos son ficticios
- Nunca usar en producción

## 🤝 Contribuciones

Este proyecto está diseñado para ser un recurso educativo. Las contribuciones para mejorar la claridad y agregar más ejemplos son bienvenidas.

## 📄 Licencia

Este proyecto está bajo la Licencia MIT - ver el archivo LICENSE para detalles.
