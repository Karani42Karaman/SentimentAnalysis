using MediatR;
using Microsoft.AspNetCore.Mvc;
using SentimentAnalysis.Application.DTOs;
using SentimentAnalysis.Application.Features.Analysis;
using SentimentAnalysis.Application.Features.Training;
using SentimentAnalysis.Domain.Interfaces;

namespace SentimentAnalysis.API.Controllers;

[ApiController]
public class SentimentController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IFileService _fileService;

    public SentimentController(IMediator mediator, IFileService fileService)
    {
        _mediator = mediator;
        _fileService = fileService;
    }

    // 1. Endpoint: Veri Seti Yükleme
    [HttpPost("corpus/upload")]
    public async Task<IActionResult> UploadCorpus(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest("Lütfen geçerli bir CSV dosyası yükleyin.");

        var filePath = await _fileService.SaveFileAsync(file.OpenReadStream(), file.FileName);

        return Ok(new
        {
            isSuccess = true,
            message = "Dosya başarıyla yüklendi. Modeli eğitirken lütfen bu dosya yolunu kullanın.",
            corpusPath = filePath
        });
    }

    // 2. Endpoint: Modeli Eğitme
    [HttpPost("model/train")]
    public async Task<IActionResult> TrainModel([FromQuery] string corpusPath, [FromBody] TrainRequestDto dto)
    {
        var command = new TrainModelCommand(dto, corpusPath);
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }

    // 3. Endpoint: Analiz Etme ve Olasılıkları Alma
    [HttpPost("analyze")]
    public async Task<IActionResult> AnalyzeText([FromBody] AnalyzeRequestDto dto)
    {
        var query = new AnalyzeTextQuery(dto);
        var result = await _mediator.Send(query);

        if (!result.IsSuccess)
            return BadRequest(result);

        return Ok(result);
    }
}