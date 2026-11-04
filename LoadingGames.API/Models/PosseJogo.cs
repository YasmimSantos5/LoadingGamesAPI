using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoadingGames.API.Models
{
	[Table("posse_jogo")]
	public class PosseJogo
	{
		[Key]
		[Column("id_posse")]
		public int IdPosse { get; set; }

		[Column("id_usuario")]
		public int IdUsuario { get; set; }

		[Column("id_jogo")]
		public int IdJogo { get; set; }

		[Column("id_compra")]
		public int IdCompra { get; set; }

		[Column("data_aquisicao")]
		public DateTime DataAquisicao { get; set; }
	}
}