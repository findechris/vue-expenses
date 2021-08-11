using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using vue_expenses_api.Domain;
using Microsoft.AspNetCore.Http;
using vue_expenses_api.Infrastructure;
using Microsoft.EntityFrameworkCore;
using CsvHelper;
using System.IO;
using System.Globalization;
using System.Linq;
using CsvHelper.Configuration;
using vue_expenses_api.Infrastructure.Security;
using System.Collections.Generic;
using Serilog;

namespace vue_expenses_api.Features.Expenses
{
    public class ImportExpense
    {
        public class Command : IRequest
        {
            public Command(
                IList<IFormFile> Upload)
            {
                this.Upload = Upload;
            }

            public IList<IFormFile> Upload { get; set; }
        }

        public class CommandValidator : AbstractValidator<ImportExpense.Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Upload).NotNull().NotEmpty();
            }
        }

        public sealed class ExpenseMap : ClassMap<Expense>
        {
            public ExpenseMap(Domain.User user, ExpensesContext context)
            {
                Map(m => m.Date).Name("BookgDt");
                Map(m => m.Value).Name("Amt");
                Map(m => m.Hash).Optional().Name("Id");
                Map(m => m.PaymentMethod).Name("BookgTxt");
                Map(m => m.PaymentType).Convert(row =>
                {
                    var fieldVal = row.Row.GetField("CdtDbtInd");
                    if (fieldVal == "CRDT")
                    {
                        return Domain.PaymentTypes.Expense;
                    }
                    return Domain.PaymentTypes.Income;
                });
                Map(m => m.Category).Convert((row) =>
                {
                    var fieldVal = row.Row.GetField("Category");
                    if (fieldVal == "")
                    {
                        fieldVal = "Sonstige";
                    }
                    var ec = context.ExpenseCategories.FirstOrDefault(m => m.Name == fieldVal);
                    if (ec == null)
                    {
                        var random = new Random();
                        var color = String.Format("#{0:X6}", random.Next(0x1000000)); // = "#A197B9"
                        ec = new ExpenseCategory(
                            fieldVal,
                            "",
                            0,
                            color,
                            user
                        );
                        context.ExpenseCategories.Add(
                            ec
                        );
                        context.SaveChanges();
                    }
                    return ec;
                });
                Map(m => m.Comments).Name("RmtInf");
                Map(m => m.Reference).Name("RmtdNm");
                Map(m => m.User).Optional().Default(user);
                Map(m => m.Type).Optional().Convert(row =>
                {
                    var fieldVal = row.Row.GetField("PayedBy");
                    if (fieldVal == null)
                    {
                        fieldVal = "1822";
                    }
                    var et = context.ExpenseTypes.FirstOrDefault(m => m.Name == fieldVal);
                    return et;
                });
                Map(m => m.PaidFor).Optional().Convert(row =>
                {
                    var fieldVal = row.Row.GetField("PayedFor");
                    if (fieldVal == null)
                    {
                        fieldVal = "Bea,Chris";
                    }
                    var splited = fieldVal.Split(',');
                    var pList = new List<Domain.ExpenseType>();
                    foreach (var split in splited)
                    {
                        var et = context.ExpenseTypes.FirstOrDefault(m => m.Name == split);
                        if (et != null)
                        {
                            pList.Add(et);
                        }
                    }
                    return pList;
                });
            }
        }

        public class Handler : IRequestHandler<Command, Unit>
        {
            private readonly ExpensesContext _context;
            private readonly ICurrentUser _currentUser;

            public Handler(
                ExpensesContext db,
                ICurrentUser currentUser
            )
            {
                _context = db;
                _currentUser = currentUser;
            }

            public async Task<Unit> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                if (request.Upload.Count > 0)
                {
                    var user = await _context.Users.SingleAsync(x => x.Email == _currentUser.EmailId,
                    cancellationToken);
                    var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture);
                    csvConfig.IgnoreBlankLines = true;
                    csvConfig.MissingFieldFound = null;
                    // csv.Configuration.HeaderValidated = false;
                    try
                    {
                        using (var reader = new StreamReader(request.Upload[0].OpenReadStream()))
                        using (var csv = new CsvReader(reader, csvConfig))
                        {
                            var expenseMap = new ExpenseMap(user, this._context);
                            csv.Context.RegisterClassMap(expenseMap);
                            var records = csv.GetRecords<Expense>();
                            foreach (var record in records)
                            {
                                var et = this._context.Expenses.FirstOrDefault(m => m.Hash == record.Hash);
                                if (et == null)
                                {
                                    this._context.Expenses.Add(record);
                                }
                                this._context.SaveChanges();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, ex.Message);
                        throw ex;
                    }
                }
                return Unit.Value;
            }
        }
    }
}