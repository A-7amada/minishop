using Microsoft.AspNetCore.Mvc;
using MiniShop.Application.Services;
namespace MiniShop.Web.Controllers;
public class AssistantController(AssistantService assistant):Controller
{
 [HttpPost,ValidateAntiForgeryToken]
 public async Task<IActionResult> Ask(string question)=>Json(new{answer=await assistant.AnswerAsync(question,System.Globalization.CultureInfo.CurrentUICulture.TwoLetterISOLanguageName=="ar",User.Identity?.IsAuthenticated==true?User.Identity.Name:null)});
}
