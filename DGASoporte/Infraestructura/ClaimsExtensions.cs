using System.Security.Claims;

namespace DGASoporte.Infraestructura
{
    public static class ClaimsExtensions
    {
        //Autenticación
        public static bool IsAuth(this ClaimsPrincipal user) =>
            user?.Identity?.IsAuthenticated == true;

        //Devuelve el claim con el tipo indicado o null si no existe.
        public static Claim? GetClaim(this ClaimsPrincipal user, string claimType) =>
            user?.Claims?.FirstOrDefault(c => c.Type == claimType);

        //Devuelve el valor del claim o null si no existe.
        public static string? GetClaimValue(this ClaimsPrincipal user, string claimType) =>
            user.GetClaim(claimType)?.Value;

        //Intenta leer el valor del claim. Retorna true/false según éxito.
        public static bool TryGetClaimValue(this ClaimsPrincipal user, string claimType, out string? value)
        {
            value = user.GetClaimValue(claimType);
            return value is not null;
        }

        //Identidad principal

        //Id de usuario como int?
        // Usar ClaimTypes.NameIdentifier.
        public static int? GetUserId(this ClaimsPrincipal user)
        {
            var raw = user.GetClaimValue(ClaimTypes.NameIdentifier);
            return int.TryParse(raw, out var id) ? id : null;
        }

        //Id de usuario obligatorio como int.
        //Lanza InvalidOperationException si no está autenticado o si el claim es inválido.
        public static int GetRequiredUserId(this ClaimsPrincipal user)
        {
            if (user?.Identity?.IsAuthenticated != true)
                throw new InvalidOperationException("Usuario no autenticado.");

            var raw = user.GetClaimValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(raw) || !int.TryParse(raw, out var id))
                throw new InvalidOperationException("Claim NameIdentifier ausente o inválido.");

            return id;
        }

        //Nombre de usuario
        public static string? GetUserName(this ClaimsPrincipal user) =>
            user.GetClaimValue(ClaimTypes.Name) ?? user?.Identity?.Name;

        //Email del usuario
        public static string? GetEmail(this ClaimsPrincipal user) =>
            user.GetClaimValue(ClaimTypes.Email);

        //Roles

        //Primer rol
        public static string? GetRole(this ClaimsPrincipal user) =>
            user.GetClaimValue(ClaimTypes.Role);

        /// <summary>
        /// Todos los roles (varios ClaimTypes.Role) como conjunto único.
        /// </summary>
        public static ISet<string> GetRoles(this ClaimsPrincipal user) =>
            user?.Claims?
                .Where(c => c.Type == ClaimTypes.Role && !string.IsNullOrWhiteSpace(c.Value))
                .Select(c => c.Value.Trim())
                .ToHashSet(StringComparer.OrdinalIgnoreCase)
            ?? new HashSet<string>(StringComparer.OrdinalIgnoreCase);
       
         //True si el usuario pertenece a alguno de los roles especificados.
        public static bool IsInAnyRole(this ClaimsPrincipal user, params string[] roles)
        {
            if (roles is null || roles.Length == 0) return false;
            var mine = user.GetRoles();
            return roles.Any(r => mine.Contains(r));
        }
    }
}
