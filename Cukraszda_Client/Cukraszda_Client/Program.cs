using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using Cukraszda_Server.Protos;

class Program
{
    static async Task Main(string[] args)
    {
        using var channel = GrpcChannel.ForAddress("https://localhost:7068");
        var authClient = new AuthService.AuthServiceClient(channel);
        var cukraszdaClient = new CukraszdaService.CukraszdaServiceClient(channel);

        string token = await Authenticate(authClient);
        if (string.IsNullOrEmpty(token))
        {
            Console.WriteLine("Hitelesítés sikertelen!");
            return;
        }

        bool exit = false;

        while (!exit)
        {
            Console.WriteLine();

            Console.WriteLine("Válasszon a lekérdezések közül:");
            Console.WriteLine("1. Díjazott torták nevei ábécérendben");
            Console.WriteLine("2. Típusonkénti sütemények átlagos ára adott mértékegység szerint");
            Console.WriteLine("3. Laktózmentes piték és tortaszeletek nevei és típusai");
            Console.WriteLine("4. Sütemény hozzáadása");
            Console.WriteLine("5. Sütemény módosítása");
            Console.WriteLine("6. Sütemény törlése");
            Console.WriteLine("7. Sütemény lekérdezése azonosító alapján");
            Console.WriteLine("8. Kilépés");


            Console.WriteLine();

            var choice = Console.ReadLine();

            Console.WriteLine();

            switch (choice)
            {
                case "1":
                    await GetDijazottTortak(cukraszdaClient, token);
                    break;
                case "2":
                    await GetAtlagArTipusonkent(cukraszdaClient, token);
                    break;
                case "3":
                    await GetLaktozmentesPitekEsTortaszeletek(cukraszdaClient, token);
                    break;
                case "4":
                    await CreateSutemeny(cukraszdaClient, token);
                    break;
                case "5":
                    await UpdateSutemeny(cukraszdaClient, token);
                    break;
                case "6":
                    await DeleteSutemeny(cukraszdaClient, token);
                    break;
                case "7":
                    await GetSutemenyById(cukraszdaClient, token);
                    break;
                case "8":
                    exit = true;
                    Console.WriteLine("Kilépés...");
                    break;
                default:
                    Console.WriteLine("Érvénytelen választás!");
                    break;
            }

            Console.WriteLine();
        }
    }

    private static async Task<string> Authenticate(AuthService.AuthServiceClient authClient)
    {
        while (true)
        {
            Console.WriteLine("Felhasználónév:");
            var username = Console.ReadLine();
            Console.WriteLine("Jelszó:");
            var password = Console.ReadLine();

            try
            {
                var response = await authClient.AuthenticateAsync(new AuthRequest
                {
                    Username = username,
                    Password = password
                });

                if (response.Success)
                {
                    Console.WriteLine();
                    Console.WriteLine("Hitelesítés sikeres!");
                    Console.WriteLine();
                    return response.Token;
                }
                else
                {
                    Console.WriteLine("Helytelen felhasználónév vagy jelszó. Próbálja újra!");
                    Console.WriteLine();
                }
            }
            catch (RpcException ex)
            {
                Console.WriteLine($"Hiba történt a hitelesítés során.");
                Console.WriteLine("Próbálja újra!");
                Console.WriteLine();
            }
        }
    }


    private static async Task GetDijazottTortak(CukraszdaService.CukraszdaServiceClient client, string token)
    {
        Console.WriteLine("Díjazott torták nevei ábécérendben:");
        var headers = new Metadata { { "authorization", token } };
        var response = await client.GetDijazottTortakAsync(new Google.Protobuf.WellKnownTypes.Empty(), headers);
        foreach (var suti in response.Sutemenyek)
        {
            Console.WriteLine($"- {suti.Nev}");
        }
    }

