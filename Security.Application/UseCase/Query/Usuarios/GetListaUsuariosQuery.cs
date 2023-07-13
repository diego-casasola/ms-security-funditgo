using MediatR;
using Security.Application.Dto.Usuarios;

namespace Security.Application.UseCase.Query.Usuarios
{
    public class GetListaUsuarioQuery : IRequest<IEnumerable<UsuarioDto>>
    {
        public string Rol { get; set; }
    }
}
