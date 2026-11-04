namespace LoadingGames.API.Models
{
	public class FinalizarCompraRequest
	{
		public int IdUsuario { get; set; }

		public string FormaPagamento { get; set; } = string.Empty;
	}
}