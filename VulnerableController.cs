using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SqlInjectionWorkshop.Data;
using SqlInjectionWorkshop.Models;
using SqlInjectionWorkshop.Models.DTOs;

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
        /// Este endpoint simula una vulnerabilidad de SQL Injection
        /// En un escenario real, esto sería una concatenación directa de strings en SQL
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogWarning("⚠️ VULNERABLE LOGIN ATTEMPT - Username: {Username}", request.Username);
                
                // ⚠️ VULNERABLE: Simulación de concatenación directa
                // En un escenario real sería: $"SELECT * FROM Users WHERE Username = '{request.Username}' AND Password = '{request.Password}'"
                var query = $"SELECT * FROM Users WHERE Username = '{request.Username}' AND Password = '{request.Password}'";
                
                _logger.LogInformation("🔍 Simulando consulta vulnerable: {Query}", query);
                
                // Simulación de la vulnerabilidad - buscar usuario con lógica vulnerable
                var user = await _context.Users
                    .Where(u => u.Username == request.Username && u.Password == request.Password)
                    .FirstOrDefaultAsync();

                // Simular bypass de autenticación si contiene caracteres de SQL Injection
                if (request.Username.Contains("'") || request.Username.Contains("--") || request.Username.Contains("OR"))
                {
                    _logger.LogWarning("🚨 SQL INJECTION DETECTED - Attempting bypass with: {Username}", request.Username);
                    // En un escenario real, esto permitiría el bypass
                    user = await _context.Users.FirstOrDefaultAsync(); // Obtener cualquier usuario
                }

                if (user != null)
                {
                    _logger.LogWarning("🚨 LOGIN SUCCESSFUL - Vulnerable endpoint bypassed! User: {Username}", user.Username);
                    return Ok(new { 
                        message = "Login exitoso", 
                        user = new { user.Username, user.Email, user.IsAdmin },
                        warning = "⚠️ Este endpoint es vulnerable a SQL Injection",
                        vulnerableQuery = query
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
        /// Este endpoint simula una vulnerabilidad de búsqueda
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string searchTerm)
        {
            try
            {
                _logger.LogWarning("⚠️ VULNERABLE SEARCH - Term: {SearchTerm}", searchTerm);
                
                // ⚠️ VULNERABLE: Simulación de concatenación directa en LIKE
                var query = $"SELECT * FROM Products WHERE Name LIKE '%{searchTerm}%' OR Description LIKE '%{searchTerm}%'";
                
                _logger.LogInformation("🔍 Simulando búsqueda vulnerable: {Query}", query);
                
                // Simular vulnerabilidad - si contiene caracteres especiales, devolver todos los productos
                var products = await _context.Products.ToListAsync();
                
                if (searchTerm.Contains("'") || searchTerm.Contains("--") || searchTerm.Contains("UNION"))
                {
                    _logger.LogWarning("🚨 SQL INJECTION DETECTED - Returning all products due to malicious input");
                    return Ok(new { 
                        products,
                        warning = "⚠️ Este endpoint es vulnerable a SQL Injection",
                        vulnerableQuery = query,
                        securityNote = "En un escenario real, esto podría exponer datos sensibles"
                    });
                }
                
                // Búsqueda normal
                products = products.Where(p => 
                    p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                    p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();

                _logger.LogWarning("🚨 SEARCH RESULTS - Found {Count} products", products.Count);
                
                return Ok(new { 
                    products,
                    warning = "⚠️ Este endpoint es vulnerable a SQL Injection",
                    vulnerableQuery = query
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
        /// Este endpoint simula una vulnerabilidad de inserción
        /// </summary>
        [HttpPost("comments")]
        public async Task<IActionResult> AddComment([FromBody] CommentRequest request)
        {
            try
            {
                _logger.LogWarning("⚠️ VULNERABLE COMMENT INSERT - Author: {Author}", request.Author);
                
                // ⚠️ VULNERABLE: Simulación de concatenación directa en INSERT
                var query = $"INSERT INTO Comments (Content, Author, CreatedAt, IsApproved) VALUES ('{request.Content}', '{request.Author}', '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', 0)";
                
                _logger.LogInformation("🔍 Simulando inserción vulnerable: {Query}", query);
                
                // Simular vulnerabilidad - si contiene caracteres especiales, mostrar advertencia
                if (request.Content.Contains("'") || request.Content.Contains("--") || request.Content.Contains("DROP"))
                {
                    _logger.LogWarning("🚨 SQL INJECTION DETECTED - Malicious content detected: {Content}", request.Content);
                    return Ok(new { 
                        message = "⚠️ Intento de SQL Injection detectado",
                        warning = "⚠️ Este endpoint es vulnerable a SQL Injection",
                        vulnerableQuery = query,
                        securityNote = "En un escenario real, esto podría ejecutar comandos maliciosos"
                    });
                }

                // Inserción normal
                var comment = new Comment
                {
                    Content = request.Content,
                    Author = request.Author,
                    CreatedAt = DateTime.UtcNow,
                    IsApproved = false
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                
                _logger.LogWarning("🚨 COMMENT INSERTED - Vulnerable insertion completed");
                
                return Ok(new { 
                    message = "Comentario agregado",
                    commentId = comment.Id,
                    warning = "⚠️ Este endpoint es vulnerable a SQL Injection",
                    vulnerableQuery = query
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
        /// Este endpoint simula una vulnerabilidad de actualización
        /// </summary>
        [HttpPost("admin/update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
        {
            try
            {
                var username = request.Username;
                var isAdmin = request.IsAdmin.ToString();
                
                _logger.LogWarning("⚠️ VULNERABLE ADMIN UPDATE - Username: {Username}, IsAdmin: {IsAdmin}", username, isAdmin);
                
                // ⚠️ VULNERABLE: Simulación de concatenación directa en UPDATE
                var query = $"UPDATE Users SET IsAdmin = {isAdmin} WHERE Username = '{username}'";
                
                _logger.LogInformation("🔍 Simulando actualización vulnerable: {Query}", query);
                
                // Simular vulnerabilidad - si contiene caracteres especiales, mostrar advertencia
                if (username.Contains("'") || username.Contains("--") || username.Contains("UPDATE"))
                {
                    _logger.LogWarning("🚨 SQL INJECTION DETECTED - Malicious username: {Username}", username);
                    return Ok(new { 
                        message = "⚠️ Intento de SQL Injection detectado",
                        warning = "⚠️ Este endpoint es vulnerable a SQL Injection",
                        vulnerableQuery = query,
                        securityNote = "En un escenario real, esto podría modificar datos no autorizados"
                    });
                }

                // Actualización normal
                var user = await _context.Users
                    .Where(u => u.Username == username)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    user.IsAdmin = bool.Parse(isAdmin);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogWarning("🚨 ADMIN UPDATE COMPLETED - User: {Username}, IsAdmin: {IsAdmin}", user.Username, user.IsAdmin);
                    
                    return Ok(new { 
                        message = "Usuario actualizado",
                        user = new { user.Username, user.IsAdmin },
                        warning = "⚠️ Este endpoint es vulnerable a SQL Injection",
                        vulnerableQuery = query
                    });
                }

                return NotFound(new { message = "Usuario no encontrado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en actualización vulnerable de usuario");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
