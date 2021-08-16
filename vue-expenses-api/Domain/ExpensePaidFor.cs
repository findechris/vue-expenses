using System.Collections.Generic;
using vue_expenses_api.Infrastructure;

namespace vue_expenses_api.Domain
{
    public class ExpensePaidFor : ArchivableEntity
    {
        public ExpensePaidFor()
        {
        }

        public ExpensePaidFor(
            int value,
            Expense expense,
            ExpenseType expenseType)
        {
            Value = value;
            Expense = Expense;
            ExpenseType = expenseType;
        }

        public decimal Value { get; set; }
        public Expense Expense { get; set; }
        public ExpenseType ExpenseType { get; set; }
    }
}