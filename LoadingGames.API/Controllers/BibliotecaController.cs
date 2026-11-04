
using LoadingGames.API.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoadingGames.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class BibliotecaController : ControllerBase
	{
		private readonly LoadingGamesDbContext _context;

		public BibliotecaController(LoadingGamesDbContext context)
		{
			_context = context;
		}

		// BUSCAR JOGOS DA BIBLIOTECA
		[HttpGet("{idUsuario}")]
		public async Task<IActionResult> BuscarBiblioteca(
			int idUsuario)
		{
			if (idUsuario <= 0)
			{
				return BadRequest(new
				{
					mensagem = "Usuário inválido."
				});
			}

			var usuarioExiste = await _context.Usuarios
				.AnyAsync(u => u.IdUsuario == idUsuario);

			if (!usuarioExiste)
			{
				return NotFound(new
				{
					mensagem = "Usuário não encontrado."
				});
			}

			var biblioteca = await (
				from posse in _context.PosseJogos
				join jogo in _context.Jogos
					on posse.IdJogo equals jogo.IdJogo
				where posse.IdUsuario == idUsuario
				orderby posse.DataAquisicao descending
				select new
				{
					idJogo = jogo.IdJogo,
					nome = jogo.Nome,
					descricao = jogo.Descricao,
					imagemPrincipalUrl = jogo.ImagemPrincipalUrl,
					dataAquisicao = posse.DataAquisicao,
					idCompra = posse.IdCompra
				}
			).ToListAsync();

			return Ok(new
			{
				mensagem = "Biblioteca carregada com sucesso.",
				totalJogos = biblioteca.Count,
				jogos = biblioteca
			});
		}
	}
}
	