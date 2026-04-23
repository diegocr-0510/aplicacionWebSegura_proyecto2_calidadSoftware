using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto2Seguridad.Web.Data;
using Proyecto2Seguridad.Web.DTOs;
using Proyecto2Seguridad.Web.Models;
using Proyecto2Seguridad.Web.Services;

namespace Proyecto2Seguridad.Web.Controllers.Api
{
    [ApiController]
    [Route("api/products")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Registrador,Auditor")]
    public class ProductsApiController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public ProductsApiController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        // Obtener todos los productos
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var products = await _context.Products
                .OrderBy(p => p.Name)
                .Select(p => new ProductApiDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Description = p.Description,
                    Quantity = p.Quantity,
                    Price = p.Price
                })
                .ToListAsync();

            return Ok(products);
        }

        // Obtener un producto por id
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var product = await _context.Products
                .Where(p => p.Id == id)
                .Select(p => new ProductApiDto
                {
                    Id = p.Id,
                    Code = p.Code,
                    Name = p.Name,
                    Description = p.Description,
                    Quantity = p.Quantity,
                    Price = p.Price
                })
                .FirstOrDefaultAsync();

            if (product == null)
            {
                return NotFound(new { message = "Producto no encontrado." });
            }

            return Ok(product);
        }

        // Crear producto
        [HttpPost]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Registrador")]
        public async Task<IActionResult> Create([FromBody] ProductApiDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Validar código único
            var codeExists = await _context.Products.AnyAsync(p => p.Code == model.Code);

            if (codeExists)
            {
                return BadRequest(new { message = "Ya existe un producto con ese código." });
            }

            var product = new Product
            {
                Code = model.Code,
                Name = model.Name,
                Description = model.Description,
                Quantity = model.Quantity,
                Price = model.Price,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            await RegisterAuditAsync(
                "API_PRODUCT_CREATE",
                $"Se creó por API el producto {product.Name} con código {product.Code}");

            model.Id = product.Id;

            return CreatedAtAction(nameof(GetById), new { id = product.Id }, model);
        }

        // Editar producto
        [HttpPut("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Registrador")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductApiDto model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(new { message = "Producto no encontrado." });
            }

            // Validar código único excluyendo el mismo registro
            var codeExists = await _context.Products.AnyAsync(p => p.Code == model.Code && p.Id != id);

            if (codeExists)
            {
                return BadRequest(new { message = "Ya existe otro producto con ese código." });
            }

            product.Code = model.Code;
            product.Name = model.Name;
            product.Description = model.Description;
            product.Quantity = model.Quantity;
            product.Price = model.Price;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            await RegisterAuditAsync(
                "API_PRODUCT_EDIT",
                $"Se editó por API el producto {product.Name} con código {product.Code}");

            return Ok(new { message = "Producto actualizado correctamente." });
        }

        // Eliminar producto
        [HttpDelete("{id}")]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "SuperAdmin,Registrador")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound(new { message = "Producto no encontrado." });
            }

            var productName = product.Name;
            var productCode = product.Code;

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            await RegisterAuditAsync(
                "API_PRODUCT_DELETE",
                $"Se eliminó por API el producto {productName} con código {productCode}");

            return Ok(new { message = "Producto eliminado correctamente." });
        }

        // Registrar auditoría de eventos API de productos
        private async Task RegisterAuditAsync(string eventType, string description)
        {
            var userId = User.Claims.FirstOrDefault(c => c.Type.Contains("nameidentifier"))?.Value;
            var userName = User.Identity?.Name ?? "Usuario desconocido";
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "IP no disponible";
            var route = HttpContext.Request.Path.ToString();

            await _auditService.LogAsync(
                eventType,
                description,
                userId,
                userName,
                ipAddress,
                route);
        }
    }
}