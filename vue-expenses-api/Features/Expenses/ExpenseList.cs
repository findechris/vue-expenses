using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Dapper;
using MediatR;
using vue_expenses_api.Dtos;
using vue_expenses_api.Infrastructure;
using vue_expenses_api.Infrastructure.Security;

namespace vue_expenses_api.Features.Expenses
{
    public class ExpenseList
    {
        public class Query : IRequest<List<ExpenseDto>>
        {
            public int? Year { get; }

            public Query(
                int? year = null)
            {
                Year = year;
            }
        }

        public class QueryHandler : IRequestHandler<Query, List<ExpenseDto>>
        {
            private readonly IDbConnection _dbConnection;
            private readonly ExpensesContext _context;
            private readonly ICurrentUser _currentUser;

            public QueryHandler(
                ExpensesContext db,
                IDbConnection connection,
                ICurrentUser currentUser
                )
            {
                _dbConnection = connection;
                _currentUser = currentUser;
                _context = db;
            }

            //Implement Paging
            public async Task<List<ExpenseDto>> Handle(
                Query message,
                CancellationToken cancellationToken)
            {
                var yearCriteria = message.Year.HasValue
                    ? $" AND STRFTIME('%Y', e.Date) = '{message.Year}'"
                    : string.Empty;
                var sql = $@"SELECT 
                                e.Id,
                                STRFTIME('%Y-%m-%d', e.Date) AS Date,
                                STRFTIME('%m', e.Date) AS Month,
                                ec.Name AS Category,
                                ec.Id AS CategoryId,
                                ec.Budget AS CategoryBudget,
                                ec.ColourHex AS CategoryColour,
                                et.Name AS Type,
                                et.Id AS TypeId,
                                e.Value,
                                e.Comments
                            FROM 
                                Expenses e
                            INNER JOIN
                                ExpenseCategories ec ON ec.Id = e.CategoryId AND ec.Archived = 0
                            INNER JOIN
                                ExpenseTypes et ON et.Id = e.TypeId AND et.Archived = 0
                            INNER JOIN
                                Users u ON u.Id = e.UserId
                            WHERE 
                                u.Email=@userEmailId 
                                AND e.Archived = 0
                                {yearCriteria}";
                var expenses = await _dbConnection.QueryAsync<ExpenseDto>(
                    sql,
                    new
                    {
                        userEmailId = _currentUser.EmailId
                    }
                );
                var exList = expenses.ToList();
                foreach (var expense in exList)
                {
                    var ex = await _context.Expenses.Include("PaidFor.ExpenseType").FirstOrDefaultAsync(m => m.Id == expense.Id, cancellationToken);
                    var pf = ex.PaidFor.ToList();
                    if (pf.Count > 0)
                    {
                        expense.PaidFor = pf.Select(m =>
                        {
                            var etd = new ExpenseTypeDto(m.Id, m.ExpenseType.Name, m.ExpenseType.Description);
                            etd.Value = m.Value;
                            return etd;
                        }).ToList();
                    }
                }
                return exList;
            }
        }
    }
}