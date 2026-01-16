using Microsoft.AspNetCore.Mvc;
using Orbitask.Services.Interfaces;
using Orbitask.Models;

namespace Orbitask.Controllers
{
    [ApiController]
    [Route("api")]
    public class ColumnController : ControllerBase
    {
        private readonly IColumnService _columnService;

        public ColumnController(IColumnService columnService)
        {
            _columnService = columnService;
        }

        [HttpGet]
        [Route("boards/{boardId:int}/columns")]
        public async Task<IActionResult> GetColumns(int boardId)
        {
            var columns = await _columnService.GetColumnsForBoard(boardId);
            return Ok(columns);
        }

        [HttpGet]
        [Route("columns/{columnId:int}")]
        public async Task<IActionResult> GetColumn(int columnId)
        {
            var column = await _columnService.GetColumn(columnId);

            if (column == null)
                return NotFound();

            return Ok(column);
        }


        [HttpPost]
        [Route("boards/{boardId:int}/columns")]
        public async Task<IActionResult> CreateColumn(int boardId, [FromBody] Column newColumn)
        {
            var column = await _columnService.CreateColumn(boardId, newColumn);
            if (column == null)
                return NotFound();

            return Ok(column);
        }

        [HttpPut]
        [Route("columns/{columnId:int}")]
        public async Task<IActionResult> UpdateColumn(int columnId, [FromBody] Column updated)
        {
            var column = await _columnService.UpdateColumn(columnId, updated);
            if (column == null)
                return NotFound();

            return Ok(column);
        }

        [HttpDelete]
        [Route("columns/{columnId:int}")]
        public async Task<IActionResult> DeleteColumn(int columnId)
        {
            var success = await _columnService.DeleteColumn(columnId);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}

