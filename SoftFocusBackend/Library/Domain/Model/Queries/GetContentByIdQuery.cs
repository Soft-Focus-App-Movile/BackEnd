namespace SoftFocusBackend.Library.Domain.Model.Queries;

public class GetContentByIdQuery
{
    public string ContentId { get; set; } = string.Empty;

    public GetContentByIdQuery() { }

    public GetContentByIdQuery(string contentId)
    {
        ContentId = contentId;
    }

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ContentId))
            throw new ArgumentException("ContentId no puede estar vacío");
    }
}
