﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SqlClient;
using Sparta.Model;
using System.Data;

namespace Sparta.Dal
{
    public static class DALGebruiker
    {
        /**
        * door Joost
        * het controlleren van credentials met de persoon als return value
        **/
        public static Persoon checkCredentials(Login user)
        {
            Persoon persoon;
            int id = getPersoonId(user);
            //openen van sql connection
            SqlConnection conn = DALConnection.GetConnectionByName("Reader");
            conn.Open();

            //sql query klaarzetten
            string sql = "SELECT PersoonId,Naam,Achternaam,Categorie,GeboorteDatum " +
                "FROM dbo.Persoon WHERE PersoonId = @id";
            SqlCommand sqlcmd = new SqlCommand(sql, conn);
            SqlParameter idParam = new SqlParameter("@id", SqlDbType.Int);
            idParam.Value = id;
            sqlcmd.Parameters.Add(idParam);
            sqlcmd.Prepare();

            //query uitvoeren en resultaat lezen
            SqlDataReader reader = sqlcmd.ExecuteReader();
            if (reader.Read())
            {
                int persoonId = reader.GetInt32(0);
                string naam = reader.GetString(1);
                string achternaam = reader.GetString(2);
                DeelnemerCategorie categorie = (DeelnemerCategorie)reader.GetInt16(3);
                DateTime gebDatum = reader.GetDateTime(4);
                persoon = new Persoon(persoonId, naam, achternaam, gebDatum, categorie);
            }
            else
            {
                throw new Exception("Persoon not found");
            }
            reader.Close();
            conn.Close();
            return persoon;
        }

        /**
         * door Joost
        **/
        private static int getPersoonId(Login user)
        {
            int id = 0;
            //open connection
            SqlConnection conn = DALConnection.GetConnectionByName("Reader");
            conn.Open();

            //set up sql query
            string sql = "SELECT PersoonId FROM dbo.Login " +
                "WHERE AanmeldNaam = @naam AND pwdhash = @pwd";
            SqlCommand sqlcmd = new SqlCommand(sql, conn);

            //maken parameters
            SqlParameter naamParam = new SqlParameter("@naam", SqlDbType.NVarChar, 50);
            SqlParameter pwdParam = new SqlParameter("@pwd", SqlDbType.NChar, 32);

            //waarde geven aan parameters
            naamParam.Value = user.Naam;
            pwdParam.Value = user.Pwdhash;

            //parameters toevoegen
            sqlcmd.Parameters.Add(naamParam);
            sqlcmd.Parameters.Add(pwdParam);
            sqlcmd.Prepare();

            SqlDataReader reader = sqlcmd.ExecuteReader();
            if (reader.Read())
            {
                id = reader.GetInt32(0);
            }
            else
            {
                throw new Exception("Login not found");
            }

            reader.Close();
            conn.Close();
            return id;
        }

        /**
         * door Joost
         * registreren van persoon
        **/
        public static void RegistreerPersoon(DeelnemerCategorie categorie,
            string naam, string achternaam, DateTime gebdatum, Login aanmeld)
        {
            int persoonId = voegPersoonToe(categorie, naam, achternaam, gebdatum);
            voegLoginToe(persoonId, aanmeld);
        }
        /**
         * door Joost
         * toe voegen van persoon
        **/
        private static int voegPersoonToe(DeelnemerCategorie categorie,
            string naam, string achternaam, DateTime gebdatum)
        {
            int id = 0;
            SqlConnection conn = DALConnection.GetConnectionByName("Reader");
            conn.Open();

            string sql = "INSERT INTO dbo.Persoon " +
                "(Naam, Achternaam, Categorie, GeboorteDatum) output INSERTED.PersoonId VALUES" +
                "(@naam, @achternaam, @categorie, @geboorteDatum); ";
            SqlCommand sqlcmd = new SqlCommand(sql, conn);
            //maken parameters
            SqlParameter naamParam = new SqlParameter("@naam", SqlDbType.NVarChar, 50);
            SqlParameter achternaamParam = new SqlParameter("@achternaam", SqlDbType.NVarChar, 50);
            SqlParameter categorieParam = new SqlParameter("@categorie", SqlDbType.SmallInt);
            SqlParameter geboorteDatumParam = new SqlParameter("@geboorteDatum", SqlDbType.Date);

            //waarde geven aan parameters
            naamParam.Value = naam;
            achternaamParam.Value = achternaam;
            categorieParam.Value = (Int16)categorie;
            geboorteDatumParam.Value = gebdatum;

            //parameters toevoegen
            sqlcmd.Parameters.Add(naamParam);
            sqlcmd.Parameters.Add(achternaamParam);
            sqlcmd.Parameters.Add(categorieParam);
            sqlcmd.Parameters.Add(geboorteDatumParam);
            sqlcmd.Prepare();
            id = (int)sqlcmd.ExecuteScalar();

            conn.Close();
            return id;
        }
        /**
         * door Joost
         * toe voegen van login
        **/
        private static void voegLoginToe(int persoonId, Login aanmeld)
        {
            //open connection
            SqlConnection conn = DALConnection.GetConnectionByName("Reader");
            conn.Open();

            //sql query
            string sql = "INSERT INTO dbo.Login (AanmeldNaam, PwdHash, PersoonId) " +
                "VALUES(@naam, @pwd, @pId)";
            SqlCommand sqlcmd = new SqlCommand(sql, conn);

            //maken parameters
            SqlParameter naamParam = new SqlParameter("@naam", SqlDbType.NVarChar, 50);
            SqlParameter pwdParam = new SqlParameter("@pwd", SqlDbType.NChar, 32);
            SqlParameter pIdParam = new SqlParameter("@pId", SqlDbType.Int);

            //waarde geven aan parameters
            naamParam.Value = aanmeld.Naam;
            pwdParam.Value = aanmeld.Pwdhash;
            pIdParam.Value = persoonId;

            //parameters toevoegen
            sqlcmd.Parameters.Add(naamParam);
            sqlcmd.Parameters.Add(pwdParam);
            sqlcmd.Parameters.Add(pIdParam);
            sqlcmd.Prepare();

            sqlcmd.ExecuteNonQuery();

            conn.Close();
        }

