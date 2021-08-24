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
        public class Query : IRequest<ExpenseListDto>
        {
            public int? Year { get; }
            public int? Offset { get; }
            public int? Rowcount { get; }
            public string SortBy { get; }
            public bool? SortDesc { get; }

            public Query(
                int? year = null, int? offset = 0, int? rowcount = 10, string sortBy = "id", bool? sortDesc = true)
            {
                Year = year;
                Offset = offset;
                Rowcount = rowcount;
                SortBy = sortBy;
                SortDesc = sortDesc;
            }
        }

        public class QueryHandler : IRequestHandler<Query, ExpenseListDto>
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
            public async Task<ExpenseListDto> Handle(
                Query message,
                CancellationToken cancellationToken)
            {
                var sorting = message.SortDesc.HasValue && message.SortDesc.Value ? "DESC" : "ASC";
                var yearCriteria = message.Year.HasValue
                    ? $" AND STRFTIME('%Y', e.Date) = '{message.Year}'"
                    : string.Empty;
                var sql = $@"SELECT 
                                e.Id as id,
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
                                AND e.PaymentType = 0
                                {yearCriteria}
                            ORDER BY {message.SortBy} {sorting}
                            LIMIT {message.Offset}, {message.Rowcount}
                            ;
                            
                            SELECT COUNT(*)
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
                                AND e.PaymentType = 0
                                {yearCriteria}
                            ";
                var multi = await _dbConnection.QueryMultipleAsync(
                    sql,
                    new
                    {
                        userEmailId = _currentUser.EmailId
                    }
                );
                var expenses = multi.Read<ExpenseDto>();
                var totalCount = multi.ReadFirst<int>();
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
                return new ExpenseListDto(exList, totalCount);
            }
        }
    }
}