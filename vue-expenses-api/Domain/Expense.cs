using System;
using System.Collections.Generic;
using vue_expenses_api.Infrastructure;

namespace vue_expenses_api.Domain
{
    public enum PaymentTypes
    {
        Income,
        Expense
    }

    public class Expense : ArchivableEntity
    {
        protected Expense()
        {
        }

        public Expense(
            DateTime date,
            ExpenseCategory category,
            ExpenseType type,
            decimal value,
            string comments,
            User user)
        {
            Date = date;
            Category = category;
            Type = type;
            Value = value;
            Comments = comments;
            User = user;
            PaymentType = PaymentTypes.Expense;
        }

        public DateTime Date { get; set; }
        public ExpenseCategory Category { get; set; }
        public ExpenseType Type { get; set; }
        public PaymentTypes PaymentType { get; set; }
        public decimal Value { get; set; }
        public string Comments { get; set; }
        public string Hash { get; set; }
        public string Reference { get; set; }
        public string PaymentMethod { get; set; }
        public User User { get; set; }
        public List<ExpensePaidFor> PaidFor { get; set; }
    }
}