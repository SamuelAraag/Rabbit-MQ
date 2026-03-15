namespace RabbitMQ.Test.Integration
{
	public class EstudoDotNet
	{
		public EstudoDotNet()
		{
		}

		[Fact]
		public void DominioAnemico_SomentePropriedades()
		{
			//uma classe sem regras de negócio, somente estrutuda de dados
			var usuario = new Usuario()
			{
				Nome = "Samuel",
				Sobrenome = "Santos",
				DataNascimento = new DateTime(1995, 12, 11),
				Idade = 30,
			};

			Assert.Equal("Samuel", usuario.Nome);
			Assert.Equal("Santos", usuario.Sobrenome);
			Assert.Equal(new DateTime(1995, 12, 11), usuario.DataNascimento);
			Assert.Equal((uint)30, usuario.Idade);
        }

		[Theory]
		[InlineData(18)]
		[InlineData(19)]
		[InlineData(20)]
		[InlineData(30)]
		[InlineData(100)]
		public void DominioRico_ClasseComRegrasDeNegocioValida(uint idade) //valido
		{
			var usuario = new UsuarioRico
			{
				Idade = idade,
			};

			var retornoValidacao = usuario.ValidarIdade();

			Assert.Equal(idade, usuario.Idade);
			Assert.True(retornoValidacao);
			Assert.True(usuario.EhMaiorDeIdade);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(17)]
        [InlineData(15)]
        [InlineData(5)]
        [InlineData(10)]
        public void DominioRico_ClasseComRegrasDeNegocioInvalida(uint idadeUsuario) //com erro
        {
			var usuario = new UsuarioRico
			{
				Idade = idadeUsuario
			};

            var retornoCriacaoUsuario = Assert.Throws<Exception>(() => usuario.ValidarIdade());

			Assert.Equal($"Usuário deve ter pelo menos 18 anos. Esté usuario tem: [{idadeUsuario}]", retornoCriacaoUsuario.Message);
        }

        #region
        public class Usuario //classe anemica
        {
			public string Nome { get; set; }
			public string Sobrenome { get; set; }
			public DateTime DataNascimento { get; set; }
			public uint Idade { get; set; }
			public bool EhMaiorDeIdade { get; set; }
		}

		public class UsuarioRico : Usuario
		{
			public bool ValidarIdade()
			{
				if (Idade < 18)
					throw new Exception($"Usuário deve ter pelo menos 18 anos. Esté usuario tem: [{Idade}]");

				EhMaiorDeIdade = true;
				return EhMaiorDeIdade;
			}
		}
        #endregion
    }
}

