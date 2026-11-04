
using LoadingGames.API.Data;
using LoadingGames.API.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

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

		// ==========================================
		// LOGIN
		// ==========================================

		[HttpPost("login")]
		public async Task<IActionResult> Login(
			[FromBody] LoginRequest request)
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

			var hasher = new PasswordHasher<Usuario>();

			var resultado = hasher.VerifyHashedPassword(
				usuario,
				usuario.SenhaHash,
				request.Senha
			);

			if (resultado == PasswordVerificationResult.Failed)
			{
				return Unauthorized(new
				{
					mensagem = "E-mail ou senha inválidos."
				});
			}

			return Ok(new
			{
				mensagem = "Login realizado com sucesso.",
				usuario = DadosUsuario(usuario)
			});
		}

		// ==========================================
		// CADASTRO
		// ==========================================

		[HttpPost("cadastro")]
		public async Task<IActionResult> Cadastro(
			[FromBody] CadastroRequest request)
		{
			if (string.IsNullOrWhiteSpace(request.Nome) ||
				string.IsNullOrWhiteSpace(request.Email) ||
				string.IsNullOrWhiteSpace(request.Senha))
			{
				return BadRequest(new
				{
					mensagem = "Preencha todos os campos."
				});
			}

			var email = request.Email.Trim();

			var emailExiste = await _context.Usuarios
				.AnyAsync(u => u.Email == email);

			if (emailExiste)
			{
				return BadRequest(new
				{
					mensagem = "Este e-mail já está cadastrado."
				});
			}

			var nomeUsuarioBase = email.Split('@')[0];
			var nomeUsuario = nomeUsuarioBase;
			var contador = 1;

			while (await _context.Usuarios
				.AnyAsync(u => u.NomeUsuario == nomeUsuario))
			{
				nomeUsuario = $"{nomeUsuarioBase}{contador}";
				contador++;
			}

			var usuario = new Usuario
			{
				Nome = request.Nome.Trim(),
				NomeUsuario = nomeUsuario,
				Email = email,
				StatusConta = "Ativo",
				TipoUsuario = "Comum",
				DataCadastro = DateTime.Now
			};

			var hasher = new PasswordHasher<Usuario>();

			usuario.SenhaHash = hasher.HashPassword(
				usuario,
				request.Senha
			);

			_context.Usuarios.Add(usuario);
			await _context.SaveChangesAsync();

			return Ok(new
			{
				mensagem = "Conta criada com sucesso."
			});
		}

		// ==========================================
		// ATUALIZAR PERFIL
		// ==========================================

		[HttpPut("perfil")]
		public async Task<IActionResult> AtualizarPerfil(
			[FromBody] AtualizarPerfilRequest request)
		{
			if (request.IdUsuario <= 0 ||
				string.IsNullOrWhiteSpace(request.Nome) ||
				string.IsNullOrWhiteSpace(request.NomeUsuario) ||
				string.IsNullOrWhiteSpace(request.Email) ||
				string.IsNullOrWhiteSpace(request.SenhaAtual))
			{
				return BadRequest(new
				{
					mensagem = "Preencha todos os campos."
				});
			}

			if (!new EmailAddressAttribute()
				.IsValid(request.Email.Trim()))
			{
				return BadRequest(new
				{
					mensagem = "Digite um e-mail válido."
				});
			}

			var usuario = await _context.Usuarios
				.FirstOrDefaultAsync(
					u => u.IdUsuario == request.IdUsuario
				);

			if (usuario == null ||
				usuario.StatusConta != "Ativo")
			{
				return Unauthorized(new
				{
					mensagem = "Conta não encontrada ou inativa."
				});
			}

			var hasher = new PasswordHasher<Usuario>();

			var resultado = hasher.VerifyHashedPassword(
				usuario,
				usuario.SenhaHash,
				request.SenhaAtual
			);

			if (resultado == PasswordVerificationResult.Failed)
			{
				return Unauthorized(new
				{
					mensagem = "Senha atual incorreta."
				});
			}

			var email = request.Email.Trim();
			var nomeUsuario = request.NomeUsuario.Trim();

			var emailExiste = await _context.Usuarios
				.AnyAsync(u =>
					u.Email == email &&
					u.IdUsuario != usuario.IdUsuario);

			if (emailExiste)
			{
				return Conflict(new
				{
					mensagem = "Este e-mail já está em uso."
				});
			}

			var nomeExiste = await _context.Usuarios
				.AnyAsync(u =>
					u.NomeUsuario == nomeUsuario &&
					u.IdUsuario != usuario.IdUsuario);

			if (nomeExiste)
			{
				return Conflict(new
				{
					mensagem = "Este nome de usuário já está em uso."
				});
			}

			usuario.Nome = request.Nome.Trim();
			usuario.NomeUsuario = nomeUsuario;
			usuario.Email = email;

			await _context.SaveChangesAsync();

			return Ok(new
			{
				mensagem = "Perfil atualizado com sucesso.",
				usuario = DadosUsuario(usuario)
			});
		}

		// ==========================================
		// ALTERAR SENHA
		// ==========================================

		[HttpPut("alterar-senha")]
		public async Task<IActionResult> AlterarSenha(
			[FromBody] AlterarSenhaRequest request)
		{
			if (request.IdUsuario <= 0 ||
				string.IsNullOrWhiteSpace(request.SenhaAtual) ||
				string.IsNullOrWhiteSpace(request.NovaSenha))
			{
				return BadRequest(new
				{
					mensagem = "Preencha todos os campos."
				});
			}

			if (request.NovaSenha.Length < 8)
			{
				return BadRequest(new
				{
					mensagem = "A nova senha deve ter pelo menos 8 caracteres."
				});
			}

			var usuario = await _context.Usuarios
				.FirstOrDefaultAsync(
					u => u.IdUsuario == request.IdUsuario
				);

			if (usuario == null ||
				usuario.StatusConta != "Ativo")
			{
				return Unauthorized(new
				{
					mensagem = "Conta não encontrada ou inativa."
				});
			}

			var hasher = new PasswordHasher<Usuario>();

			var resultado = hasher.VerifyHashedPassword(
				usuario,
				usuario.SenhaHash,
				request.SenhaAtual
			);

			if (resultado == PasswordVerificationResult.Failed)
			{
				return Unauthorized(new
				{
					mensagem = "Senha atual incorreta."
				});
			}

			if (request.NovaSenha == request.SenhaAtual)
			{
				return BadRequest(new
				{
					mensagem = "A nova senha deve ser diferente da atual."
				});
			}

			usuario.SenhaHash = hasher.HashPassword(
				usuario,
				request.NovaSenha
			);

			await _context.SaveChangesAsync();

			return Ok(new
			{
				mensagem = "Senha alterada com sucesso."
			});
		}

		// ==========================================
		// DADOS DO USUÁRIO
		// ==========================================

		private static object DadosUsuario(Usuario usuario)
		{
			return new
			{
				id = usuario.IdUsuario,
				nome = usuario.Nome,
				nomeUsuario = usuario.NomeUsuario,
				email = usuario.Email,
				fotoPerfilUrl = usuario.FotoPerfilUrl,
				tipoUsuario = usuario.TipoUsuario
			};
		}
	}

	// ==========================================
	// REQUESTS
	// ==========================================

	public class LoginRequest
	{
		public string Email { get; set; } = string.Empty;
		public string Senha { get; set; } = string.Empty;
	}

	public class CadastroRequest
	{
		public string Nome { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Senha { get; set; } = string.Empty;
	}

	public class AtualizarPerfilRequest
	{
		public int IdUsuario { get; set; }
		public string Nome { get; set; } = string.Empty;
		public string NomeUsuario { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string SenhaAtual { get; set; } = string.Empty;
	}

	public class AlterarSenhaRequest
	{
		public int IdUsuario { get; set; }
		public string SenhaAtual { get; set; } = string.Empty;
		public string NovaSenha { get; set; } = string.Empty;
	}
}
