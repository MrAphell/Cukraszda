using Grpc.Core;
using Cukraszda_Server.Protos;
using Google.Protobuf.WellKnownTypes;

namespace Cukraszda_Server.Services
{
    public class CukraszdaServiceImpl : CukraszdaService.CukraszdaServiceBase
    {
        private readonly DataService dataService;

        public CukraszdaServiceImpl(DataService dataService)
        {
            this.dataService = dataService;
        }

        private void ValidateToken(ServerCallContext context)
        {
            var token = context.RequestHeaders.FirstOrDefault(h => h.Key == "authorization")?.Value;
            if (token != "valid_token")
            {
                throw new RpcException(new Status(StatusCode.Unauthenticated, "Érvénytelen vagy hiányzó token."));
            }
        }


        public override Task<SutemenyLista> GetDijazottTortak(Google.Protobuf.WellKnownTypes.Empty request, ServerCallContext context)
        {
            ValidateToken(context);

            var sutemenyek = dataService.GetSutemenyek();
            var dijazottTortak = sutemenyek
                .Where(suti => suti.Dijazott)
                .OrderBy(suti => suti.Nev)
                .ToList();

            var sutemenyLista = new SutemenyLista();
            sutemenyLista.Sutemenyek.AddRange(dijazottTortak);

            return Task.FromResult(sutemenyLista);
        }


        public override Task<AtlagArLista> GetAtlagArTipusonkent(AtlagArRequest request, ServerCallContext context)
        {
            ValidateToken(context);

            var supportedUnits = new List<string> { "db", "kg", "rúd", "8 szeletes", "16 szeletes", "24 szeletes" };

            if (string.IsNullOrWhiteSpace(request.Mertekegyseg) || !supportedUnits.Contains(request.Mertekegyseg.ToLower()))
            {
                throw new RpcException(new Status(StatusCode.InvalidArgument,
                    $"Érvénytelen mértékegység: '{request.Mertekegyseg}'. Támogatott mértékegységek: {string.Join(", ", supportedUnits)}"));
            }

            var sutemenyek = dataService.GetSutemenyek();
            var atlagArak = new Dictionary<string, List<double>>();

            foreach (var suti in sutemenyek)
            {
                foreach (var ar in suti.Arak)
                {
                    if (ar.Egyseg.ToLower() == request.Mertekegyseg.ToLower())
                    {
                        if (!atlagArak.ContainsKey(suti.Tipus))
                        {
                            atlagArak[suti.Tipus] = new List<double>();
                        }
                        atlagArak[suti.Tipus].Add(ar.Ertek);
                    }
                }
            }

            var result = new AtlagArLista();

            foreach (var tipus in atlagArak.Keys)
            {
                var atlag = 0.0;
                foreach (var ar in atlagArak[tipus])
                {
                    atlag += ar;
                }

                atlag /= atlagArak[tipus].Count;

                result.AtlagArak.Add(new AtlagAr { Tipus = tipus, AtlagAr_ = atlag });
            }

            return Task.FromResult(result);
        }

        public override Task<SutemenyLista> GetLaktozmentesPitekEsTortaszeletek(LaktozmentesRequest request, ServerCallContext context)
        {
            ValidateToken(context);

            var sutemenyek = dataService.GetSutemenyek();
            var result = new SutemenyLista();

            foreach (var suti in sutemenyek)
            {
                if (request.Tipusok.Contains(suti.Tipus))
                {
                    foreach (var tartalom in suti.Tartalmak)
                    {
                        if (tartalom.Mentes == request.Mentes)
                        {
                            result.Sutemenyek.Add(suti);
                            break;
                        }
                    }
                }
            }

            return Task.FromResult(result);
        }

        public override Task<SutemenyResponse> CreateSutemeny(SutemenyData request, ServerCallContext context)
        {
            ValidateToken(context);

            var sutemenyId = dataService.AddSutemeny(request);

            return Task.FromResult(new SutemenyResponse
            {
                Message = "Sütemény sikeresen hozzáadva.",
                SutemenyId = sutemenyId
            });
        }


        public override Task<SutemenyResponse> UpdateSutemeny(SutemenyData request, ServerCallContext context)
        {
            ValidateToken(context);

            bool success = dataService.UpdateSutemeny(request);
            if (!success)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "A megadott azonosítójú sütemény nem található."));
            }

            return Task.FromResult(new SutemenyResponse
            {
                Message = "Sütemény sikeresen frissítve.",
                SutemenyId = request.Id
            });
        }


        public override Task<Empty> DeleteSutemeny(SutemenyIdRequest request, ServerCallContext context)
        {
            ValidateToken(context);

            bool success = dataService.DeleteSutemeny(request.Id);
            if (!success)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "A megadott azonosítójú sütemény nem található."));
            }

            return Task.FromResult(new Empty());
        }


        public override Task<SutemenyData> GetSutemenyById(SutemenyIdRequest request, ServerCallContext context)
        {
            ValidateToken(context);

            var sutemeny = dataService.GetSutemenyById(request.Id);
            if (sutemeny == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound, "A megadott azonosítójú sütemény nem található."));
            }

            return Task.FromResult(sutemeny);
        }


    }
}
