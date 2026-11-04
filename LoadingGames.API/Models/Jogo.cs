using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LoadingGames.API.Models
{
	[Table("jogo")]
	public class Jogo
	{
		[Key]
		[Column("id_jogo")]
		public int IdJogo { get; set; }

		[Column("id_desenvolvedor")]
		public int IdDesenvolvedor { get; set; }

		[Column("id_publicadora")]
		public int IdPublicadora { get; set; }

		[Column("nome")]
		public string Nome { get; set; } = string.Empty;

		[Column("descricao")]
		public string? Descricao { get; set; }

		[Column("preco")]
		public decimal Preco { get; set; }

		[Column("imagem_principal_url")]
		public string? ImagemPrincipalUrl { get; set; }

		[Column("data_lancamento")]
		public DateTime? DataLancamento { get; set; }

		[Column("classificacao_indicativa")]
		public byte ClassificacaoIndicativa { get; set; }

		[Column("ativo")]
		public bool Ativo { get; set; }
	}
}