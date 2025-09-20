using Microsoft.AspNetCore.Mvc;

namespace ProfanityService.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ProfanityController : ControllerBase
    {
        private Database database = Database.GetInstance();

        /// <summary>
        /// Loads profanities from JSON file into the database.
        /// </summary>
        [HttpPost("create-database")]
        public async Task<IActionResult> CreateDatabase()
        {
            try
            {
                await database.CreateDatabase();
                return Ok(new { message = "Profanity database created successfully." });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Deletes all words from the database.
        /// </summary>
        [HttpDelete("delete-database")]
        public async Task<IActionResult> DeleteDatabase()
        {
            try
            {
                await database.DeleteDatabase();
                return Ok(new { message = "Profanity database deleted successfully." });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        /// <summary>
        /// Checks if the given text contains any profanities.
        /// </summary>
        [HttpPost("check-text")]
        public async Task<IActionResult> CheckProfanities([FromBody] ProfanityRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Text))
                return BadRequest(new { error = "Text text is required." });
            var checker = new Profanity(database);
            var containsProfanity = await checker.ContainsProfanityAsync(request);

            return Ok(new
            {
                containsProfanity,
                message = containsProfanity ? "Text contains profanities." : "Text is clean."
            });
        }
    }
}
