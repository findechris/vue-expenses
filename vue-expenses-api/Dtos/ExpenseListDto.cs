using System;
using System.Collections.Generic;

namespace vue_expenses_api.Dtos
{
    public class ExpenseListDto
    {
        protected ExpenseListDto()
        {
        }

        public ExpenseListDto(
            List<ExpenseDto> expenseDtos,
            int totalItems
            )
        {
            Items = expenseDtos;
            TotalItems = totalItems;
        }

        public List<ExpenseDto> Items { get; set; }
        public int TotalItems { get; set; }

    }
}