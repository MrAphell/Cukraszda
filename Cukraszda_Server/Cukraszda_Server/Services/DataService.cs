using MySql.Data.MySqlClient;
using Cukraszda_Server.Protos;

namespace Cukraszda_Server.Services
{
    public class DataService
    {
        private readonly string connectionString;

        public DataService(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public List<SutemenyData> GetSutemenyek()
        {
            var sutemenyek = new List<SutemenyData>();

            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                var query = @"
                    SELECT 
                        s.id AS SutiId, 
                        s.nev, 
                        s.tipus, 
                        s.dijazott, 
                        a.id AS ArId, 
                        a.ertek, 
                        a.egyseg, 
                        t.id AS TartalomId, 
                        t.mentes
                    FROM suti s
                    LEFT JOIN ar a ON s.id = a.sutiid
                    LEFT JOIN tartalom t ON s.id = t.sutiid";

                using (var command = new MySqlCommand(query, connection))
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var sutiId = reader.GetInt32("SutiId");
                        var sutemeny = sutemenyek.Find(s => s.Id == sutiId);

                        if (sutemeny == null)
                        {
                            sutemeny = new SutemenyData
                            {
                                Id = sutiId,
                                Nev = reader.GetString("nev"),
                                Tipus = reader.GetString("tipus"),
                                Dijazott = reader.GetBoolean("dijazott"),
                                Arak = { },
                                Tartalmak = { }
                            };
                            sutemenyek.Add(sutemeny);
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("ArId")))
                        {
                            sutemeny.Arak.Add(new ArData
                            {
                                Id = reader.GetInt32("ArId"),
                                Ertek = reader.GetDouble("ertek"),
                                Egyseg = reader.GetString("egyseg")
                            });
                        }

                        if (!reader.IsDBNull(reader.GetOrdinal("TartalomId")))
                        {
                            sutemeny.Tartalmak.Add(new TartalomData
                            {
                                Id = reader.GetInt32("TartalomId"),
                                Mentes = reader.GetString("mentes")
                            });
                        }
                    }
                }
            }

            return sutemenyek;
        }

        public UserData GetUserByUsername(string username)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                var query = @"
                    SELECT id, username, password_hash, role
                    FROM users
                    WHERE username = @username";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@username", username);

                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new UserData
                            {
                                Id = reader.GetInt32("id"),
                                Username = reader.GetString("username"),
                                PasswordHash = reader.GetString("password_hash"),
                                Role = reader.GetString("role")
                            };
                        }
                    }
                }
            }

            return null;
        }

        public int AddSutemeny(SutemenyData sutemeny)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                var insertSutiQuery = @"
            INSERT INTO suti (nev, tipus, dijazott) 
            VALUES (@nev, @tipus, @dijazott);
            SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(insertSutiQuery, connection))
                {
                    command.Parameters.AddWithValue("@nev", sutemeny.Nev);
                    command.Parameters.AddWithValue("@tipus", sutemeny.Tipus);
                    command.Parameters.AddWithValue("@dijazott", sutemeny.Dijazott);

                    var sutiId = Convert.ToInt32(command.ExecuteScalar());

                    foreach (var ar in sutemeny.Arak)
                    {
                        var insertArQuery = @"
                    INSERT INTO ar (sutiid, ertek, egyseg) 
                    VALUES (@sutiid, @ertek, @egyseg)";

                        using (var arCommand = new MySqlCommand(insertArQuery, connection))
                        {
                            arCommand.Parameters.AddWithValue("@sutiid", sutiId);
                            arCommand.Parameters.AddWithValue("@ertek", ar.Ertek);
                            arCommand.Parameters.AddWithValue("@egyseg", ar.Egyseg);
                            arCommand.ExecuteNonQuery();
                        }
                    }

                    foreach (var tartalom in sutemeny.Tartalmak)
                    {
                        var insertTartalomQuery = @"
                    INSERT INTO tartalom (sutiid, mentes) 
                    VALUES (@sutiid, @mentes)";

                        using (var tartalomCommand = new MySqlCommand(insertTartalomQuery, connection))
                        {
                            tartalomCommand.Parameters.AddWithValue("@sutiid", sutiId);
                            tartalomCommand.Parameters.AddWithValue("@mentes", tartalom.Mentes);
                            tartalomCommand.ExecuteNonQuery();
                        }
                    }

                    return sutiId;
                }
            }
        }


        public bool UpdateSutemeny(SutemenyData sutemeny)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                var updateSutiQuery = @"
            UPDATE suti 
            SET nev = @nev, tipus = @tipus, dijazott = @dijazott 
            WHERE id = @id";

                using (var command = new MySqlCommand(updateSutiQuery, connection))
                {
                    command.Parameters.AddWithValue("@id", sutemeny.Id);
                    command.Parameters.AddWithValue("@nev", sutemeny.Nev);
                    command.Parameters.AddWithValue("@tipus", sutemeny.Tipus);
                    command.Parameters.AddWithValue("@dijazott", sutemeny.Dijazott);

                    var rowsAffected = command.ExecuteNonQuery();
                    if (rowsAffected == 0) return false;
                }

                var deleteArQuery = "DELETE FROM ar WHERE sutiid = @sutiid";
                using (var deleteCommand = new MySqlCommand(deleteArQuery, connection))
                {
                    deleteCommand.Parameters.AddWithValue("@sutiid", sutemeny.Id);
                    deleteCommand.ExecuteNonQuery();
                }

                foreach (var ar in sutemeny.Arak)
                {
                    var insertArQuery = @"
                INSERT INTO ar (sutiid, ertek, egyseg) 
                VALUES (@sutiid, @ertek, @egyseg)";

                    using (var arCommand = new MySqlCommand(insertArQuery, connection))
                    {
                        arCommand.Parameters.AddWithValue("@sutiid", sutemeny.Id);
                        arCommand.Parameters.AddWithValue("@ertek", ar.Ertek);
                        arCommand.Parameters.AddWithValue("@egyseg", ar.Egyseg);
                        arCommand.ExecuteNonQuery();
                    }
                }

                var deleteTartalomQuery = "DELETE FROM tartalom WHERE sutiid = @sutiid";
                using (var deleteCommand = new MySqlCommand(deleteTartalomQuery, connection))
                {
                    deleteCommand.Parameters.AddWithValue("@sutiid", sutemeny.Id);
                    deleteCommand.ExecuteNonQuery();
                }

                foreach (var tartalom in sutemeny.Tartalmak)
                {
                    var insertTartalomQuery = @"
                INSERT INTO tartalom (sutiid, mentes) 
                VALUES (@sutiid, @mentes)";

                    using (var tartalomCommand = new MySqlCommand(insertTartalomQuery, connection))
                    {
                        tartalomCommand.Parameters.AddWithValue("@sutiid", sutemeny.Id);
                        tartalomCommand.Parameters.AddWithValue("@mentes", tartalom.Mentes);
                        tartalomCommand.ExecuteNonQuery();
                    }
                }

                return true;
            }
        }


        public bool DeleteSutemeny(int id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                var deleteArQuery = "DELETE FROM ar WHERE sutiid = @id";
                using (var arCommand = new MySqlCommand(deleteArQuery, connection))
                {
                    arCommand.Parameters.AddWithValue("@id", id);
                    arCommand.ExecuteNonQuery();
                }

                var deleteTartalomQuery = "DELETE FROM tartalom WHERE sutiid = @id";
                using (var tartalomCommand = new MySqlCommand(deleteTartalomQuery, connection))
                {
                    tartalomCommand.Parameters.AddWithValue("@id", id);
                    tartalomCommand.ExecuteNonQuery();
                }

                var deleteSutiQuery = "DELETE FROM suti WHERE id = @id";
                using (var command = new MySqlCommand(deleteSutiQuery, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    var rowsAffected = command.ExecuteNonQuery();
                    return rowsAffected > 0;
                }
            }
        }


        public SutemenyData GetSutemenyById(int id)
        {
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();

                var query = @"
            SELECT 
                s.id AS SutiId, 
                s.nev, 
                s.tipus, 
                s.dijazott, 
                a.id AS ArId, 
                a.ertek, 
                a.egyseg, 
                t.id AS TartalomId, 
                t.mentes
            FROM suti s
            LEFT JOIN ar a ON s.id = a.sutiid
            LEFT JOIN tartalom t ON s.id = t.sutiid
            WHERE s.id = @id";

                using (var command = new MySqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@id", id);

                    using (var reader = command.ExecuteReader())
                    {
                        SutemenyData sutemeny = null;

                        while (reader.Read())
                        {
                            if (sutemeny == null)
                            {
                                sutemeny = new SutemenyData
                                {
                                    Id = reader.GetInt32("SutiId"),
                                    Nev = reader.GetString("nev"),
                                    Tipus = reader.GetString("tipus"),
                                    Dijazott = reader.GetBoolean("dijazott")
                                };
                            }

                            if (!reader.IsDBNull(reader.GetOrdinal("ArId")))
                            {
                                sutemeny.Arak.Add(new ArData
                                {
                                    Id = reader.GetInt32("ArId"),
                                    Ertek = reader.GetDouble("ertek"),
                                    Egyseg = reader.GetString("egyseg")
                                });
                            }

                            if (!reader.IsDBNull(reader.GetOrdinal("TartalomId")))
                            {
                                sutemeny.Tartalmak.Add(new TartalomData
                                {
                                    Id = reader.GetInt32("TartalomId"),
                                    Mentes = reader.GetString("mentes")
                                });
                            }
                        }

                        return sutemeny;
                    }
                }
            }
        }

    }

    public class UserData
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Role { get; set; }
    }
}
