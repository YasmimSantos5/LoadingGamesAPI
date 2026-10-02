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
	}
}