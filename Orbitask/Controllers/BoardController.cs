
using Microsoft.AspNetCore.Mvc;
using Orbitask.Models;
using Orbitask.Services.Interfaces;

namespace Orbitask.Controllers
{
    [ApiController]
    [Route("api")]
    public class BoardController : ControllerBase
    {
        private readonly IBoardService _boardService;

        public BoardController(IBoardService boardService)
        {
            _boardService = boardService;
        }

        [HttpGet("workbenches/{workspaceId:int}/boards")]
        public async Task<IActionResult> GetBoards(int workspaceId)
        {
            var boards = await _boardService.GetBoardsForWorkbench(workspaceId);
            return Ok(boards);
        }

        [HttpGet("boards/{boardId:int}")]
        public async Task<IActionResult> GetBoard(int boardId)
        {
            var board = await _boardService.GetBoard(boardId);
            if (board == null)
                return NotFound();

            return Ok(board);
        }

        [HttpPost("workbenches/{workspaceId:int}/boards")]
        public async Task<IActionResult> CreateBoard(int workspaceId, [FromBody] Board newBoard)
        {
            var board = await _boardService.CreateBoard(workspaceId, newBoard);
            if (board == null)
                return NotFound();

            return Ok(board);
        }

        [HttpPut("boards/{boardId:int}")]
        public async Task<IActionResult> UpdateBoard(int boardId, [FromBody] Board updated)
        {
            var board = await _boardService.UpdateBoard(boardId, updated);
            if (board == null)
                return NotFound();

            return Ok(board);
        }

        [HttpDelete("boards/{boardId:int}")]
        public async Task<IActionResult> DeleteBoard(int boardId)
        {
            var success = await _boardService.DeleteBoard(boardId);
            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
