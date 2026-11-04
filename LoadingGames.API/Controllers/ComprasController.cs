using System;
using System.Linq;
using System.Threading.Tasks;
using LoadingGames.API.Data;
using LoadingGames.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoadingGames.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class ComprasController : ControllerBase
	{
		private readonly LoadingGamesDbContext _context;

		public ComprasController(LoadingGamesDbContext context)
		{
			_context = context;
		}

		[HttpPost("finalizar")]
		public async Task<IActionResult> FinalizarCompra(
			[FromBody] FinalizarCompraRequest request)
		{
			// ==========================================
			// 1. VALIDAR DADOS RECEBIDOS
			// ==========================================

			if (request == null)
			{
				return BadRequest(new
				{
					mensagem = "Dados da compra não informados."
				});
			}

			if (request.IdUsuario <= 0)
			{
				return BadRequest(new
				{
					mensagem = "Usuário inválido."
				});
			}

			var formasPermitidas = new[]
			{
				"Cartao",
				"Pix",
				"Boleto"
			};

			if (string.IsNullOrWhiteSpace(request.FormaPagamento))
			{
				return BadRequest(new
				{
					mensagem = "Informe a forma de pagamento."
				});
			}

			var formaPagamentoValida =
				formasPermitidas.Any(
					forma =>
						forma.Equals(
							request.FormaPagamento,
							StringComparison.OrdinalIgnoreCase
						)
				);

			if (!formaPagamentoValida)
			{
				return BadRequest(new
				{
					mensagem = "Forma de pagamento inválida."
				});
			}

			// ==========================================
			// 2. VERIFICAR USUÁRIO
			// ==========================================

			var usuarioExiste =
				await _context.Usuarios.AnyAsync(
					usuario =>
						usuario.IdUsuario == request.IdUsuario
				);

			if (!usuarioExiste)
			{
				return NotFound(new
				{
					mensagem = "Usuário não encontrado."
				});
			}

			// ==========================================
			// 3. BUSCAR CARRINHO
			// ==========================================

			var carrinho =
				await _context.Carrinhos
					.FirstOrDefaultAsync(
						carrinho =>
							carrinho.IdUsuario == request.IdUsuario
					);

			if (carrinho == null)
			{
				return BadRequest(new
				{
					mensagem = "Carrinho não encontrado."
				});
			}

			// ==========================================
			// 4. BUSCAR ITENS DO CARRINHO
			// ==========================================

			var itensCarrinho =
				await _context.ItensCarrinho
					.Where(
						item =>
							item.IdCarrinho == carrinho.IdCarrinho
					)
					.ToListAsync();

			if (itensCarrinho.Count == 0)
			{
				return BadRequest(new
				{
					mensagem = "O carrinho está vazio."
				});
			}

			// ==========================================
			// 5. BUSCAR JOGOS
			// ==========================================

			var idsJogos =
				itensCarrinho
					.Select(item => item.IdJogo)
					.Distinct()
					.ToList();

			var jogos =
				await _context.Jogos
					.Where(
						jogo =>
							idsJogos.Contains(jogo.IdJogo)
					)
					.ToListAsync();

			if (jogos.Count != idsJogos.Count)
			{
				return BadRequest(new
				{
					mensagem =
						"Um ou mais jogos do carrinho não foram encontrados."
				});
			}

			// ==========================================
			// 6. VERIFICAR JOGOS JÁ COMPRADOS
			// ==========================================

			var jogosJaPossuidos =
				await _context.PosseJogos
					.Where(
						posse =>
							posse.IdUsuario == request.IdUsuario &&
							idsJogos.Contains(posse.IdJogo)
					)
					.Select(posse => posse.IdJogo)
					.ToListAsync();

			if (jogosJaPossuidos.Count > 0)
			{
				return BadRequest(new
				{
					mensagem =
						"Você já possui um ou mais jogos deste carrinho."
				});
			}

			// ==========================================
			// 7. CALCULAR TOTAL
			// ==========================================

			var total =
				jogos.Sum(jogo => jogo.Preco);

			// ==========================================
			// 8. PADRONIZAR PAGAMENTO
			// ==========================================

			string formaPagamento;

			if (request.FormaPagamento.Equals(
					"Pix",
					StringComparison.OrdinalIgnoreCase))
			{
				formaPagamento = "Pix";
			}
			else if (request.FormaPagamento.Equals(
						 "Boleto",
						 StringComparison.OrdinalIgnoreCase))
			{
				formaPagamento = "Boleto";
			}
			else
			{
				formaPagamento = "Cartao";
			}

			// ==========================================
			// 9. INICIAR TRANSAÇÃO
			// ==========================================

			await using var transaction =
				await _context.Database.BeginTransactionAsync();

			try
			{
				// ======================================
				// 10. CRIAR COMPRA
				// ======================================

				var compra = new Compra
				{
					IdUsuario = request.IdUsuario,
					DataCompra = DateTime.Now,
					ValorTotal = total,
					FormaPagamento = formaPagamento,
					StatusCompra = "Concluida"
				};

				_context.Compras.Add(compra);

				// Salva para o MySQL gerar id_compra
				await _context.SaveChangesAsync();

				// ======================================
				// 11. CRIAR ITENS E POSSES
				// ======================================

				foreach (var itemCarrinho in itensCarrinho)
				{
					var jogo =
						jogos.First(
							jogo =>
								jogo.IdJogo == itemCarrinho.IdJogo
						);

					// ITEM DA COMPRA
					var itemCompra = new ItemCompra
					{
						IdCompra = compra.IdCompra,
						IdJogo = jogo.IdJogo,
						PrecoUnitario = jogo.Preco
					};

					_context.ItensCompra.Add(itemCompra);

					// POSSE DO JOGO
					var posseJogo = new PosseJogo
					{
						IdUsuario = request.IdUsuario,
						IdJogo = jogo.IdJogo,
						IdCompra = compra.IdCompra,
						DataAquisicao = DateTime.Now
					};

					_context.PosseJogos.Add(posseJogo);
				}

				// ======================================
				// 12. LIMPAR CARRINHO
				// ======================================

				_context.ItensCarrinho.RemoveRange(
					itensCarrinho
				);

				// ======================================
				// 13. SALVAR ALTERAÇÕES
				// ======================================

				await _context.SaveChangesAsync();

				// ======================================
				// 14. CONFIRMAR TRANSAÇÃO
				// ======================================

				await transaction.CommitAsync();

				// ======================================
				// 15. RETORNAR SUCESSO
				// ======================================

				return Ok(new
				{
					mensagem =
						"Compra realizada com sucesso!",

					idCompra =
						compra.IdCompra,

					valorTotal =
						compra.ValorTotal,

					formaPagamento =
						compra.FormaPagamento,

					statusCompra =
						compra.StatusCompra
				});
			}
			catch (Exception ex)
			{
				await transaction.RollbackAsync();

				return StatusCode(
					500,
					new
					{
						mensagem =
							"Erro ao finalizar a compra.",

						erro =
							ex.Message
					}
				);
			}
		}
	}
}