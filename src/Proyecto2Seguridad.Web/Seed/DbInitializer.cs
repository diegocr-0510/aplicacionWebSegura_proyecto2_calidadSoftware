using Microsoft.AspNetCore.Identity;
using Proyecto2Seguridad.Web.Models;

namespace Proyecto2Seguridad.Web.Seed
{
    /// <summary>
    /// Clase encargada de sembrar roles y un usuario administrador inicial.
    /// </summary>
    public static class DbInitializer
    {
        /// <summary>
        /// Crea los roles base del sistema y un usuario administrador inicial si no existen.
        /// </summary>
        /// <param name="userManager">Administrador de usuarios.</param>
        /// <param name="roleManager">Administrador de roles.</param>
        public static async Task SeedRolesAndAdminAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            // Lista de roles mínimos exigidos por el proyecto.
            string[] roles = { "SuperAdmin", "Auditor", "Registrador" };

            // Crear cada rol si no existe.
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new ApplicationRole { Name = role });
                }
            }

            // Datos del usuario administrador inicial.
            var adminEmail = "admin@utn.ac.cr";
            var adminUserName = "admin";

            // Verificar si el usuario ya existe.
            var existingUser = await userManager.FindByEmailAsync(adminEmail);

            if (existingUser == null)
            {
                var adminUser = new ApplicationUser
                {
                    UserName = adminUserName,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                // Crear el usuario con una contraseña fuerte.
                var result = await userManager.CreateAsync(adminUser, "Admin123*");

                // Si se creó correctamente, asignar rol SuperAdmin.
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "SuperAdmin");
                }
            }
        }
    }
}