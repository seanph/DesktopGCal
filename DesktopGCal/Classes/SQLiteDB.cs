#region License and Summary
// Copyright (c) 2014 Sean Phillips
// Distributed under the BSD License (see LICENSE.md for full license text)
// 
// SQLiteDB.cs
//   Exposes a basic SQLite database class built on the System.Data.SQLite 
//   provider.
#endregion

using System;
using System.IO;
using System.Data;
using System.Data.SQLite;

namespace Seanph.Calendar.Helpers
{
    public class SqLiteDb
    {
        string _connstr;

        public SqLiteDb(string dbFilename)
        {
            if (!File.Exists(dbFilename))
            {
                FileStream f = File.Create(dbFilename);
                f.Close();
            }

            _connstr = string.Format("Data Source={0}", dbFilename);
        }

        /// <summary>
        ///     Performs an SQL query that returns a dataset (eg. SELECT)
        /// </summary>
        public DataTable DataQuery(string sqlCmd)
        {
            var dt = new DataTable();

            try
            {
                var con = new SQLiteConnection(_connstr);
                con.Open();

                var cmd = new SQLiteCommand(con);
                cmd.CommandText = sqlCmd;

                SQLiteDataReader reader = cmd.ExecuteReader();
                dt.Load(reader);
                reader.Close();

                con.Close();
            }
            catch (Exception e)
            {
                throw new Exception(e.Message);
            }

            return dt;
        }

        /// <summary>
        ///     Performs an SQL query that doesn't return a dataset (eg. UPDATE)
        /// </summary>
        public int NondataQuery(string sqlCmd)
        {
            var con = new SQLiteConnection(_connstr);
            con.Open();

            var cmd = new SQLiteCommand(con);
            cmd.CommandText = sqlCmd;

            int rowsUpdated = cmd.ExecuteNonQuery();
            con.Close();

            return rowsUpdated;
        }
    }
}