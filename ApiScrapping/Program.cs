﻿using ApiScrapping;

Console.Write("\t-== Api Scrapper ==-");
Console.WriteLine("\t\t[ Andre Herman (amelco.herman@gmail.com) ]");
Console.WriteLine($"\tLeia o arquivo de configuração: {Constantes.NomeArquivoConfig}\n\n");

var config = new Config(args);
var scrapper = new Scrapper(config);

Console.WriteLine("   # Verificando código de retorno das rotas...");
foreach (var rota in config.Rotas)
{
    try
    {
        scrapper.Navega(rota);

    }
    catch (Exception e)
    {
        Console.WriteLine($"\n=> Erro:\n   {e.Message}\n   {e.InnerException?.Message ?? ""}\n=> ### {Constantes.Mensagens.VerifiqueArquivoConfig}");
    }
}

Console.WriteLine("\n   Rotas que apresentaram erros\n");
foreach (var r in scrapper.RotasComErro)
{
    Console.WriteLine("   * " + r.ToString());
}
Console.WriteLine("\n   # Verificação concluída.");
