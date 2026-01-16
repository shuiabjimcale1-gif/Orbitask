using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Orbitask.Models;
using Orbitask.Services.Interfaces;

namespace Orbitask.Controllers
{
    [Route("api")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ITagService tagService;

        public TagController(ITagService tagService)
        {
            this.tagService = tagService;
        }

        [HttpPost]
        [Route("boards/{boardId:int}/tags")]
        public async Task<IActionResult> CreateTag(int boardId, [FromBody] Tag newTag)
        {
            var tag = await tagService.CreateTag(boardId, newTag);

            if (tag == null)
                return NotFound(); // board doesn't exist

            return Ok(tag);
        }

        [HttpGet]
        [Route("tags/{tagId:int}")]
        public async Task<IActionResult> GetTag(int tagId)
        {
            var tag = await tagService.GetTag(tagId);

            if (tag == null)
                return NotFound();

            return Ok(tag);
        }

        [HttpPut]
        [Route("tags/{tagId:int}")]
        public async Task<IActionResult> UpdateTag(int tagId, [FromBody] Tag updated)
        {
            var tag = await tagService.UpdateTag(tagId, updated);

            if (tag == null)
                return NotFound();

            return Ok(tag);
        }

        [HttpDelete]
        [Route("tags/{tagId:int}")]
        public async Task<IActionResult> DeleteTag(int tagId)
        {
            var success = await tagService.DeleteTag(tagId);

            if (!success)
                return NotFound();

            return NoContent();
        }
    }
}
