using System.Collections.Generic;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using vue_expenses_api.Dtos;

namespace vue_expenses_api.Features.Expenses
{
    [Route("expenses")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ExpensesController : Controller
    {
        private readonly IMediator _mediator;

        public ExpensesController(
            IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("list/{offset}/{rowcount}/{sortBy}/{sortDesc}")]
        public async Task<ExpenseListDto> Get(int offset, int rowcount, string sortBy, bool sortDesc)
        {
            return await _mediator.Send(new ExpenseList.Query(null, offset, rowcount, sortBy, sortDesc));
        }

        [HttpGet]
        [Route("getbyyear/{year}/{offset}/{rowcount}/{sortBy}/{sortDesc}")]
        public async Task<ExpenseListDto> GetByYear(int year, int offset, int rowcount, string sortBy, bool sortDesc)
        {
            return await _mediator.Send(new ExpenseList.Query(year, offset, rowcount, sortBy, sortDesc));
        }

        [HttpGet("{id}")]
        public async Task<ExpenseDto> Get(
            int id)
        {
            return await _mediator.Send(new ExpenseDetails.Query(id));
        }

        [HttpPost]
        public async Task<ExpenseDto> Create(
            [FromBody] CreateExpense.Command command)
        {
            return await _mediator.Send(command);
        }

        [HttpPost]
        [Route("import")]
        public async Task Import(
            IList<IFormFile> file)
        {
            await _mediator.Send(new ImportExpense.Command(file));
        }

        [HttpPut("{id}")]
        public async Task<ExpenseDto> Update(
            int? id,
            [FromBody] UpdateExpense.Command command)
        {
            return await _mediator.Send(command);
        }

        [HttpDelete("{id}")]
        public async Task Delete(
            int id)
        {
            await _mediator.Send(new DeleteExpense.Command(id));
        }
    }
}