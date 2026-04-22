using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto2Seguridad.Web.Data;
using Proyecto2Seguridad.Web.Models;
using Proyecto2Seguridad.Web.Services;

namespace Proyecto2Seguridad.Web.Controllers
{
    [Authorize(Roles = "SuperAdmin,Registrador,Auditor")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly AuditService _auditService;

        public ProductsController(ApplicationDbContext context, AuditService auditService)
        {
            _context = context;
            _auditService = auditService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(products);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [Authorize(Roles = "SuperAdmin,Registrador")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Registrador")]
        public async Task<IActionResult> Create(Product product)
        {
            bool codeExists = await _context.Products.AnyAsync(p => p.Code == product.Code);

            if (codeExists)
            {
                ModelState.AddModelError("Code", "Ya existe un producto con ese código.");
            }

            if (!ModelState.IsValid)
            {
                return View(product);
            }

            product.CreatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            // Registrar auditoría de creación
            await RegisterAuditAsync(
                "PRODUCT_CREATE",
                $"Se creó el producto {product.Name} con código {product.Code}");

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "SuperAdmin,Registrador")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FindAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Registrador")]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            bool codeExists = await _context.Products.AnyAsync(p => p.Code == product.Code && p.Id != product.Id);

            if (codeExists)
            {
                ModelState.AddModelError("Code", "Ya existe otro producto con ese código.");
            }

            if (!ModelState.IsValid)
            {
                return View(product);
            }

            var existingProduct = await _context.Products.FindAsync(id);

            if (existingProduct == null)
            {
                return NotFound();
            }

            existingProduct.Code = product.Code;
            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Quantity = product.Quantity;
            existingProduct.Price = product.Price;
            existingProduct.UpdatedAt = DateTime.UtcNow;

            _context.Update(existingProduct);
            await _context.SaveChangesAsync();

            // Registrar auditoría de edición
            await RegisterAuditAsync(
                "PRODUCT_EDIT",
                $"Se editó el producto {existingProduct.Name} con código {existingProduct.Code}");

            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "SuperAdmin,Registrador")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Registrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                var productName = product.Name;
                var productCode = product.Code;

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();

                // Registrar auditoría de borrado
                await RegisterAuditAsync(
                    "PRODUCT_DELETE",
                    $"Se eliminó el producto {productName} con código {productCode}");
            }

            return RedirectToAction(nameof(Index));
        }

        // Método auxiliar para registrar auditoría usando el usuario autenticado
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