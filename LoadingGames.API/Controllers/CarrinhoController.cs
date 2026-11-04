using LoadingGames.API.Data;
using LoadingGames.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoadingGames.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CarrinhoController : ControllerBase
	{
		private readonly LoadingGamesDbContext _context;

		public CarrinhoController(
			LoadingGamesDbContext context
		)
		{
			_context = context;
		}


		// =====================================================
		// ADICIONAR JOGO AO CARRINHO
		// POST: api/carrinho/adicionar
		// =====================================================

		[HttpPost("adicionar")]
		public async Task<IActionResult> AdicionarAoCarrinho(
			[FromBody] AdicionarCarrinhoRequest request
		)
		{
			// Verifica se o usuário existe
			var usuarioExiste =
				await _context.Usuarios.AnyAsync(
					u => u.IdUsuario == request.IdUsuario
				);

			if (!usuarioExiste)
			{
				return NotFound(new
				{
					mensagem = "Usuário não encontrado."
				});
			}


			// Verifica se o jogo existe e está ativo
			var jogo =
				await _context.Jogos.FirstOrDefaultAsync(
					j =>
						j.IdJogo == request.IdJogo &&
						j.Ativo
				);

			if (jogo == null)
			{
				return NotFound(new
				{
					mensagem = "Jogo não encontrado."
				});
			}


			// Procura o carrinho do usuário
			var carrinho =
				await _context.Carrinhos.FirstOrDefaultAsync(
					c => c.IdUsuario == request.IdUsuario
				);


			// Se o usuário ainda não tiver carrinho,
			// cria um novo
			if (carrinho == null)
			{
				carrinho = new Carrinho
				{
					IdUsuario = request.IdUsuario,
					DataCriacao = DateTime.Now
				};

				_context.Carrinhos.Add(carrinho);

				await _context.SaveChangesAsync();
			}


			// Verifica se o jogo já está no carrinho
			var jogoJaEstaNoCarrinho =
				await _context.ItensCarrinho.AnyAsync(
					item =>
						item.IdCarrinho == carrinho.IdCarrinho &&
						item.IdJogo == request.IdJogo
				);

			if (jogoJaEstaNoCarrinho)
			{
				return Conflict(new
				{
					mensagem =
						"Este jogo já está no carrinho."
				});
			}


			// Cria o item do carrinho
			var itemCarrinho =
				new ItemCarrinho
				{
					IdCarrinho =
						carrinho.IdCarrinho,

					IdJogo =
						request.IdJogo,

					DataAdicao =
						DateTime.Now
				};


			// Adiciona no banco
			_context.ItensCarrinho.Add(
				itemCarrinho
			);

			await _context.SaveChangesAsync();


			return Ok(new
			{
				mensagem =
					"Jogo adicionado ao carrinho!",

				idCarrinho =
					carrinho.IdCarrinho,

				idJogo =
					jogo.IdJogo,

				nomeJogo =
					jogo.Nome
			});
		}


		// =====================================================
		// BUSCAR CARRINHO DO USUÁRIO
		// GET: api/carrinho/1
		// =====================================================

		[HttpGet("{idUsuario}")]
		public async Task<IActionResult> BuscarCarrinho(
			int idUsuario
		)
		{
			// Verifica se o usuário existe
			var usuarioExiste =
				await _context.Usuarios.AnyAsync(
					u => u.IdUsuario == idUsuario
				);

			if (!usuarioExiste)
			{
				return NotFound(new
				{
					mensagem =
						"Usuário não encontrado."
				});
			}


			// Procura o carrinho do usuário
			var carrinho =
				await _context.Carrinhos
					.AsNoTracking()
					.FirstOrDefaultAsync(
						c => c.IdUsuario == idUsuario
					);


			// Usuário ainda não possui carrinho
			if (carrinho == null)
			{
				return Ok(new
				{
					idCarrinho = 0,
					itens = Array.Empty<object>(),
					total = 0m
				});
			}


			// Busca os jogos que estão no carrinho
			var itens =
				await (
					from item in _context.ItensCarrinho

					join jogo in _context.Jogos
						on item.IdJogo equals jogo.IdJogo

					where
						item.IdCarrinho ==
							carrinho.IdCarrinho
						&&
						jogo.Ativo

					orderby
						item.DataAdicao descending

					select new
					{
						idItemCarrinho =
							item.IdItemCarrinho,

						idJogo =
							jogo.IdJogo,

						nome =
							jogo.Nome,

						preco =
							jogo.Preco,

						imagemPrincipalUrl =
							jogo.ImagemPrincipalUrl,

						classificacaoIndicativa =
							jogo.ClassificacaoIndicativa,

						dataAdicao =
							item.DataAdicao
					}
				)
				.ToListAsync();


			// Soma o preço de todos os jogos
			var total =
				itens.Sum(
					item => item.preco
				);


			// Retorna o carrinho
			return Ok(new
			{
				idCarrinho =
					carrinho.IdCarrinho,

				itens =
					itens,

				total =
					total
			});
		}


		// =====================================================
		// REMOVER JOGO DO CARRINHO
		// DELETE: api/carrinho/item/1?idUsuario=1
		// =====================================================

		[HttpDelete("item/{idItemCarrinho}")]
		public async Task<IActionResult> RemoverItemCarrinho(
			int idItemCarrinho,
			[FromQuery] int idUsuario
		)
		{
			// Verifica se o usuário existe
			var usuarioExiste =
				await _context.Usuarios.AnyAsync(
					u => u.IdUsuario == idUsuario
				);

			if (!usuarioExiste)
			{
				return NotFound(new
				{
					mensagem =
						"Usuário não encontrado."
				});
			}


			// Procura o carrinho pertencente ao usuário
			var carrinho =
				await _context.Carrinhos
					.FirstOrDefaultAsync(
						c => c.IdUsuario == idUsuario
					);

			if (carrinho == null)
			{
				return NotFound(new
				{
					mensagem =
						"Carrinho não encontrado."
				});
			}


			// Procura o item SOMENTE dentro
			// do carrinho desse usuário
			var itemCarrinho =
				await _context.ItensCarrinho
					.FirstOrDefaultAsync(
						item =>
							item.IdItemCarrinho ==
								idItemCarrinho
							&&
							item.IdCarrinho ==
								carrinho.IdCarrinho
					);


			if (itemCarrinho == null)
			{
				return NotFound(new
				{
					mensagem =
						"Item não encontrado no carrinho."
				});
			}


			// Remove o item
			_context.ItensCarrinho.Remove(
				itemCarrinho
			);

			await _context.SaveChangesAsync();


			return Ok(new
			{
				mensagem =
					"Jogo removido do carrinho!",

				idItemCarrinho =
					idItemCarrinho
			});
		}
	}


	// =========================================================
	// REQUEST PARA ADICIONAR JOGO
	// =========================================================

	public class AdicionarCarrinhoRequest
	{
		public int IdUsuario { get; set; }

		public int IdJogo { get; set; }
	}
}