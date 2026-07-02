using AiCodeReviewAgent.Application.Reviews;
using Microsoft.AspNetCore.Mvc;

namespace AiCodeReviewAgent.Api.Controllers
{
    [ApiController]
    [Route("api/reviews")]
    public sealed class ReviewsController : ControllerBase
    {
        private readonly ICodeReviewService _codeReviewService;

        public ReviewsController(ICodeReviewService codeReviewService)
        {
            _codeReviewService = codeReviewService;
        }

        [HttpPost("analyze")]
        public async Task<ActionResult<AnalyzeCodeResponse>> Analyze(
            [FromBody] AnalyzeCodeRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _codeReviewService.AnalyzeAsync(request, cancellationToken);
            return Ok(result);
        }
    }
}
