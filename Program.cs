﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ConsoleTables;
using Docker.DotNet;
using Docker.DotNet.Models;

class Program
{
    static async Task Main(string [] args)
    {

        var dockerClient = new DockerClientConfiguration(new Uri("npipe://./pipe/docker_engine")).CreateClient();

        while (true)
        {
            Console.Clear();
            //Criação do Menu
            Console.WriteLine("Selecione uma opção: ");
            Console.WriteLine("1. Criar container SQL Server");
            Console.WriteLine("2. Listar containers");
            Console.WriteLine("3. Iniciar todos os containers");
            Console.WriteLine("4. Parar todos os containers");
            Console.WriteLine("5. Sair");

            Console.Write("Opção: ");
            string option = Console.ReadLine();

            switch(option)
            {
                case "1":
                    Console.Write("Digite a porta do host: ");
                    string hostPort = Console.ReadLine();
                    Console.Write("Digite o nome do Container: ");
                    string containerName = Console.ReadLine();
                    await CreateSqlServerContainerAsync(dockerClient,hostPort,containerName);
                    break;
                case "2":
                    await ListAndInteractWithContainersAsync(dockerClient);
                    break;
                case "3":
                    await StartAllContainersAsync(dockerClient);
                    break;
                case "4":
                    await StopAllContainersAsync(dockerClient);
                    break;
                case "5":
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Opção inválida!");
                    break;
            }

        }
    }    

    static async Task CreateSqlServerContainerAsync(DockerClient dockerClient, string hostPort, string containerName )
    {
        Console.Clear();
        //Configuração dos dados
        string saPassword = "1s860t77r@";
        string acceptEula = "Y";
        string colation = "Latin1_General_CI_CA";

        //Container Sql Server
        var createParameters = new CreateContainerParameters
        {
            Image = "mcr.microsoft.com/mssql/server:2019-latest",
            Env = new[] { "ACCEPT_EULA=" + acceptEula, "SA_PASSWORD=" + saPassword, "MSSQL_COLLATION=" + colation },
            HostConfig = new HostConfig
            {
                PortBindings = new Dictionary<string, IList<PortBinding>>
                {
                    {"1433/tcp", new List<PortBinding> { new PortBinding{ HostPort = hostPort} } }
                }
            },
            Name = containerName,
            ExposedPorts = new Dictionary<string, EmptyStruct> { { "1433/tcp", default(EmptyStruct) } },
        };
        // cria o container com os parametros informados
        var response = await dockerClient.Containers.CreateContainerAsync(createParameters);
        Console.WriteLine("Container SQL Server criado com sucesso.");
    }

    //Inicia todos os containers
    static async Task StartAllContainersAsync(DockerClient dockerClient)
    {
        var containers = await dockerClient.Containers.ListContainersAsync(new ContainersListParameters { All = true }); // busca todos os containers, até os desligados
        foreach (var container in containers)
        {
            await dockerClient.Containers.StartContainerAsync(container.ID, new ContainerStartParameters());
            string containerName = string.Join(", ", container.Names);
            Console.WriteLine($"Container {containerName} iniciado");
        }
        Console.WriteLine("Todos os containers foram iniciados.");

        // Opção para voltar ao menu principal
        Console.WriteLine("Pressione qualquer tecla para voltar ao menu principal.");
        Console.ReadKey();
    }

    static async Task StopAllContainersAsync(DockerClient dockerClient)
    {
        var containers = await dockerClient.Containers.ListContainersAsync(new ContainersListParameters { All = true });
        foreach (var container in containers)
        {
            await dockerClient.Containers.StopContainerAsync(container.ID, new ContainerStopParameters());
            string containerName = string.Join(", ", container.Names);
            Console.WriteLine($"Container {containerName} parado");
        }

        // Opção para voltar ao menu principal
        Console.WriteLine("Pressione qualquer tecla para voltar ao menu principal.");
        Console.ReadKey();
    }

    static async Task ListAndInteractWithContainersAsync(DockerClient dockerClient)
    {
        Console.Clear();
        // Listar containers
        var containers = await dockerClient.Containers.ListContainersAsync(new ContainersListParameters { All = true }); // inclui containers parados

        Console.WriteLine("Listando containers Docker:");

        //cria uma tabela para mostrar os containers
        var table = new ConsoleTable("Índice", "Nome", "Status","Portas Publicas" ,"Portas Privadas");

        for (int i = 0; i < containers.Count; i++)
        {
            table.AddRow(i + 1, containers[i].Names[0], containers[i].State, string.Join(", ", containers[i].Ports.Select(p => p.PublicPort)) , string.Join(", ", containers[i].Ports.Select(p => p.PrivatePort)));
        }

        table.Write();

        // Selecionar um container para interagir
        Console.WriteLine("Selecione o número do container para interagir, '0' para voltar ao menu principal ou 'sair' para encerrar o programa: ");
        string option = Console.ReadLine();

        if (option == "0")
            return;
        else if (option.ToLower() == "sair")
            Environment.Exit(0);

        if (int.TryParse(option, out int containerIndex) && containerIndex >= 1 && containerIndex <= containers.Count)
        {
            var selectedContainer = containers[containerIndex - 1];
            Console.WriteLine($"Selecionado: {selectedContainer.Names[0]}");

            // Opções de interação
            Console.WriteLine("Opções de interação:");
            Console.WriteLine("1. Reiniciar container");
            Console.WriteLine("2. Parar container");
            Console.WriteLine("3. Iniciar container");
            Console.WriteLine("4. Voltar ao Menu");
            Console.Write("Opção: ");
            string interactionOption = Console.ReadLine();

            switch (interactionOption)
            {
                case "1":
                    await dockerClient.Containers.RestartContainerAsync(selectedContainer.ID, new ContainerRestartParameters());
                    Console.WriteLine("Container reiniciado com sucesso.");
                    break;
                case "2":
                    await dockerClient.Containers.StopContainerAsync(selectedContainer.ID, new ContainerStopParameters());
                    Console.WriteLine("Container parado com sucesso.");
                    break;
                case "3":
                    await dockerClient.Containers.StartContainerAsync(selectedContainer.ID, new ContainerStartParameters());
                    Console.WriteLine("Container iniciado com sucesso.");
                    break;
                case "4":
                    break;
                default:
                    Console.WriteLine("Opção inválida!");
                    break;
            }
        }
        else
        {
            Console.WriteLine("Número de container inválido.");
        };
    }
}