        // Door: Davut Demir
        // Update de password van een gebruiker
        public static void UpdatePwd(int loginid, string pwdhash)
        {
            // opent een writer connection met de database
            SqlConnection sqlConn = DALConnection.GetConnectionByName("Writer");
            sqlConn.Open();

            // de query die de password van de gebruiker update
            string updateQuery = "UPDATE dbo.Login SET PwdHash = @hash WHERE LoginId = @id";

            // zet de query klaar voor gebruik
            SqlCommand sqlCmnd = new SqlCommand(updateQuery, sqlConn);

            // leggen de informatie van de loginId en de nieuwe password hash in de query
            // met gebruik van parameters
            SqlParameter paramHash = new SqlParameter("@hash", SqlDbType.NVarChar, 32);
            SqlParameter paramId = new SqlParameter("@id", SqlDbType.Int);

            // hier wordt de parameters gevult met informatie
            paramHash.Value = pwdhash;
            paramId.Value = loginid;

            // hier worden de parameters toegevoegd
            sqlCmnd.Parameters.Add(paramHash);
            sqlCmnd.Parameters.Add(paramId);
            sqlCmnd.Prepare();

            // hier wordt de query uitgevoerd op de database
            sqlCmnd.ExecuteNonQuery();

            sqlConn.Close();
        }


        // Door: Davut Demir
        // haalt de loginId van de database en returnt deze value
        public static int GetLoginId(int persoonid, string pwdhash)
        {
            // opent een reader connection met de database
            SqlConnection sqlConn = DALConnection.GetConnectionByName("Reader");
            sqlConn.Open();

            // de query die de informatie vind van de LoginId
            string selectQuery = "SELECT LoginId FROM dbo.Login WHERE PersoonId = @id AND PwdHash = @hash";

            // zet de query klaar voor gebruik
            SqlCommand sqlCmnd = new SqlCommand(selectQuery, sqlConn);

            // leggen de informatie van de PersoonId en de password hash in de query
            // met gebruik van parameters
            SqlParameter paramId = new SqlParameter("@id", SqlDbType.Int);
            SqlParameter paramHash = new SqlParameter("@hash", SqlDbType.NVarChar, 32);

            // hier wordt de parameters gevult met informatie
            paramHash.Value = pwdhash;
            paramId.Value = persoonid;

            // hier worden de parameters toegevoegd
            sqlCmnd.Parameters.Add(paramId);
            sqlCmnd.Parameters.Add(paramHash);
            sqlCmnd.Prepare();

            // hier wordt de query uitgevoerd op de database
            SqlDataReader dataReader = sqlCmnd.ExecuteReader();

            // we lezen 1 lijn, waar de informatie staat over de LoginId, en daarna slaan we deze op in een variabel.
            dataReader.Read();
            int loginid = dataReader.GetInt32(0);
            sqlConn.Close();

            return loginid;
        }


