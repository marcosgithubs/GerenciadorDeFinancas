using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.Json;

class Program
{
    static void Main()
    {
        Console.Clear();
        decimal salario = 0;
        string PATH = Path.GetFullPath("Dados");
        List<Despesa> despesas = new();
        int proximoId = 1;

        if (File.Exists(PATH + "despesas.json") && File.Exists(PATH + "salario.json"))
        {
            string jsonDespesas = File.ReadAllText(PATH + "despesas.json");
            string jsonSalario = File.ReadAllText(PATH + "salario.json");

            try
            {
                var dadosDespesas = JsonSerializer.Deserialize<List<Despesa>>(jsonDespesas);
                var dadosSalario = JsonSerializer.Deserialize<DadosFinanceiros>(jsonSalario);

                if (dadosDespesas != null)
                {
                    despesas = dadosDespesas;
                    if (despesas.Count > 0)
                        proximoId = despesas.Max(d => d.Id) + 1;
                }
                if (dadosSalario != null)
                    salario = dadosSalario.Salario;
            }
            catch
            {
                despesas = new();
            }
        }

        if (salario == 0)
        {
            Console.Write("Digite seu salário mensal: ");
            if (decimal.TryParse(Console.ReadLine(), out decimal s))
            {
                salario = s;
                SalvarSalario(PATH, salario);
            }
        }

        while (true)
        {
            Console.Clear();
            Console.WriteLine("Gerenciador de Finanças");
            Console.WriteLine("0 - Ver Saldo");
            Console.WriteLine("1 - Adicionar Despesa");
            Console.WriteLine("2 - Remover Despesa");
            Console.WriteLine("3 - Listar Despesas");
            Console.WriteLine("4 - Editar Despesa");
            Console.WriteLine("5 - Relatório por Categoria");
            Console.WriteLine("6 - Editar Salário");
            Console.WriteLine("7 - Previsão Orçamento");
            Console.WriteLine("8 - Sair");
            Console.Write("Escolha uma opção: ");
            string? opcaoInput = Console.ReadLine();

            if (int.TryParse(opcaoInput, out int opcao))
            {
                switch (opcao)
                {
                    case 0:
                        Funcao.VerSaldo(despesas, salario);
                        break;
                    case 1:
                        Funcao.AdicionarDespesa(despesas, ref proximoId, PATH);
                        break;
                    case 2:
                        Funcao.RemoverDespesa(despesas, PATH);
                        break;
                    case 3:
                        Funcao.ListarDespesas(despesas, salario);
                        break;
                    case 4:
                        Funcao.EditarDespesa(despesas, PATH);
                        break;
                    case 5:
                        Funcao.RelatorioPorCategoria(despesas);
                        break;
                    case 6:
                        salario = Funcao.EditarSalario(PATH);
                        break;
                    case 7:
                        Funcao.PrevisaoPorMeses(despesas, salario);
                        break;
                    case 8:
                        return;
                }
            }

            Console.WriteLine("\nPressione Enter para continuar...");
            Console.ReadLine();
        }
    }

    static void SalvarSalario(string path, decimal salario)
    {
        var dados = new DadosFinanceiros { Salario = salario };
        string json = JsonSerializer.Serialize(dados);
        File.WriteAllText(path + "salario.json", json);
    }
}

public class Funcao
{
    public static void AdicionarDespesa(List<Despesa> despesas, ref int id, string path)
    {
        Console.Write("Descrição: ");
        string? nome = Console.ReadLine();

        Console.Write("Valor (use vírgula): ");
        if (!decimal.TryParse(Console.ReadLine(), out decimal valor))
        {
            Console.WriteLine("Valor inválido.");
            return;
        }
        Console.Write("Divída é mensal (aluguel, etc.) sim/não: ");
        string? mensal = Console.ReadLine();
        bool isMensal = false;
        if (mensal != null)
        {
            if (mensal.ToLower().Contains("n"))
            {
                isMensal = false;
            }
            else if (mensal.ToLower().Contains("s"))
            {
                isMensal = true;
            }
        }
        DateTime data1 = DateTime.MinValue;
        if (isMensal == false)
        {
            Console.Write("Data (dd/mm/aaaa): ");
            if (!DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime data))
            {
                Console.WriteLine("Data inválida.");
                data1 = data;
                return;
            }
        }

