using System;
using System.IO;
using System.Text.Json;
using System.Text;
using Npgsql;

namespace GerenciadorEscritorio.BancoDeDados
{
    // Molde para guardar as configurações
    public class ConfigDados
    {
        public string Servidor { get; set; } = "localhost";
        public string Porta { get; set; } = "5432";
        public string Usuario { get; set; } = "postgres";
        public string Senha { get; set; } = "";
        public string Banco { get; set; } = "sistemaescritorio";
    }

    // NOVA CLASSE: Para guardar quem está operando o sistema no momento
    public static class Sessao
    {
        public static string OperadorAtual { get; set; } = "Sistema";
    }

    public class ConexaoDB
    {
        private static string caminhoConfig = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config_db.json");

        // Algoritmo de Criptografia Simples para ofuscar o arquivo JSON
        private static string Criptografar(string textoPlano)
        {
            if (string.IsNullOrEmpty(textoPlano)) return textoPlano;
            byte[] bytes = Encoding.UTF8.GetBytes(textoPlano);
            for (int i = 0; i < bytes.Length; i++) bytes[i] = (byte)(bytes[i] ^ 42); // Chave XOR
            return Convert.ToBase64String(bytes);
        }

        private static string Descriptografar(string textoCifrado)
        {
            if (string.IsNullOrEmpty(textoCifrado)) return textoCifrado;
            try
            {
                byte[] bytes = Convert.FromBase64String(textoCifrado);
                for (int i = 0; i < bytes.Length; i++) bytes[i] = (byte)(bytes[i] ^ 42);
                return Encoding.UTF8.GetString(bytes);
            }
            catch { return textoCifrado; }
        }

        public static ConfigDados LerConfiguracao()
        {
            if (File.Exists(caminhoConfig))
            {
                string json = File.ReadAllText(caminhoConfig);
                var config = JsonSerializer.Deserialize<ConfigDados>(json);
                config.Senha = Descriptografar(config.Senha); // Descriptografa ao ler
                return config;
            }
            return new ConfigDados();
        }

        public static void SalvarConfiguracao(ConfigDados config)
        {
            // Cria um clone para salvar a senha criptografada sem alterar a tela em tempo real
            var configSegura = new ConfigDados
            {
                Servidor = config.Servidor,
                Porta = config.Porta,
                Usuario = config.Usuario,
                Banco = config.Banco,
                Senha = Criptografar(config.Senha) // Criptografa ao salvar
            };

            string json = JsonSerializer.Serialize(configSegura, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(caminhoConfig, json);
        }

        public NpgsqlConnection Conectar()
        {
            ConfigDados config = LerConfiguracao();
            string stringConexao = $"Server={config.Servidor};Port={config.Porta};User Id={config.Usuario};Password={config.Senha};Database={config.Banco};";

            NpgsqlConnection conexao = new NpgsqlConnection(stringConexao);
            conexao.Open();
            return conexao;
        }
    }
}