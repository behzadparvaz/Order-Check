using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System;
using System.Data;
using TapsiDOC.Order.Core.Domain.Orders.CommonContract;
using TapsiDOC.Order.Core.Domain.Orders.Repositories;

namespace TapsiDOC.Order.Infra.Data.Sql.Queries.Outbox
{
    public class OutboxQueryRepository : IOutboxQueryRepository
    {
        private readonly IConfiguration _config;
        private const string _sqlOutBox = "SqlOutBox";

        public OutboxQueryRepository(IConfiguration config) 
        {
            _config = config;
        }
        
        public async Task<List<OutBoxEvent>> GetOutBoxItemsByAggregate(string aggregate)
        {
            IDbConnection _connection = new SqlConnection(_config.GetConnectionString(_sqlOutBox));
            var orders = await _connection.QueryAsync<OutBoxEvent>($"select * from [dbo].[OutBoxEventItems] where Aggregate = '{aggregate}' and IsProcessed = 0 and StatusCode = 100");

            return orders.ToList();
        }
    }
}
