namespace Relatorios.RabbitMQ
{
	public static class Lista
	{
		public static List<SolicitacaoRelatorio> Relatorios = new();
	}

	public class SolicitacaoRelatorio
	{
		public Guid Id { get; private set; } = Guid.NewGuid();
		public string Nome { get; set; }
		public string Status { get; set; } = "Pendente";
		public DateTime? ProcessedTime { get; set; } = null;
	}
}

