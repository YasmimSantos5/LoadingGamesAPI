using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoadingGames.API.Models
{
	[Table("usuario")]
	public class Usuario
	{
		[Key]
		[Column("id_usuario")]
		public int IdUsuario { get; set; }

		[Column("nome")]
		public string Nome { get; set; } = string.Empty;

		[Column("nome_usuario")]
		public string NomeUsuario { get; set; } = string.Empty;

		[Column("email")]
		public string Email { get; set; } = string.Empty;

		[Column("senha_hash")]
		public string SenhaHash { get; set; } = string.Empty;

		[Column("foto_perfil_url")]
		public string? FotoPerfilUrl { get; set; }

		[Column("status_conta")]
		public string StatusConta { get; set; } = "Ativo";

		[Column("tipo_usuario")]
		public string TipoUsuario { get; set; } = "Comum";

		[Column("data_cadastro")]
		public DateTime DataCadastro { get; set; }
	}
}