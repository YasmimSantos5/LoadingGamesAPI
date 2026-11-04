using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoadingGames.API.Models
{
	[Table("carrinho")]
	public class Carrinho
	{
		[Key]
		[Column("id_carrinho")]
		public int IdCarrinho { get; set; }

		[Column("id_usuario")]
		public int IdUsuario { get; set; }

		[Column("data_criacao")]
		public DateTime DataCriacao { get; set; }
	}
}