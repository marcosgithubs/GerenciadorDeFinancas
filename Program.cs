using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
namespace GerenciadorFinancas
{
    public class Despesa
    {
        public int Id { get; set; }
        public string Descricao { get; set; }
        public decimal Valor { get; set; }
        public DateTime Data { get; set; }
        public string Categoria { get; set; }

        public Despesa(int id, string descricao, decimal valor, DateTime data, string categoria)
        {
            Id = id;
            Descricao = descricao;
            Valor = valor;
            Data = data;
            Categoria = categoria;
        }

        public override string ToString()
        {
            return $"{Id}: {Descricao} - {Valor:C} em {Data:dd/MM/yyyy} ({Categoria})";
        }
    }

    public class GerenciadorFinancas
    {
        public List<Despesa> despesas;
        private int proximoId;

        public GerenciadorFinancas()
        {
            despesas = new List<Despesa>();
            proximoId = 1;
        }

        public void AdicionarDespesa(string descricao, decimal valor, DateTime data, string categoria)
        {
            var despesa = new Despesa(proximoId++, descricao, valor, data, categoria);
            despesas.Add(despesa);
            Console.WriteLine("Despesa adicionada");
        }

        public void ListarDespesas()
        {
            if (despesas.Count == 0)
            {
                Console.WriteLine("Nenhuma despesa na Lista.");
                return;
            }

            Console.WriteLine("\nLista de Despesas:");
            foreach (var despesa in despesas.OrderBy(d => d.Data))
            {
                Console.WriteLine(despesa);
            }

            Console.WriteLine($"\nTotal de despesas: {despesas.Sum(d => d.Valor):C}");
        }

        public void EditarDespesa(int id, string descricao, decimal valor, DateTime data, string categoria)
        {
            var despesa = despesas.FirstOrDefault(d => d.Id == id);
            if (despesa == null)
            {
                Console.WriteLine("Despesa não encontrada.");
                return;
            }

            despesa.Descricao = descricao;
            despesa.Valor = valor;
            despesa.Data = data;
            despesa.Categoria = categoria;

            Console.WriteLine("Despesa atualizada com sucesso!");
        }

        public void RemoverDespesa(int id)
        {
            var despesa = despesas.FirstOrDefault(d => d.Id == id);
            if (despesa == null)
            {
                Console.WriteLine("Despesa não encontrada.");
                return;
            }

            despesas.Remove(despesa);
            Console.WriteLine("Despesa removida com sucesso!");
        }

        public void GerarRelatorioPorCategoria()
        {
            if (despesas.Count == 0)
            {
                Console.WriteLine("Nenhuma despesa cadastrada.");
                return;
            }

            var relatorio = despesas
                .GroupBy(d => d.Categoria)
                .Select(g => new
                {
                    Categoria = g.Key,
                    Total = g.Sum(d => d.Valor),
                    Quantidade = g.Count()
                })
                .OrderByDescending(r => r.Total);

            Console.WriteLine("\nRelatório por Categoria:");
            foreach (var item in relatorio)
            {
                Console.WriteLine($"{item.Categoria}: {item.Quantidade} despesas - Total: {item.Total:C}");
            }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var gerenciador = new GerenciadorFinancas();
            bool sair = false;

            Console.WriteLine("=== Gerenciador de Finanças Pessoais ===");

            while (!sair)
            {
                Console.WriteLine("\nMenu Principal:");
                Console.WriteLine("1. Adicionar Despesa");
                Console.WriteLine("2. Listar Despesas");
                Console.WriteLine("3. Editar Despesa");
                Console.WriteLine("4. Remover Despesa");
                Console.WriteLine("5. Relatório por Categoria");
                Console.WriteLine("6. Sair");
                Console.Write("Escolha uma opção: ");

                if (!int.TryParse(Console.ReadLine(), out int opcao))
                {
                    Console.WriteLine("Opção inválida. Tente novamente.");
                    continue;
                }

                switch (opcao)
                {
                    case 1:
                        AdicionarDespesa(gerenciador);
                        break;
                    case 2:
                        gerenciador.ListarDespesas();
                        break;
                    case 3:
                        EditarDespesa(gerenciador);
                        break;
                    case 4:
                        if (gerenciador.despesas.Count != 0)
                        {RemoverDespesa(gerenciador);}
                        else
                        { Console.WriteLine("Sem Despesas.");}
                        break;
                    case 5:
                        gerenciador.GerarRelatorioPorCategoria();
                        break;
                    case 6:
                        sair = true;
                        break;
                    default:
                        Console.WriteLine("Opção inválida. Pressione Enter.");
                        Console.Clear();
                        break;
                }
                Console.WriteLine("Opção inválida. Pressione Enter.");
                Console.ReadLine();
                Console.Clear();
            }
        }

        static void AdicionarDespesa(GerenciadorFinancas gerenciador)
        {
            Console.WriteLine("\nAdicionar Nova Despesa:");

            Console.Write("Descrição: ");
            string descricao = Console.ReadLine();

            Console.Write("Valor: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal valor))
            {
                Console.WriteLine("Valor inválido.");
                return;
            }

            Console.Write("Data (dd/mm/aaaa): ");
            if (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime data))
            {
                Console.WriteLine("Data inválida. Use o formato dd/mm/aaaa.");
                return;
            }

            Console.Write("Categoria: ");
            string categoria = Console.ReadLine();

            gerenciador.AdicionarDespesa(descricao, valor, data, categoria);
        }

        static void EditarDespesa(GerenciadorFinancas gerenciador)
        {
            Console.WriteLine("\nEditar Despesa:");
            gerenciador.ListarDespesas();

            Console.Write("ID da despesa a editar: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("ID inválido.");
                return;
            }

            Console.Write("Nova descrição: ");
            string descricao = Console.ReadLine();

            Console.Write("Novo valor: ");
            if (!decimal.TryParse(Console.ReadLine(), out decimal valor))
            {
                Console.WriteLine("Valor inválido.");
                return;
            }

            Console.Write("Nova data (dd/mm/aaaa): ");
            if (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime data))
            {
                Console.WriteLine("Data inválida. Use o formato dd/mm/aaaa.");
                return;
            }

            Console.Write("Nova categoria: ");
            string categoria = Console.ReadLine();

            gerenciador.EditarDespesa(id, descricao, valor, data, categoria);
        }

        static void RemoverDespesa(GerenciadorFinancas gerenciador)
        {
            Console.WriteLine("\nRemover Despesa:");
            gerenciador.ListarDespesas();

            Console.Write("ID da despesa a remover: ");
            if (!int.TryParse(Console.ReadLine(), out int id))
            {
                Console.WriteLine("ID inválido.");
                return;
            }

            gerenciador.RemoverDespesa(id);
        }
    }


}
