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
        /// ⚠️ VULNERABLE ENDPOINT - SQL Injection in authentication
        /// This endpoint simulates a SQL Injection vulnerability
        /// In a real scenario, this would be direct string concatenation in SQL
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogWarning("⚠️ VULNERABLE LOGIN ATTEMPT - Username: {Username}", request.Username);
                
                // ⚠️ VULNERABLE: Simulation of direct concatenation
                // In a real scenario it would be: $"SELECT * FROM Users WHERE Username = '{request.Username}' AND Password = '{request.Password}'"
                var query = $"SELECT * FROM Users WHERE Username = '{request.Username}' AND Password = '{request.Password}'";
                
                _logger.LogInformation("🔍 Simulating vulnerable query: {Query}", query);
                
                // Simulation of the vulnerability - search user with vulnerable logic
                var user = await _context.Users
                    .Where(u => u.Username == request.Username && u.Password == request.Password)
                    .FirstOrDefaultAsync();

                // Simular bypass de autenticación si contiene caracteres de SQL Injection
                if (request.Username.Contains("'") || request.Username.Contains("--") || request.Username.Contains("OR"))
                {
                    _logger.LogWarning("🚨 SQL INJECTION DETECTED - Attempting bypass with: {Username}", request.Username);
                    // In a real scenario, this would allow the bypass
                    user = await _context.Users.FirstOrDefaultAsync(); // Get any user
                }

                if (user != null)
                {
                    _logger.LogWarning("🚨 LOGIN SUCCESSFUL - Vulnerable endpoint bypassed! User: {Username}", user.Username);
                    return Ok(new { 
                        message = "Login successful", 
                        user = new { user.Username, user.Email, user.IsAdmin },
                        warning = "⚠️ This endpoint is vulnerable to SQL Injection",
                        vulnerableQuery = query
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
        /// This endpoint simulates a search vulnerability
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string searchTerm)
        {
            try
            {
                _logger.LogWarning("⚠️ VULNERABLE SEARCH - Term: {SearchTerm}", searchTerm);
                
                // ⚠️ VULNERABLE: Simulation of direct concatenation in LIKE
                var query = $"SELECT * FROM Products WHERE Name LIKE '%{searchTerm}%' OR Description LIKE '%{searchTerm}%'";
                
                _logger.LogInformation("🔍 Simulating vulnerable search: {Query}", query);
                
                // Simulate vulnerability - if it contains special characters, return all products
                var products = await _context.Products.ToListAsync();
                
                if (searchTerm.Contains("'") || searchTerm.Contains("--") || searchTerm.Contains("UNION"))
                {
                    _logger.LogWarning("🚨 SQL INJECTION DETECTED - Returning all products due to malicious input");
                    return Ok(new { 
                        products,
                        warning = "⚠️ This endpoint is vulnerable to SQL Injection",
                        vulnerableQuery = query,
                        securityNote = "In a real scenario, this could expose sensitive data"
                    });
                }
                
                // Normal search
                products = products.Where(p => 
                    p.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) || 
                    p.Description.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)
                ).ToList();

                _logger.LogWarning("🚨 SEARCH RESULTS - Found {Count} products", products.Count);
                
                return Ok(new { 
                    products,
                    warning = "⚠️ This endpoint is vulnerable to SQL Injection",
                    vulnerableQuery = query
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
        /// This endpoint simulates an insertion vulnerability
        /// </summary>
        [HttpPost("comments")]
        public async Task<IActionResult> AddComment([FromBody] CommentRequest request)
        {
            try
            {
                _logger.LogWarning("⚠️ VULNERABLE COMMENT INSERT - Author: {Author}", request.Author);
                
                // ⚠️ VULNERABLE: Simulation of direct concatenation in INSERT
                var query = $"INSERT INTO Comments (Content, Author, CreatedAt, IsApproved) VALUES ('{request.Content}', '{request.Author}', '{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}', 0)";
                
                _logger.LogInformation("🔍 Simulating vulnerable insertion: {Query}", query);
                
                // Simulate vulnerability - if it contains special characters, show warning
                if (request.Content.Contains("'") || request.Content.Contains("--") || request.Content.Contains("DROP"))
                {
                    _logger.LogWarning("🚨 SQL INJECTION DETECTED - Malicious content detected: {Content}", request.Content);
                    return Ok(new { 
                        message = "⚠️ SQL Injection attempt detected",
                        warning = "⚠️ This endpoint is vulnerable to SQL Injection",
                        vulnerableQuery = query,
                        securityNote = "In a real scenario, this could execute malicious commands"
                    });
                }

                // Normal insertion
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
                    message = "Comment added",
                    commentId = comment.Id,
                    warning = "⚠️ This endpoint is vulnerable to SQL Injection",
                    vulnerableQuery = query
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
        /// This endpoint simulates an update vulnerability
        /// </summary>
        [HttpPost("admin/update")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserRequest request)
        {
            try
            {
                var username = request.Username;
                var isAdmin = request.IsAdmin.ToString();
                
                _logger.LogWarning("⚠️ VULNERABLE ADMIN UPDATE - Username: {Username}, IsAdmin: {IsAdmin}", username, isAdmin);
                
                // ⚠️ VULNERABLE: Simulation of direct concatenation in UPDATE
                var query = $"UPDATE Users SET IsAdmin = {isAdmin} WHERE Username = '{username}'";
                
                _logger.LogInformation("🔍 Simulating vulnerable update: {Query}", query);
                
                // Simulate vulnerability - if it contains special characters, show warning
                if (username.Contains("'") || username.Contains("--") || username.Contains("UPDATE"))
                {
                    _logger.LogWarning("🚨 SQL INJECTION DETECTED - Malicious username: {Username}", username);
                    return Ok(new { 
                        message = "⚠️ SQL Injection attempt detected",
                        warning = "⚠️ This endpoint is vulnerable to SQL Injection",
                        vulnerableQuery = query,
                        securityNote = "In a real scenario, this could modify unauthorized data"
                    });
                }

                // Normal update
                var user = await _context.Users
                    .Where(u => u.Username == username)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    user.IsAdmin = bool.Parse(isAdmin);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogWarning("🚨 ADMIN UPDATE COMPLETED - User: {Username}, IsAdmin: {IsAdmin}", user.Username, user.IsAdmin);
                    
                    return Ok(new { 
                        message = "User updated",
                        user = new { user.Username, user.IsAdmin },
                        warning = "⚠️ This endpoint is vulnerable to SQL Injection",
                        vulnerableQuery = query
                    });
                }

                return NotFound(new { message = "User not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in vulnerable user update");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}