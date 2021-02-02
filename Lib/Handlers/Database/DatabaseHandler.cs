﻿using System;
using Lib.ChainOfResponsibilityPattern;
using Lib.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Smo;
using SmoDatabase = Microsoft.SqlServer.Management.Smo.Database;

namespace Lib.Handlers.Database
{
    public class DatabaseHandler : AbstractRequestHandler, IDatabaseHandler
    {
        private readonly ILogger _logger;

        public DatabaseHandler(ILogger logger)
        {
            _logger = logger;
        }

        public override void Handle(Request request)
        {
            RemoveDatabase(request.ServerName, request.DatabaseName);
            base.Handle(request);
        }

        public void RemoveDatabase(string serverName, string databaseName)
        {
            try
            {
                var server = new Server(serverName);
                server.KillAllProcesses(databaseName);
                var database = new SmoDatabase(server, databaseName);
                database.Refresh();
                database.DropIfExists();
                LogDatabaseRemoved(serverName, databaseName);
            }
            catch (Exception ex)
            {
                LogDatabaseException(ex);
            }
        }

        private void LogDatabaseRemoved(string serverName, string databaseName)
        {
            _logger.LogInformation("Database [{serverName}].[{databaseName}] was removed", serverName, databaseName);
        }

        private void LogDatabaseException(Exception ex)
        {
            _logger.LogError("An error has occurred on [{name}] {ex}", Name, ex);
        }
    }
}