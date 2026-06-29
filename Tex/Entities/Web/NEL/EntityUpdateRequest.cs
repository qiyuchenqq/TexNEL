using System.Text.Json.Serialization;

namespace Tex.Entities.Web.NEL;

public class EntityUpdateRequest
{
	[JsonPropertyName("id")]
	public string UserId { get; set; } = string.Empty;

	[JsonPropertyName("token")]
	public string AccessToken { get; set; } = string.Empty;
}

