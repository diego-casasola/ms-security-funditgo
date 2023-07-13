using MediatR;
using Microsoft.AspNetCore.Identity;
using Security.Application.Dto.Usuarios;
using Security.Application.UseCase.Query.Usuarios;
using Security.Infrastructure.Security;

namespace Security.Infrastructure.Query.Usuarios
{
    internal class GetListaUsuarioHandler : IRequestHandler<GetListaUsuarioQuery, IEnumerable<UsuarioDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public GetListaUsuarioHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }
        public async Task<IEnumerable<UsuarioDto>> Handle(GetListaUsuarioQuery request, CancellationToken cancellationToken)
        {
            var usersInRole = await _userManager.GetUsersInRoleAsync(request.Rol);

            var lista = usersInRole.Select(usuario => new UsuarioDto
            {
                Id = usuario.Id,
                NombreCompleto = usuario.FullName,
                Email = usuario.Email,
                Active = usuario.Active,
                Staff = usuario.Staff,
            }).ToList();

            return lista;
        }
    }

}
