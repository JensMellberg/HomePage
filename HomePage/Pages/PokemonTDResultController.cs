using HomePage.Model;
using HomePage.Repositories;
using Microsoft.AspNetCore.Mvc;
using PokemonTDEngine;

namespace HomePage.Pages
{
    [ApiController]
    [Route("PokemonTD/Results")]
    public class PokemonTDResultController(SignInRepository signInRepository, PokemonTDResultRepository resultRepository, DatabaseLogger logger) : ControllerBase
    {
        [HttpPost("Get")]
        public IActionResult GetResults([FromBody] ExternalAccessDetails accessDetails)
        {
            if (!signInRepository.VerifyAuthCookie(accessDetails.username, accessDetails.accessToken))
            {
                return Unauthorized();
            }

            var results = resultRepository.GetResults(accessDetails.username);
            return new JsonResult(new
            {
                results = results.Select(x => new
                {
                    difficulty = x.Key.ToString(),
                    isWin = x.Value.IsWin,
                    levelCompleted = x.Value.LevelCompleted,
                    damageTestResult = x.Value.DamageTestResult
                }).ToArray()
            });
        }

        [HttpPost("Store")]
        public IActionResult StoreResult([FromBody] StoreResultRequestObject requestDetails)
        {
            if (!signInRepository.VerifyAuthCookie(requestDetails.username, requestDetails.accessToken))
            {
                return Unauthorized();
            }

            List<Event> deseralizedEvents;
            try
            {
                deseralizedEvents = EventTracker.DeserializeEvents(requestDetails.serializedEvents);
            }
            catch (Exception e)
            {
                logger.Log(LogRowSeverity.Error, $"Error when deserializing PokemonTD events {e.Message}", requestDetails.username, e.StackTrace);
                return BadRequest();
            }

            if (!Enum.TryParse<Difficulty>(requestDetails.difficulty, out var difficulty))
            {
                logger.Error($"Invalid PokemonTD difficulty {requestDetails.difficulty}.", requestDetails.username);
                return BadRequest();
            }

            var updatedResult = resultRepository.AddResult(requestDetails.username, difficulty, deseralizedEvents);
            return updatedResult ? Ok() : new OkObjectResult("Failed to validate result.");
        }

        public class StoreResultRequestObject
        {
            public string username { get; set; }
            public Guid accessToken { get; set; }
            public string serializedEvents { get; set; }
            public string difficulty { get; set; }
        }
    }
}
