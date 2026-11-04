using LoadingGames.API.Models;
using Microsoft.EntityFrameworkCore;

namespace LoadingGames.API.Data
{
	public class LoadingGamesDbContext : DbContext
	{
		public LoadingGamesDbContext(
			DbContextOptions<LoadingGamesDbContext> options)
			: base(options)
		{
		}

		public DbSet<Usuario> Usuarios { get; set; }
		public DbSet<Jogo> Jogos { get; set; }

		public DbSet<Carrinho> Carrinhos { get; set; }
		public DbSet<ItemCarrinho> ItensCarrinho { get; set; }

		public DbSet<Compra> Compras { get; set; }
		public DbSet<ItemCompra> ItensCompra { get; set; }
		public DbSet<PosseJogo> PosseJogos { get; set; }
	}
}