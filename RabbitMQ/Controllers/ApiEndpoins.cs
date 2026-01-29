using System;
using Relatorios.RabbitMQ;

namespace RabbitMQ.Controllers
{
	public static class ApiEndpoins
	{
		public static void AddApiEndpoint(this WebApplication app)
		{
			app.MapPost("solicitar-relatorio/{name}", (string name) =>
			{
				var solicitacao = new SolicitacaoRelatorio
				{
					Nome = name,
				};

				Lista.Relatorios.Add(solicitacao);
				return Results.Ok(solicitacao);
			});


			app.MapGet("relatorios", () => Lista.Relatorios);
		}
	}
}