        Console.Write("Categoria: ");
        string? categoria = Console.ReadLine();
        despesas.Add(new Despesa
        {
            Id = id++,
            Descricao = nome ?? "Sem descrição",
            Valor = valor,
            Data = data1,
            Categoria = categoria ?? "Outros",
            IsMensal = isMensal
        });

        SalvarDespesas(despesas, path);
        Console.WriteLine("Despesa adicionada.");
    }

    public static void RemoverDespesa(List<Despesa> despesas, string path)
    {
        if (despesas.Count == 0)
        {
            Console.WriteLine("Nenhuma despesa para remover.");
            return;
        }

        ListarDespesasSimples(despesas);

        Console.Write("ID da despesa para remover: ");
        if (int.TryParse(Console.ReadLine(), out int id))
        {
            var d = despesas.FirstOrDefault(d => d.Id == id);
            if (d != null)
            {
                despesas.Remove(d);
                SalvarDespesas(despesas, path);
                Console.WriteLine("Despesa removida.");
            }
            else
                Console.WriteLine("ID não encontrado.");
        }
    }

    public static void EditarDespesa(List<Despesa> despesas, string path)
    {
        if (despesas.Count == 0)
        {
            Console.WriteLine("Nenhuma despesa para editar.");
            return;
        }

        ListarDespesasSimples(despesas);

        Console.Write("ID da despesa para editar: ");
        if (!int.TryParse(Console.ReadLine(), out int id))
        {
            Console.WriteLine("ID inválido.");
            return;
        }

        var despesa = despesas.FirstOrDefault(d => d.Id == id);
        if (despesa == null)
        {
            Console.WriteLine("Despesa não encontrada.");
            return;
        }

        Console.Write("Nova descrição: ");
        despesa.Descricao = Console.ReadLine() ?? despesa.Descricao;

        Console.Write("Novo valor: ");
        if (decimal.TryParse(Console.ReadLine(), out decimal valor))
            despesa.Valor = valor;

        Console.Write("Nova data (dd/mm/aaaa): ");
        if (DateTime.TryParseExact(Console.ReadLine(), "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime data))
            despesa.Data = data;

        Console.Write("Nova categoria: ");
        string? categoria = Console.ReadLine();
        if (!string.IsNullOrWhiteSpace(categoria))
            despesa.Categoria = categoria;

        SalvarDespesas(despesas, path);
        Console.WriteLine("Despesa editada.");
    }

    public static void ListarDespesas(List<Despesa> despesas, decimal salario)
    {
        if (despesas.Count == 0)
        {
            Console.WriteLine("Nenhuma despesa registrada.");
            return;
        }
        Console.WriteLine("\nDespesas:");
        foreach (var d in despesas.OrderBy(d => d.Data))
        {
            var valor = "";
            if (d.IsMensal == false) { valor = ($"{d.Data} | {d.Categoria}").ToString(); }
            else { valor = ($"Pago Mensalmente | {d.Categoria}").ToString(); }
            Console.WriteLine($"{d.Id} - {d.Descricao} | {d.Valor:C} | {valor}");
        }
        decimal total = despesas.Sum(d => d.Valor);
        Console.WriteLine($"\nTotal de despesas: {total:C}");
        Console.WriteLine($"Salário: {salario:C}");
        Console.WriteLine(salario - total >= 0 ? $"Sobra: {salario - total:C}" : $"Dívida: {salario - total:C}");
    }

    public static void ListarDespesasSimples(List<Despesa> despesas)
    {
        Console.WriteLine("\nDespesas:");
        foreach (var d in despesas)
        {
            Console.WriteLine($"{d.Id} - {d.Descricao} | {d.Valor:C}");
        }
    }

    public static void RelatorioPorCategoria(List<Despesa> despesas)
    {
        if (despesas.Count == 0)
        {
            Console.WriteLine("Nenhuma despesa.");
            return;
        }

        var relatorio = despesas
            .GroupBy(d => d.Categoria)
            .Select(g => new
            {
                Categoria = g.Key,
                Total = g.Sum(x => x.Valor),
                Quantidade = g.Count()
            })
            .OrderByDescending(x => x.Total);

        Console.WriteLine("\nRelatório por Categoria:");
        foreach (var item in relatorio)
        {
            Console.WriteLine($"{item.Categoria}: {item.Quantidade} despesas - Total: {item.Total:C}");
        }
    }

    public static void VerSaldo(List<Despesa> despesas, decimal salario)
    {
        decimal total = despesas.Sum(d => d.Valor);
        Console.WriteLine($"Salário: {salario:C}");
        Console.WriteLine($"Despesas totais: {total:C}");
        Console.WriteLine(salario - total >= 0 ? $"Sobra: {salario - total:C}" : $"Dívida: {salario - total:C}");
    }

    public static decimal EditarSalario(string path)
    {
        Console.Write("Novo salário: ");
        if (decimal.TryParse(Console.ReadLine(), out decimal salario))
        {
            var dados = new DadosFinanceiros { Salario = salario };
            string json = JsonSerializer.Serialize(dados);
            File.WriteAllText(path + "salario.json", json);
            Console.WriteLine("Salário atualizado!");
            return salario;
        }
        Console.WriteLine("Entrada inválida.");
        return 0;
    }

    static void SalvarDespesas(List<Despesa> despesas, string path)
    {
        string json = JsonSerializer.Serialize(despesas);
        File.WriteAllText(path + "despesas.json", json);
    }

    public static void PrevisaoPorMeses(List<Despesa> despesas, decimal salario)
    {
        Console.WriteLine("Digite quantos meses deseja prever:");
        if (!int.TryParse(Console.ReadLine(), out int meses) || meses <= 0)
        {
            Console.WriteLine("Número de meses inválido.");
            return;
        }

        decimal previsaoSalario = salario * meses;
        decimal dividaTotal = 0;
        DateTime dataAtual = DateTime.Now;

        Console.WriteLine($"\nDetalhamento da Previsão para {meses} mês(es):\n");

        for (int i = 0; i < meses; i++)
        {
            DateTime mesAlvo = dataAtual.AddMonths(i);
            decimal dividaMes = 0;

            foreach (var d in despesas)
            {
                if (d.IsMensal)
                {
                    dividaMes += d.Valor;
                }
                else if (d.Data.Month == mesAlvo.Month && d.Data.Year == mesAlvo.Year)
                {
                    dividaMes += d.Valor;
                }
            }

            dividaTotal += dividaMes;
            decimal salarioMes = salario;
            decimal lucroMes = salarioMes - dividaMes;

            Console.WriteLine($"Mês {i + 1}: {mesAlvo.ToString("MMMM/yyyy", new CultureInfo("pt-BR"))}");
            Console.WriteLine($"  → Salário: {salarioMes:C}");
            Console.WriteLine($"  → Despesas: {dividaMes:C}");
            Console.WriteLine($"  → Lucro: {lucroMes:C}");
        }

        Console.WriteLine($"\nResumo Final:");
        Console.WriteLine($"→ Salário total previsto: {previsaoSalario:C}");
        Console.WriteLine($"→ Dívida total prevista: {dividaTotal:C}");
        Console.WriteLine($"→ Lucro total previsto: {previsaoSalario - dividaTotal:C}");
    }
}

public class DadosFinanceiros
{
    public decimal Salario { get; set; }
}

public class Despesa
{
    public int Id { get; set; }
    public string Descricao { get; set; } = "";
    public decimal Valor { get; set; }
    public DateTime Data { get; set; }
    public string Categoria { get; set; } = "";
    public bool IsMensal { get; set; }
}
