using System.Text.Json.Serialization;

namespace FinTrack.Domain.Enums;

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum HistoryActionType
{
    Created = 1,
    Updated = 2,
    Deleted = 3,
    Restored = 4
}
