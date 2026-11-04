using LoadingGames.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoadingGames.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class JogosController : ControllerBase
	{
		private readonly LoadingGamesDbContext _context;

		public JogosController(LoadingGamesDbContext context)
		{
			_context = context;
		}

		// GET: api/jogos
		// GET: api/jogos?pesquisa=God
		[HttpGet]
		public async Task<IActionResult> PesquisarJogos(
			[FromQuery] string? pesquisa)
		{
			var query = _context.Jogos
				.AsNoTracking()
				.Where(j => j.Ativo);

			// Se digitou algo, pesquisa pelo nome
			if (!string.IsNullOrWhiteSpace(pesquisa))
			{
				pesquisa = pesquisa.Trim();

				query = query.Where(j =>
					EF.Functions.Like(
						j.Nome,
						$"%{pesquisa}%"
					)
				);
			}

			var jogos = await query
				.OrderBy(j => j.Nome)
				.Select(j => new
				{
					id = j.IdJogo,
					nome = j.Nome,
					descricao = j.Descricao,
					preco = j.Preco,
					imagemPrincipalUrl = j.ImagemPrincipalUrl,
					dataLancamento = j.DataLancamento,
					classificacaoIndicativa =
						j.ClassificacaoIndicativa
				})
				.ToListAsync();

			return Ok(jogos);
		}
	}
}