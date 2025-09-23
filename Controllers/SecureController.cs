using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SqlInjectionWorkshop.Data;
using SqlInjectionWorkshop.Models;
using SqlInjectionWorkshop.Models.DTOs;
using System.Security.Cryptography;
using System.Text;

namespace SqlInjectionWorkshop.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SecureController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<SecureController> _logger;

        public SecureController(ApplicationDbContext context, ILogger<SecureController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// ✅ SECURE ENDPOINT - Autenticación segura usando Entity Framework
        /// Este endpoint es seguro porque usa Entity Framework con parámetros
        /// que previenen la inyección SQL automáticamente.
        /// 
        /// Entity Framework usa parámetros preparados que escapan automáticamente
        /// los caracteres especiales y previenen la inyección SQL.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("✅ SECURE LOGIN ATTEMPT - Username: {Username}", request.Username);
                
                // ✅ SECURE: Usar Entity Framework con LINQ (usa parámetros automáticamente)
                var user = await _context.Users
                    .Where(u => u.Username == request.Username && u.Password == request.Password)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    _logger.LogInformation("✅ LOGIN SUCCESSFUL - User: {Username}", user.Username);
                    return Ok(new { 
                        message = "Login exitoso", 
                        user = new { user.Username, user.Email, user.IsAdmin },
                        security = "✅ Este endpoint es seguro contra SQL Injection"
                    });
                }

                _logger.LogWarning("❌ LOGIN FAILED - Invalid credentials for: {Username}", request.Username);
                return Unauthorized(new { message = "Credenciales inválidas" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en login seguro");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// ✅ SECURE ENDPOINT - Búsqueda segura usando Entity Framework
        /// Este endpoint es seguro porque usa Entity Framework con parámetros
        /// que previenen la inyección SQL automáticamente.
        /// 
        /// Contains() en Entity Framework se convierte en parámetros preparados.
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string searchTerm)
        {
            try
            {
                _logger.LogInformation("✅ SECURE SEARCH - Term: {SearchTerm}", searchTerm);
                
                // ✅ SECURE: Usar Entity Framework con LINQ (usa parámetros automáticamente)
                var products = await _context.Products
                    .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
                    .ToListAsync();

                _logger.LogInformation("✅ SEARCH RESULTS - Found {Count} products", products.Count);
                
                return Ok(new { 
                    products,
                    security = "✅ Este endpoint es seguro contra SQL Injection"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en búsqueda segura");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// ✅ SECURE ENDPOINT - Inserción segura usando Entity Framework
        /// Este endpoint es seguro porque usa Entity Framework que maneja
        /// automáticamente la parametrización de consultas.
        /// 
        /// Entity Framework previene la inyección SQL usando parámetros preparados.
        /// </summary>
        [HttpPost("comments")]
        public async Task<IActionResult> AddComment([FromBody] CommentRequest request)
        {
            try
            {
                _logger.LogInformation("✅ SECURE COMMENT INSERT - Author: {Author}", request.Author);
                
                // ✅ SECURE: Usar Entity Framework (usa parámetros automáticamente)
                var comment = new Comment
                {
                    Content = request.Content,
                    Author = request.Author,
                    CreatedAt = DateTime.UtcNow,
                    IsApproved = false
                };

                _context.Comments.Add(comment);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("✅ COMMENT INSERTED - Comment ID: {Id}", comment.Id);
                
                return Ok(new { 
                    message = "Comentario agregado",
                    commentId = comment.Id,
                    security = "✅ Este endpoint es seguro contra SQL Injection"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en inserción segura de comentario");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// ✅ SECURE ENDPOINT - Actualización segura usando Entity Framework
        /// Este endpoint es seguro porque usa Entity Framework que maneja
        /// automáticamente la parametrización de consultas.
        /// 
        /// Entity Framework previene la inyección SQL usando parámetros preparados.
        /// </summary>
        [HttpPost("admin/update")]
        public async Task<IActionResult> UpdateUser([FromBody] dynamic request)
        {
            try
            {
                var username = request.username?.ToString() ?? "";
                var isAdmin = bool.Parse(request.isAdmin?.ToString() ?? "false");
                
                _logger.LogInformation("✅ SECURE ADMIN UPDATE - Username: {Username}, IsAdmin: {IsAdmin}", username, isAdmin);
                
                // ✅ SECURE: Usar Entity Framework con LINQ (usa parámetros automáticamente)
                var user = await _context.Users
                    .Where(u => u.Username == username)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    user.IsAdmin = isAdmin;
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("✅ ADMIN UPDATE COMPLETED - User: {Username}, IsAdmin: {IsAdmin}", user.Username, user.IsAdmin);
                    
                    return Ok(new { 
                        message = "Usuario actualizado",
                        user = new { user.Username, user.IsAdmin },
                        security = "✅ Este endpoint es seguro contra SQL Injection"
                    });
                }

                return NotFound(new { message = "Usuario no encontrado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en actualización segura de usuario");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// ✅ BONUS: Endpoint adicional que muestra cómo usar SQL crudo de forma segura
        /// Si necesitas usar SQL crudo, siempre usa parámetros con FromSqlInterpolated
        /// </summary>
        [HttpGet("products/expensive")]
        public async Task<IActionResult> GetExpensiveProducts([FromQuery] decimal minPrice)
        {
            try
            {
                _logger.LogInformation("✅ SECURE RAW SQL - MinPrice: {MinPrice}", minPrice);
                
                // ✅ SECURE: Usar FromSqlInterpolated con parámetros
                var products = await _context.Products
                    .FromSqlInterpolated($"SELECT * FROM Products WHERE Price >= {minPrice}")
                    .ToListAsync();

                _logger.LogInformation("✅ EXPENSIVE PRODUCTS - Found {Count} products", products.Count);
                
                return Ok(new { 
                    products,
                    security = "✅ Este endpoint usa SQL crudo de forma segura con parámetros"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en consulta segura de productos caros");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}
