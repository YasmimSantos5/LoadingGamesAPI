using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoadingGames.API.Models
{
	[Table("compra")]
	public class Compra
	{
		[Key]
		[Column("id_compra")]
		public int IdCompra { get; set; }

		[Column("id_usuario")]
		public int IdUsuario { get; set; }

		[Column("data_compra")]
		public DateTime DataCompra { get; set; }

		[Column("valor_total")]
		public decimal ValorTotal { get; set; }

		[Column("forma_pagamento")]
		public string FormaPagamento { get; set; } = string.Empty;

		[Column("status_compra")]
		public string StatusCompra { get; set; } = string.Empty;
	}
}