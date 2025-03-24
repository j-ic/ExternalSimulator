namespace OutputSimulator.MQTT;

public class BrokerConfig
{
	public required string Ip { get; set; }
	public int Port { get; set; }
	public bool UseTls { get; set; }
	public bool UseCertification { get; set; }
	public string? CaCertificationPath { get; set; }
	public string? ClientCertificationPath { get; set; }
	public string? ClientKeyPath { get; set; }
}