        // Door: Juan Albergen
        public static void voegtoeContactInfo(Contact info)
        {
            //Initialiseren van een DB connectie
            SqlConnection connection = DALConnection.GetConnectionByName("Writer");
            connection.Open();

            //Preparen van query
            SqlParameter contactInfoldParam = new SqlParameter("@1", SqlDbType.Int);
            SqlParameter persoonIdParam = new SqlParameter("@2", SqlDbType.Int);
            SqlParameter straatParam = new SqlParameter("@3", SqlDbType.NVarChar, 50);
            SqlParameter huisnummerParam = new SqlParameter("@4", SqlDbType.Int);
            SqlParameter huisnummertoevoegingParam = new SqlParameter("@5", SqlDbType.NVarChar, 10);
            SqlParameter plaatsParam = new SqlParameter("@6", SqlDbType.NVarChar, 50);
            SqlParameter postcodeParam = new SqlParameter("@7", SqlDbType.NChar, 6);
            SqlParameter emailParam = new SqlParameter("@8", SqlDbType.NVarChar, 50);
            SqlParameter telefoonParam = new SqlParameter("@9", SqlDbType.NVarChar, 20);

            //Opzetten query
            string sqlContactInfo = "INSERT INTO ContactInfo (PersoonId, Straat, Huisnummer, Huisnummertoevoeging, Plaats, Postcode, Email, Telefoon)" +
                                    " VALUES (@2, @3, @4, @5, @6, @7, @8, @9);";

            SqlCommand command = new SqlCommand(sqlContactInfo, connection);

            command.Parameters.Add(contactInfoldParam);
            command.Parameters.Add(persoonIdParam);
            command.Parameters.Add(straatParam);
            command.Parameters.Add(huisnummerParam);
            command.Parameters.Add(huisnummertoevoegingParam);
            command.Parameters.Add(plaatsParam);
            command.Parameters.Add(postcodeParam);
            command.Parameters.Add(emailParam);
            command.Parameters.Add(telefoonParam);

            contactInfoldParam.Value = info.Id;
            persoonIdParam.Value = info.Persoonid;
            straatParam.Value = info.Straat;
            huisnummerParam.Value = info.Huisnummer;
            huisnummertoevoegingParam.Value = info.Huisnummertoevoeging;
            plaatsParam.Value = info.Plaats;
            postcodeParam.Value = info.Postcode;
            emailParam.Value = info.Email;
            telefoonParam.Value = info.Telefoon;

            command.Prepare();
            command.ExecuteNonQuery();

            connection.Close();
        }


        // Door: Juan Albergen

        public static void vernieuwContactInfo(Contact info)
        {

            //Initialiseren van een DB connectie
            SqlConnection connection = DALConnection.GetConnectionByName("Writer");
            connection.Open();

            //Preparen van query
            SqlParameter contactInfoldParam = new SqlParameter("@1", SqlDbType.Int);
            SqlParameter persoonIdParam = new SqlParameter("@2", SqlDbType.Int);
            SqlParameter straatParam = new SqlParameter("@3", SqlDbType.NVarChar, 50);
            SqlParameter huisnummerParam = new SqlParameter("@4", SqlDbType.Int);
            SqlParameter huisnummertoevoegingParam = new SqlParameter("@5", SqlDbType.NVarChar, 10);
            SqlParameter plaatsParam = new SqlParameter("@6", SqlDbType.NVarChar, 50);
            SqlParameter postcodeParam = new SqlParameter("@7", SqlDbType.NChar, 6);
            SqlParameter emailParam = new SqlParameter("@8", SqlDbType.NVarChar, 50);
            SqlParameter telefoonParam = new SqlParameter("@9", SqlDbType.NVarChar, 20);

            //Opzetten query
            string sqlContactInfo = "UPDATE ContactInfo" +
                                    " SET ContactInfold = @1 PersoonId = @2, Straat = @3, Huisnummer = @4 , Huisnummertoevoeging = @5, Plaats = @6, Postcode = @7, Email = @8, Telefoon = @9" +
                                    " WHERE PersoonId =  10";



            SqlCommand command = new SqlCommand(sqlContactInfo, connection);

            contactInfoldParam.Value = info.Id;
            persoonIdParam.Value = info.Persoonid;
            straatParam.Value = info.Straat;
            huisnummerParam.Value = info.Huisnummer;
            huisnummertoevoegingParam.Value = info.Huisnummertoevoeging;
            plaatsParam.Value = info.Plaats;
            postcodeParam.Value = info.Postcode;
            emailParam.Value = info.Email;
            telefoonParam.Value = info.Telefoon;

            command.Parameters.Add(contactInfoldParam);
            command.Parameters.Add(persoonIdParam);
            command.Parameters.Add(straatParam);
            command.Parameters.Add(huisnummerParam);
            command.Parameters.Add(huisnummertoevoegingParam);
            command.Parameters.Add(plaatsParam);
            command.Parameters.Add(postcodeParam);
            command.Parameters.Add(emailParam);
            command.Parameters.Add(telefoonParam);

            command.Prepare();
            command.ExecuteNonQuery();

            connection.Close();

        }

        //Dit is een dummy methode, er onstond een error die vroeg naar deze methode in de logic laag. Bestond niet. Door deze methode hebben we het wel kunnnen testen. 
        //Er wordt dus ook gebruik gemaakt van een PersoonId die niet veranderd.
        public static Contact GetContactInfoByPersoonId(Int32 persoonid)
        {
            Contact c = new Contact();
            c.Persoonid = 10;
            return c;
        }
    }
}
