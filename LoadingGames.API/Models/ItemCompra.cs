using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoadingGames.API.Models
{
	[Table("item_compra")]
	public class ItemCompra
	{
		[Key]
		[Column("id_item_compra")]
		public int IdItemCompra { get; set; }

		[Column("id_compra")]
		public int IdCompra { get; set; }

		[Column("id_jogo")]
		public int IdJogo { get; set; }

		[Column("preco_unitario")]
		public decimal PrecoUnitario { get; set; }
	}
}