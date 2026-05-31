using GieudexPol.Application.Interfaces;
using GieudexPol.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GieudexPol.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WhaleRankingController : ControllerBase
    {
        private readonly IWhaleRankingService _whaleRankingService;

        public WhaleRankingController(IWhaleRankingService whaleRankingService)
        {
            _whaleRankingService = whaleRankingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var whaleRankings = await _whaleRankingService.GetAllAsync();
            return Ok(whaleRankings);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var whaleRanking = await _whaleRankingService.GetByIdAsync(id);
            return whaleRanking == null ? NotFound() : Ok(whaleRanking);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetByUserId(int userId)
        {
            var whaleRanking = await _whaleRankingService.GetByUserIdAsync(userId);
            return whaleRanking == null ? NotFound() : Ok(whaleRanking);
        }

        [HttpGet("top/{topN}")]
        public async Task<IActionResult> GetTopWhales(int topN)
        {
            var topWhales = await _whaleRankingService.GetTopWhalesAsync(topN);
            return Ok(topWhales);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshRanking()
        {
            await _whaleRankingService.RefreshRankingAsync();
            return Ok(new { success = true, message = "Ranking odświeżony pomyślnie." });
        }
    }
}