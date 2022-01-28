using Json.Generator;

namespace ExampleApi;

[JsonFieldExists]
public partial class StationModel
{
    public string Name { get; set; }
    public DateTime CreatedAt { get; set; }
}