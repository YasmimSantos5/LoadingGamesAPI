using LoadingGames.API.Data;
using LoadingGames.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoadingGames.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly LoadingGamesDbContext _context;

		public AuthController(LoadingGamesDbContext context)
		{
			_context = context;
		}

		// =========================
		// LOGIN
		// =========================

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequest request)
		{
			var usuario = await _context.Usuarios
				.FirstOrDefaultAsync(u => u.Email == request.Email);

			if (usuario == null)
			{
				return Unauthorized(new
				{
					mensagem = "E-mail ou senha inválidos."
				});
			}

			if (usuario.StatusConta != "Ativo")
			{
				return Unauthorized(new
				{
					mensagem = "Esta conta está inativa."
				});
			}

			var passwordHasher = new PasswordHasher<Usuario>();

			var resultado = passwordHasher.VerifyHashedPassword(
				usuario,
				usuario.SenhaHash,
				request.Senha
			);

			if (resultado != PasswordVerificationResult.Success &&
				resultado != PasswordVerificationResult.SuccessRehashNeeded)
			{
				return Unauthorized(new
				{
					mensagem = "E-mail ou senha inválidos."
				});
			}

			return Ok(new
			{
				mensagem = "Login realizado com sucesso.",

				usuario = new
				{
					id = usuario.IdUsuario,
					nome = usuario.Nome,
					nomeUsuario = usuario.NomeUsuario,
					email = usuario.Email,
					fotoPerfilUrl = usuario.FotoPerfilUrl,
					tipoUsuario = usuario.TipoUsuario
				}
			});
		}


		// =========================
		// CADASTRO
		// =========================

		[HttpPost("cadastro")]
		public async Task<IActionResult> Cadastro(
			[FromBody] CadastroRequest request)
		{
			// Verifica se o e-mail já existe
			var emailExiste = await _context.Usuarios
				.AnyAsync(u => u.Email == request.Email);

			if (emailExiste)
			{
				return BadRequest(new
				{
					mensagem = "Este e-mail já está cadastrado."
				});
			}

			// Gera nome de usuário usando o e-mail
			var nomeUsuarioBase = request.Email.Split('@')[0];

			var nomeUsuario = nomeUsuarioBase;

			var contador = 1;

			// Garante que o nome de usuário seja único
			while (await _context.Usuarios
				.AnyAsync(u => u.NomeUsuario == nomeUsuario))
			{
				nomeUsuario = $"{nomeUsuarioBase}{contador}";
				contador++;
			}

			// Cria o usuário
			var usuario = new Usuario
			{
				Nome = request.Nome,
				NomeUsuario = nomeUsuario,
				Email = request.Email,
				StatusConta = "Ativo",
				TipoUsuario = "Comum",
				DataCadastro = DateTime.Now
			};

			// Gera o hash da senha
			var passwordHasher = new PasswordHasher<Usuario>();

			usuario.SenhaHash = passwordHasher.HashPassword(
				usuario,
				request.Senha
			);

			// Salva no banco
			_context.Usuarios.Add(usuario);

			await _context.SaveChangesAsync();

			return Ok(new
			{
				mensagem = "Conta criada com sucesso."
			});
		}
	}


	// =========================
	// DADOS DO LOGIN
	// =========================

	public class LoginRequest
	{
		public string Email { get; set; } = string.Empty;

		public string Senha { get; set; } = string.Empty;
	}


	// =========================
	// DADOS DO CADASTRO
	// =========================

	public class CadastroRequest
	{
		public string Nome { get; set; } = string.Empty;

		public string Email { get; set; } = string.Empty;

		public string Senha { get; set; } = string.Empty;
	}
}