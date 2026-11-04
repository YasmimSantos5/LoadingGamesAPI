using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoadingGames.API.Models
{
	[Table("item_carrinho")]
	public class ItemCarrinho
	{
		[Key]
		[Column("id_item_carrinho")]
		public int IdItemCarrinho { get; set; }

		[Column("id_carrinho")]
		public int IdCarrinho { get; set; }

		[Column("id_jogo")]
		public int IdJogo { get; set; }

		[Column("data_adicao")]
		public DateTime DataAdicao { get; set; }
	}
}