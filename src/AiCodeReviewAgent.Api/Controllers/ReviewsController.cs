using System.Text.Json;
using AiCodeReviewAgent.Application.Reviews;
using Microsoft.AspNetCore.Mvc;

namespace AiCodeReviewAgent.Api.Controllers
{
    [ApiController]
    [Route("api/reviews")]
    public sealed class ReviewsController : ControllerBase
    {
        private readonly ICodeReviewService _codeReviewService;
        private readonly IAiCodeReviewClient _aiCodeReviewClient;

        public ReviewsController(
            ICodeReviewService codeReviewService,
            IAiCodeReviewClient aiCodeReviewClient)
        {
            _codeReviewService = codeReviewService;
            _aiCodeReviewClient = aiCodeReviewClient;
        }

        [HttpPost("analyze")]
        public async Task<ActionResult<AnalyzeCodeResponse>> Analyze(
            [FromBody] AnalyzeCodeRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _codeReviewService.AnalyzeAsync(request, cancellationToken);
            return Ok(result);
        }
        [HttpPost("analyze-ai")]
        public async Task<ActionResult<object>> AnalyzeWithAi(
            [FromBody] AnalyzeCodeRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _aiCodeReviewClient.AnalyzeCodeAsync(request, cancellationToken);
            Console.WriteLine($"AI Review Result: test PR review");
            Console.WriteLine($"AI Review Result: test PR review");
            Console.WriteLine($"AI Review Result: test PR review");

            return Ok(new
            {
                fileName = request.FileName,
                aiReview = result
            });
        }
    }
}
