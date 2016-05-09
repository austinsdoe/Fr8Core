﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthMonitorUtility
{
    /// <summary>
    /// An utility class simplifying the execution of SQL scripts for database-related metric retrieval. 
    /// </summary>
    public class DatabaseProvider : IDisposable
    {
        private SqlConnection _conn;
        private SqlCommand _comm;
        private SqlDataReader _reader;

        private DatabaseProvider() { }

        public string Database
        {
            get
            {
                return _conn.Database;
            }
        }
        public static DatabaseProvider Create(string scriptName, string connectionString)
        {
            var provider = new DatabaseProvider();
            provider.Init(scriptName, connectionString);
            return provider;
        }

        private void Init(string scriptName, string connectionString)
        {
            string rootPath = Utilities.MiscUtils.UpNLevels(Environment.CurrentDirectory, 2);
            string sqlFolder = Path.Combine(rootPath, "SQL");
            string sqlScript = Path.Combine(sqlFolder, scriptName);

            string commandText;

            if (File.Exists(sqlScript))
                commandText = File.ReadAllText(sqlScript);
            else
                throw new FileNotFoundException($"The SQL script is not found in this location: {sqlScript}");

            _conn = new SqlConnection(connectionString);
            _conn.Open();

            _comm = new SqlCommand(commandText, _conn);
            _comm.CommandTimeout = 300;
        }

        public SqlDataReader ExecuteReader()
        {
            _reader = _comm.ExecuteReader();
            return _reader;
        }

        public object ExecuteScalar()
        {
            var result = _comm.ExecuteScalar();
            return result;
        }

        public void Dispose()
        {
            if (_reader != null && !_reader.IsClosed)
                _reader.Close();

            if (_conn.State != ConnectionState.Closed)
                _conn.Close();
        }
    }
}