using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using MediatR;
using vue_expenses_api.Dtos;
using vue_expenses_api.Infrastructure.Security;

namespace vue_expenses_api.Features.Users
{
    public class UsersList
    {
        public class Query : IRequest<List<UserDto>>
        {
        }

        public class QueryHandler : IRequestHandler<Query, List<UserDto>>
        {
            private readonly IDbConnection _dbConnection;
            private readonly ICurrentUser _currentUser;

            public QueryHandler(
                IDbConnection connection,
                ICurrentUser currentUser)
            {
                _dbConnection = connection;
                _currentUser = currentUser;
            }

            //Implement Paging
            public async Task<List<UserDto>> Handle(
                Query message,
                CancellationToken cancellationToken)
            {
                var sql = @"SELECT 
                                users.*
                            FROM 
                                users
                            WHERE 
                                users.Archived = 0";

                var users = await _dbConnection.QueryAsync<UserDto>(
                    sql);

                return users.ToList();
            }
        }
    }
}