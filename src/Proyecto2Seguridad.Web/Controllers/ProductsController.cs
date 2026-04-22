using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Proyecto2Seguridad.Web.Data;
using Proyecto2Seguridad.Web.Models;

namespace Proyecto2Seguridad.Web.Controllers
{
    // Este controlador permite gestionar productos.
    // Auditor puede ver, pero solo SuperAdmin y Registrador pueden escribir.
    [Authorize(Roles = "SuperAdmin,Registrador,Auditor")]
    public class ProductsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ProductsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Lista de productos
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .OrderBy(p => p.Name)
                .ToListAsync();

            return View(products);
        }

        // Detalle de producto
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

        // Formulario de creación
        [Authorize(Roles = "SuperAdmin,Registrador")]
        public IActionResult Create()
        {
            return View();
        }

        // Guardar nuevo producto
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Registrador")]
        public async Task<IActionResult> Create(Product product)
        {
            // Validación extra: código único
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

            return RedirectToAction(nameof(Index));
        }

        // Formulario de edición
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

        // Guardar edición
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Registrador")]
        public async Task<IActionResult> Edit(int id, Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            // Validación extra: código único excluyendo el mismo registro
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

            return RedirectToAction(nameof(Index));
        }

        // Confirmación de borrado
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

        // Borrado real
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "SuperAdmin,Registrador")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Products.FindAsync(id);

            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}