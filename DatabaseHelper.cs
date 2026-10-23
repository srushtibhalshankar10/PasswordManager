using System;
using System.Collections.Generic;
using System.Text;

namespace PasswordManager
{
    using Microsoft.Data.SqlClient;

    public class PasswordEntry
    {
        public int Id { get; set; }
        public string Website { get; set; } = "";
        public string Username { get; set; } = "";
        public string EncryptedPassword { get; set; } = "";
        public string IV { get; set; } = "";
    }

    public class DatabaseHelper
    {
        private readonly string _connectionString =
   @"Server=.\SQLEXPRESS;Database=PasswordManagerDB;Trusted_Connection=True;TrustServerCertificate=True;";

        public bool MasterExists()
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();
            using var cmd = new SqlCommand("SELECT COUNT(*) FROM MasterUser", con);
            return (int)cmd.ExecuteScalar() > 0;
        }

        public void SaveMaster(string hash, string salt, string keySalt)
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();
            using var cmd = new SqlCommand(
                "INSERT INTO MasterUser (PasswordHash, Salt, KeySalt) VALUES (@h, @s, @k)", con);
            cmd.Parameters.AddWithValue("@h", hash);
            cmd.Parameters.AddWithValue("@s", salt);
            cmd.Parameters.AddWithValue("@k", keySalt);
            cmd.ExecuteNonQuery();
        }

        public (string Hash, string Salt, string KeySalt)? GetMaster()
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();
            using var cmd = new SqlCommand(
                "SELECT TOP 1 PasswordHash, Salt, KeySalt FROM MasterUser", con);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
                return (reader.GetString(0), reader.GetString(1), reader.GetString(2));
            return null;
        }

        public void AddEntry(string website, string username, string encryptedPassword, string iv)
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();
            using var cmd = new SqlCommand(
                @"INSERT INTO PasswordEntries (WebsiteName, Username, EncryptedPassword, IV)
              VALUES (@w, @u, @p, @iv)", con);
            cmd.Parameters.AddWithValue("@w", website);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", encryptedPassword);
            cmd.Parameters.AddWithValue("@iv", iv);
            cmd.ExecuteNonQuery();
        }

        public List<PasswordEntry> GetAllEntries()
        {
            var list = new List<PasswordEntry>();
            using var con = new SqlConnection(_connectionString);
            con.Open();
            using var cmd = new SqlCommand(
                "SELECT Id, WebsiteName, Username, EncryptedPassword, IV FROM PasswordEntries", con);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new PasswordEntry
                {
                    Id = reader.GetInt32(0),
                    Website = reader.GetString(1),
                    Username = reader.GetString(2),
                    EncryptedPassword = reader.GetString(3),
                    IV = reader.GetString(4)
                });
            }
            return list;
        }

        public void DeleteEntry(int id)
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();
            using var cmd = new SqlCommand("DELETE FROM PasswordEntries WHERE Id = @id", con);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
        public void UpdateEntry(int id, string website, string username,
                        string encryptedPassword, string iv)
        {
            using var con = new SqlConnection(_connectionString);
            con.Open();
            using var cmd = new SqlCommand(
                @"UPDATE PasswordEntries
          SET WebsiteName = @w, Username = @u, EncryptedPassword = @p, IV = @iv
          WHERE Id = @id", con);
            cmd.Parameters.AddWithValue("@w", website);
            cmd.Parameters.AddWithValue("@u", username);
            cmd.Parameters.AddWithValue("@p", encryptedPassword);
            cmd.Parameters.AddWithValue("@iv", iv);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public List<PasswordEntry> SearchEntries(string term)
        {
            var list = new List<PasswordEntry>();
            using var con = new SqlConnection(_connectionString);
            con.Open();
            using var cmd = new SqlCommand(
                @"SELECT Id, WebsiteName, Username, EncryptedPassword, IV
          FROM PasswordEntries
          WHERE WebsiteName LIKE @t OR Username LIKE @t", con);
            cmd.Parameters.AddWithValue("@t", "%" + term + "%");
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new PasswordEntry
                {
                    Id = reader.GetInt32(0),
                    Website = reader.GetString(1),
                    Username = reader.GetString(2),
                    EncryptedPassword = reader.GetString(3),
                    IV = reader.GetString(4)
                });
            }
            return list;
        }
    }
}
