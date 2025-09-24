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
        /// ✅ SECURE ENDPOINT - Secure authentication using Entity Framework
        /// This endpoint is secure because it uses Entity Framework with parameters
        /// that automatically prevent SQL injection.
        /// 
        /// Entity Framework uses prepared parameters that automatically escape
        /// special characters and prevent SQL injection.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("✅ SECURE LOGIN ATTEMPT - Username: {Username}", request.Username);
                
                // ✅ SECURE: Use Entity Framework with LINQ (uses parameters automatically)
                var user = await _context.Users
                    .Where(u => u.Username == request.Username && u.Password == request.Password)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    _logger.LogInformation("✅ LOGIN SUCCESSFUL - User: {Username}", user.Username);
                    return Ok(new { 
                        message = "Login successful", 
                        user = new { user.Username, user.Email, user.IsAdmin },
                        security = "✅ This endpoint is secure against SQL Injection"
                    });
                }

                _logger.LogWarning("❌ LOGIN FAILED - Invalid credentials for: {Username}", request.Username);
                return Unauthorized(new { message = "Invalid credentials" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in secure login");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// ✅ SECURE ENDPOINT - Secure search using Entity Framework
        /// This endpoint is secure because it uses Entity Framework with parameters
        /// that automatically prevent SQL injection.
        /// 
        /// Contains() in Entity Framework is converted to prepared parameters.
        /// </summary>
        [HttpGet("search")]
        public async Task<IActionResult> SearchProducts([FromQuery] string searchTerm)
        {
            try
            {
                _logger.LogInformation("✅ SECURE SEARCH - Term: {SearchTerm}", searchTerm);
                
                // ✅ SECURE: Use Entity Framework with LINQ (uses parameters automatically)
                var products = await _context.Products
                    .Where(p => p.Name.Contains(searchTerm) || p.Description.Contains(searchTerm))
                    .ToListAsync();

                _logger.LogInformation("✅ SEARCH RESULTS - Found {Count} products", products.Count);
                
                return Ok(new { 
                    products,
                    security = "✅ This endpoint is secure against SQL Injection"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in secure search");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// ✅ SECURE ENDPOINT - Secure insertion using Entity Framework
        /// This endpoint is secure because it uses Entity Framework that handles
        /// query parameterization automatically.
        /// 
        /// Entity Framework prevents SQL injection using prepared parameters.
        /// </summary>
        [HttpPost("comments")]
        public async Task<IActionResult> AddComment([FromBody] CommentRequest request)
        {
            try
            {
                _logger.LogInformation("✅ SECURE COMMENT INSERT - Author: {Author}", request.Author);
                
                // ✅ SECURE: Use Entity Framework (uses parameters automatically)
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
                    message = "Comment added",
                    commentId = comment.Id,
                    security = "✅ This endpoint is secure against SQL Injection"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in secure comment insertion");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// ✅ SECURE ENDPOINT - Secure update using Entity Framework
        /// This endpoint is secure because it uses Entity Framework that handles
        /// query parameterization automatically.
        /// 
        /// Entity Framework prevents SQL injection using prepared parameters.
        /// </summary>
        [HttpPost("admin/update")]
        public async Task<IActionResult> UpdateUser([FromBody] dynamic request)
        {
            try
            {
                var username = request.username?.ToString() ?? "";
                var isAdmin = bool.Parse(request.isAdmin?.ToString() ?? "false");
                
                _logger.LogInformation("✅ SECURE ADMIN UPDATE - Username: {Username}, IsAdmin: {IsAdmin}", username, isAdmin);
                
                // ✅ SECURE: Use Entity Framework with LINQ (uses parameters automatically)
                var user = await _context.Users
                    .Where(u => u.Username == username)
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    user.IsAdmin = isAdmin;
                    await _context.SaveChangesAsync();
                    
                    _logger.LogInformation("✅ ADMIN UPDATE COMPLETED - User: {Username}, IsAdmin: {IsAdmin}", user.Username, user.IsAdmin);
                    
                    return Ok(new { 
                        message = "User updated",
                        user = new { user.Username, user.IsAdmin },
                        security = "✅ This endpoint is secure against SQL Injection"
                    });
                }

                return NotFound(new { message = "User not found" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in secure user update");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }

        /// <summary>
        /// ✅ BONUS: Additional endpoint showing how to use raw SQL securely
        /// If you need to use raw SQL, always use parameters with FromSqlInterpolated
        /// </summary>
        [HttpGet("products/expensive")]
        public async Task<IActionResult> GetExpensiveProducts([FromQuery] decimal minPrice)
        {
            try
            {
                _logger.LogInformation("✅ SECURE RAW SQL - MinPrice: {MinPrice}", minPrice);
                
                // ✅ SECURE: Use FromSqlInterpolated with parameters
                var products = await _context.Products
                    .FromSqlInterpolated($"SELECT * FROM Products WHERE Price >= {minPrice}")
                    .ToListAsync();

                _logger.LogInformation("✅ EXPENSIVE PRODUCTS - Found {Count} products", products.Count);
                
                return Ok(new { 
                    products,
                    security = "✅ This endpoint uses raw SQL securely with parameters"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in secure expensive products query");
                return StatusCode(500, new { message = "Internal server error" });
            }
        }
    }
}
