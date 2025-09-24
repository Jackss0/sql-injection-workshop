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
        /// ⚠️ VULNERABLE ENDPOINT - SQL Injection in authentication
        /// This endpoint is vulnerable to SQL Injection because it directly concatenates
        /// input parameters in the SQL query without using parameters.
        /// 
        /// Attack example:
        /// Username: admin' OR '1'='1' --
        /// Password: anything
        /// 
        /// This would result in: SELECT * FROM Users WHERE Username = 'admin' OR '1'='1' --' AND Password = 'anything'
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogWarning("⚠️ VULNERABLE LOGIN ATTEMPT - Username: {Username}", request.Username);
                
                // ⚠️ VULNERABLE: Direct string concatenation in SQL
                var query = $"SELECT * FROM Users WHERE Username = '{request.Username}' AND Password = '{request.Password}'";
                
                _logger.LogInformation("🔍 Executing vulnerable query: {Query}", query);
                
                // Execute vulnerable query using FromSqlRaw
                var user = await _context.Users
                    .FromSqlRaw(query)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    _logger.LogWarning("🚨 LOGIN SUCCESSFUL - Vulnerable endpoint bypassed! User: {Username}", user.Username);
                    return Ok(new { 
                        message = "Login successful", 
                        user = new { user.Username, user.Email, user.IsAdmin },
                        warning = "⚠️ This endpoint is vulnerable to SQL Injection"
                    });
                }

                return Unauthorized(new { message = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in vulnerable login");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// ⚠️ VULNERABLE ENDPOINT - SQL Injection in search
        /// This endpoint is vulnerable because it uses direct concatenation in LIKE
        /// 
        /// Attack example:
        /// SearchTerm: %' UNION SELECT Username, Password, Email, IsAdmin FROM Users --
        /// 
        /// This could expose sensitive user information
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string searchTerm)
        {
            try
            {
                _logger.LogWarning("⚠️ VULNERABLE SEARCH - Term: {SearchTerm}", searchTerm);
                
                // ⚠️ VULNERABLE: Direct concatenation in LIKE
                var query = $"SELECT * FROM Products WHERE Name LIKE '%{searchTerm}%' OR Description LIKE '%{searchTerm}%'";
                
                _logger.LogInformation("🔍 Executing vulnerable search: {Query}", query);
                
                var products = await _context.Products
                    .FromSqlRaw(query)
                    .ToListAsync();

                _logger.LogWarning("🚨 SEARCH RESULTS - Found {Count} products", products.Count);
                
                return Ok(new { 
                    products,
                    warning = "⚠️ This endpoint is vulnerable to SQL Injection"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in vulnerable search");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// ⚠️ VULNERABLE ENDPOINT - SQL Injection in comment insertion
        /// This endpoint allows SQL injection in data insertion
        /// 
        /// Attack example:
        /// Content: '); DROP TABLE Comments; --
        /// Author: hacker
        /// 
        /// This could delete the comments table
        /// </summary>
        [HttpPost("comments")]
        public async Task<IActionResult> AddComment([FromBody] CommentRequest request)
        {
            try
            {
                _logger.LogWarning("⚠️ VULNERABLE COMMENT INSERT - Author: {Author}", request.Author);
                
                // ⚠️ VULNERABLE: Direct concatenation in INSERT
                var query = $"INSERT INTO Comments (Content, Author, CreatedAt, IsApproved) VALUES ('{request.Content}', '{request.Author}', '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', 0)";
                
                _logger.LogInformation("🔍 Executing vulnerable insertion: {Query}", query);
                
                await _context.Database.ExecuteSqlRawAsync(query);
                
                _logger.LogWarning("🚨 COMMENT INSERTED - Vulnerable insertion completed");
                
                return Ok(new { 
                    message = "Comment added",
                    warning = "⚠️ This endpoint is vulnerable to SQL Injection"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in vulnerable comment insertion");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// ⚠️ VULNERABLE ENDPOINT - SQL Injection in data update
        /// This endpoint allows data modification using SQL Injection
        /// 
        /// Attack example:
        /// Username: admin'; UPDATE Users SET IsAdmin = 1 WHERE Username = 'user1'; --
        /// 
        /// This could grant administrator privileges to normal users
        /// </summary>
        [HttpPost("admin/update")]
        public async Task<IActionResult> UpdateUser([FromBody] dynamic request)
        {
            try
            {
                var username = request.username?.ToString() ?? "";
                var isAdmin = request.isAdmin?.ToString() ?? "false";
                
                _logger.LogWarning("⚠️ VULNERABLE ADMIN UPDATE - Username: {Username}, IsAdmin: {IsAdmin}", username, isAdmin);
                
                // ⚠️ VULNERABLE: Direct concatenation in UPDATE
                var query = $"UPDATE Users SET IsAdmin = {isAdmin} WHERE Username = '{username}'";
                
                _logger.LogInformation("🔍 Executing vulnerable update: {Query}", query);
                
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(query);
                
                _logger.LogWarning("🚨 ADMIN UPDATE COMPLETED - Rows affected: {RowsAffected}", rowsAffected);
                
                return Ok(new { 
                    message = "User updated",
                    rowsAffected,
                    warning = "⚠️ This endpoint is vulnerable to SQL Injection"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in vulnerable user update");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