    private static async Task GetAtlagArTipusonkent(CukraszdaService.CukraszdaServiceClient client, string token)
    {
        Console.Write("Adja meg a mértékegységet (pl. 'db, kg, rúd, 8/16/24 szeletes'): ");
        var mertekegyseg = Console.ReadLine();

        if (string.IsNullOrWhiteSpace(mertekegyseg))
        {
            Console.WriteLine("A mértékegység nem lehet üres!");
            return;
        }

        var headers = new Metadata { { "authorization", token } };
        var response = await client.GetAtlagArTipusonkentAsync(new AtlagArRequest { Mertekegyseg = mertekegyseg }, headers);
        Console.WriteLine($"Átlagárak '{mertekegyseg}' mértékegység szerint:");
        foreach (var tipus in response.AtlagArak)
        {
            Console.WriteLine($"- {tipus.Tipus}: {tipus.AtlagAr_} {mertekegyseg}");
        }
    }

    private static async Task GetLaktozmentesPitekEsTortaszeletek(CukraszdaService.CukraszdaServiceClient client, string token)
    {
        var request = new LaktozmentesRequest
        {
            Mentes = "L",
            Tipusok = { "pite", "tortaszelet" }
        };

        var headers = new Metadata { { "authorization", token } };
        var response = await client.GetLaktozmentesPitekEsTortaszeletekAsync(request, headers);
        Console.WriteLine("Laktózmentes édességek (pite és tortaszelet):");
        foreach (var suti in response.Sutemenyek)
        {
            Console.WriteLine($"- {suti.Nev} ({suti.Tipus})");
        }
    }
    private static async Task CreateSutemeny(CukraszdaService.CukraszdaServiceClient client, string token)
    {
        Console.WriteLine("Sütemény hozzáadása:");

        Console.Write("Név: ");
        var nev = Console.ReadLine();

        Console.Write("Típus: ");
        var tipus = Console.ReadLine();

        Console.Write("Díjazott? (true/false): ");
        var dijazott = bool.Parse(Console.ReadLine());

        var headers = new Metadata { { "authorization", token } };
        var response = await client.CreateSutemenyAsync(new SutemenyData
        {
            Nev = nev,
            Tipus = tipus,
            Dijazott = dijazott
        }, headers);

        Console.WriteLine(response.Message);
    }
    private static async Task UpdateSutemeny(CukraszdaService.CukraszdaServiceClient client, string token)
    {
        Console.WriteLine("Sütemény módosítása:");

        Console.Write("Azonosító: ");
        var id = int.Parse(Console.ReadLine());

        Console.Write("Új név: ");
        var nev = Console.ReadLine();

        Console.Write("Új típus: ");
        var tipus = Console.ReadLine();

        Console.Write("Díjazott? (true/false): ");
        var dijazott = bool.Parse(Console.ReadLine());

        var headers = new Metadata { { "authorization", token } };
        var response = await client.UpdateSutemenyAsync(new SutemenyData
        {
            Id = id,
            Nev = nev,
            Tipus = tipus,
            Dijazott = dijazott
        }, headers);

        Console.WriteLine(response.Message);
    }
    private static async Task DeleteSutemeny(CukraszdaService.CukraszdaServiceClient client, string token)
    {
        Console.WriteLine("Sütemény törlése:");

        Console.Write("Azonosító: ");
        var id = int.Parse(Console.ReadLine());

        var headers = new Metadata { { "authorization", token } };
        await client.DeleteSutemenyAsync(new SutemenyIdRequest { Id = id }, headers);

        Console.WriteLine("Sütemény törölve.");
    }
    private static async Task GetSutemenyById(CukraszdaService.CukraszdaServiceClient client, string token)
    {
        Console.WriteLine("Sütemény lekérdezése azonosító alapján:");

        Console.Write("Azonosító: ");
        var id = int.Parse(Console.ReadLine());

        var headers = new Metadata { { "authorization", token } };
        var response = await client.GetSutemenyByIdAsync(new SutemenyIdRequest { Id = id }, headers);

        Console.WriteLine($"Név: {response.Nev}");
        Console.WriteLine($"Típus: {response.Tipus}");
        Console.WriteLine($"Díjazott: {response.Dijazott}");
    }

}
