using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;

namespace GatewaySimulator.Controllers;

[ApiController]
[Route("api")]
public class IDonEvenCareController : ControllerBase
{
    #region constructor

    public IDonEvenCareController(ILogger<IDonEvenCareController> logger)
    {
        _logger = logger;
    }

    #endregion

    #region public methods

    [HttpPost("OPCUA/GetOPCModelInfo")]
    public ActionResult GetOPCModelInfo()
    {
        byte[] opcModelInfo = System.IO.File.ReadAllBytes("qwer2InformationModel.xml");

        var response = new { modelXml = new { data = opcModelInfo } };

        return Ok(response);
    }

    [HttpPost("OPCUA/GetMqttCertification")]
    public ActionResult GetMqttCertification()
    {
        var response = new 
        { 
            GROUP_ID = 21,
            MQTT_SERVER_HOST = "mqtt-input.flexing.ai",
            MQTT_SERVER_PORT = "1890",
            CA_CERT_FILE = Convert.ToBase64String(
                Encoding.UTF8.GetBytes("ca.crt")),
            CLIENT_CERT_FILE = Convert.ToBase64String(
                Encoding.UTF8.GetBytes("client.crt")),
            CLIENT_KEY_FILE = Convert.ToBase64String(
                Encoding.UTF8.GetBytes("client.key")),
        };

        string responseJson = JsonSerializer.Serialize(response);

        return Ok(responseJson);
    }

    #endregion

    #region private fields

    private readonly ILogger<IDonEvenCareController> _logger;

    #endregion
}
