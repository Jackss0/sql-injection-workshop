using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SqlInjectionWorkshop.Data;
using SqlInjectionWorkshop.Models;
using SqlInjectionWorkshop.Models.DTOs;
using System.Data;

namespace SqlInjectionWorkshop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VulnerableController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<VulnerableController> _logger;

        public VulnerableController(ApplicationDbContext context, ILogger<VulnerableController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// ⚠️ VULNERABLE ENDPOINT - SQL Injection en autenticación
        /// Este endpoint es vulnerable a SQL Injection porque concatena directamente
        /// los parámetros de entrada en la consulta SQL sin usar parámetros.
        /// 
        /// Ejemplo de ataque:
        /// Username: admin' OR '1'='1' --
        /// Password: cualquier cosa
        /// 
        /// Esto resultaría en: SELECT * FROM Users WHERE Username = 'admin' OR '1'='1' --' AND Password = 'cualquier cosa'
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogWarning("⚠️ VULNERABLE LOGIN ATTEMPT - Username: {Username}", request.Username);
                
                // ⚠️ VULNERABLE: Concatenación directa de strings en SQL
                var query = $"SELECT * FROM Users WHERE Username = '{request.Username}' AND Password = '{request.Password}'";
                
                _logger.LogInformation("🔍 Ejecutando consulta vulnerable: {Query}", query);
                
                // Ejecutar consulta vulnerable usando FromSqlRaw
                var user = await _context.Users
                    .FromSqlRaw(query)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    _logger.LogWarning("🚨 LOGIN SUCCESSFUL - Vulnerable endpoint bypassed! User: {Username}", user.Username);
                    return Ok(new { 
                        message = "Login exitoso", 
                        user = new { user.Username, user.Email, user.IsAdmin },
                        warning = "⚠️ Este endpoint es vulnerable a SQL Injection"
                    });
                }

                return Unauthorized(new { message = "Credenciales inválidas" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login vulnerable");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// ⚠️ VULNERABLE ENDPOINT - SQL Injection en búsqueda
        /// Este endpoint es vulnerable porque usa concatenación directa en LIKE
        /// 
        /// Ejemplo de ataque:
        /// SearchTerm: %' UNION SELECT Username, Password, Email, IsAdmin FROM Users --
        /// 
        /// Esto podría exponer información sensible de usuarios
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string searchTerm)
        {
            try
            {
                _logger.LogWarning("⚠️ VULNERABLE SEARCH - Term: {SearchTerm}", searchTerm);
                
                // ⚠️ VULNERABLE: Concatenación directa en LIKE
                var query = $"SELECT * FROM Products WHERE Name LIKE '%{searchTerm}%' OR Description LIKE '%{searchTerm}%'";
                
                _logger.LogInformation("🔍 Ejecutando búsqueda vulnerable: {Query}", query);
                
                var products = await _context.Products
                    .FromSqlRaw(query)
                    .ToListAsync();

                _logger.LogWarning("🚨 SEARCH RESULTS - Found {Count} products", products.Count);
                
                return Ok(new { 
                    products,
                    warning = "⚠️ Este endpoint es vulnerable a SQL Injection"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda vulnerable");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// ⚠️ VULNERABLE ENDPOINT - SQL Injection en inserción de comentarios
        /// Este endpoint permite inyección SQL en la inserción de datos
        /// 
        /// Ejemplo de ataque:
        /// Content: '); DROP TABLE Comments; --
        /// Author: hacker
        /// 
        /// Esto podría eliminar la tabla de comentarios
        /// </summary>
        [HttpPost("comments")]
        public async Task<IActionResult> AddComment([FromBody] CommentRequest request)
        {
            try
            {
                _logger.LogWarning("⚠️ VULNERABLE COMMENT INSERT - Author: {Author}", request.Author);
                
                // ⚠️ VULNERABLE: Concatenación directa en INSERT
                var query = $"INSERT INTO Comments (Content, Author, CreatedAt, IsApproved) VALUES ('{request.Content}', '{request.Author}', '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', 0)";
                
                _logger.LogInformation("🔍 Ejecutando inserción vulnerable: {Query}", query);
                
                await _context.Database.ExecuteSqlRawAsync(query);
                
                _logger.LogWarning("🚨 COMMENT INSERTED - Vulnerable insertion completed");
                
                return Ok(new { 
                    message = "Comentario agregado",
                    warning = "⚠️ Este endpoint es vulnerable a SQL Injection"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en inserción vulnerable de comentario");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// ⚠️ VULNERABLE ENDPOINT - SQL Injection en actualización de datos
        /// Este endpoint permite modificar datos usando SQL Injection
        /// 
        /// Ejemplo de ataque:
        /// Username: admin'; UPDATE Users SET IsAdmin = 1 WHERE Username = 'user1'; --
        /// 
        /// Esto podría otorgar privilegios de administrador a usuarios normales
        /// </summary>
        [HttpPost("admin/update")]
        public async Task<IActionResult> UpdateUser([FromBody] dynamic request)
        {
            try
            {
                var username = request.username?.ToString() ?? "";
                var isAdmin = request.isAdmin?.ToString() ?? "false";
                
                _logger.LogWarning("⚠️ VULNERABLE ADMIN UPDATE - Username: {Username}, IsAdmin: {IsAdmin}", username, isAdmin);
                
                // ⚠️ VULNERABLE: Concatenación directa en UPDATE
                var query = $"UPDATE Users SET IsAdmin = {isAdmin} WHERE Username = '{username}'";
                
                _logger.LogInformation("🔍 Ejecutando actualización vulnerable: {Query}", query);
                
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(query);
                
                _logger.LogWarning("🚨 ADMIN UPDATE COMPLETED - Rows affected: {RowsAffected}", rowsAffected);
                
                return Ok(new { 
                    message = "Usuario actualizado",
                    rowsAffected,
                    warning = "⚠️ Este endpoint es vulnerable a SQL Injection"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en actualización vulnerable de usuario");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
