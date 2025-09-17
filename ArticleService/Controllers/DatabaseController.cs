using Microsoft.AspNetCore.Mvc;

namespace ArticleService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DatabaseController : ControllerBase
{
    private Database database = Database.GetInstance();
    
    [HttpDelete]
    public void Delete()
    {
        database.DeleteDatabase();
    }

    [HttpPost]
    public void Post()
    {
        database.RecreateDatabase();
    }
}