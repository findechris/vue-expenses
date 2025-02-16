﻿using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using vue_expenses_api.Domain;
using vue_expenses_api.Dtos;
using vue_expenses_api.Infrastructure;
using vue_expenses_api.Infrastructure.Security;

namespace vue_expenses_api.Features.Expenses
{
    public class CreateExpense
    {
        public class Command : IRequest<ExpenseDto>
        {
            public Command(
                DateTime date,
                int categoryId,
                int typeId,
                decimal value,
                string comments)
            {
                Date = date;
                CategoryId = categoryId;
                TypeId = typeId;
                Value = value;
                Comments = comments;
            }

            public DateTime Date { get; set; }
            public int CategoryId { get; set; }
            public int TypeId { get; set; }
            public decimal Value { get; set; }
            public string Comments { get; set; }
        }

        public class CommandValidator : AbstractValidator<Command>
        {
            public CommandValidator()
            {
                RuleFor(x => x.Date).NotNull().NotEmpty();
                RuleFor(x => x.CategoryId).NotNull().NotEmpty();
                RuleFor(x => x.TypeId).NotNull().NotEmpty();
                RuleFor(x => x.Value).NotNull().NotEmpty();
            }
        }

        public class Handler : IRequestHandler<Command, ExpenseDto>
        {
            private readonly ExpensesContext _context;
            private readonly ICurrentUser _currentUser;

            public Handler(
                ExpensesContext db,
                ICurrentUser currentUser)
            {
                _context = db;
                _currentUser = currentUser;
            }

            public async Task<ExpenseDto> Handle(
                Command request,
                CancellationToken cancellationToken)
            {
                var user = await _context.Users.SingleAsync(x => x.Email == _currentUser.EmailId);

                var expenseCategory = await _context.ExpenseCategories.SingleAsync(
                        x => x.Id == request.CategoryId,
                        cancellationToken);

                var expenseType = await _context.ExpenseTypes.SingleAsync(
                        x => x.Id == request.TypeId,
                        cancellationToken);

                var expense = new Expense(
                    request.Date,
                    expenseCategory,
                    _context.ExpenseTypes.SingleOrDefaultAsync(
                        x => x.Id == request.TypeId,
                        cancellationToken).Result,
                    request.Value,
                    request.Comments,
                    user);
                expense.PaymentMethod = "Cash";
                expense.PaymentType = PaymentTypes.Expense;
                expense.Reference = expenseType.Name;

                // PaidFor ToDo
                var fieldVal = "";
                if (fieldVal == "")
                {
                    fieldVal = "Bea:Chris";
                }
                var splited = fieldVal.Split(':');
                var pList = new List<Domain.ExpensePaidFor>();
                foreach (var split in splited)
                {
                    var et = await _context.ExpenseTypes.SingleOrDefaultAsync(m => m.Name == split);
                    var pfor = new ExpensePaidFor();
                    pfor.ExpenseType = et;
                    pfor.Value = (expense.Value / splited.Length);
                    if (et != null)
                    {
                        pList.Add(pfor);
                    }
                }
                expense.PaidFor = pList;

                await _context.Expenses.AddAsync(
                    expense,
                    cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);

                return new ExpenseDto(
                    expense.Id,
                    expense.Date.ToString("yyyy-MM-dd"),
                    expenseCategory.Id,
                    expenseCategory.Name,
                    expenseCategory.Budget,
                    expenseCategory.ColourHex,
                    expenseType.Id,
                    expenseType.Name,
                    expense.Value,
                    expense.Comments);
            }
        }
    }
}