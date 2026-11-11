using System.Net;
using HtmlAgilityPack;
using Yukari.Core.Models;
using Yukari.Core.Sources;
using Yukari.Core.Sources.Http;

namespace Yukari.Plugin.MangaLivre;

[ComicSourceMetadata(
    "MangaLivre",
    "1.1.0+core2.6.0",
    "https://github.com/Yukari-App/Plugin.MangaLivre/releases",
    "https://mangalivre.blog/favicon.ico",
    "Filtros de status e avaliação só são aplicados quando a busca está vazia."
)]

public partial class MangaLivreSource : IComicSource, IRequiresHttpClient
{
    private const int DefaultPageSize = 32;
    private const string SourceLanguage = "pt-BR";

    private const string BaseUrl = "https://mangalivre.blog";

    private ISharedHttpClient? _httpClient;

    public IReadOnlyList<Filter> Filters => _filters;
    public IReadOnlyDictionary<string, string> Languages => _languages;

    public void SetHttpClient(ISharedHttpClient httpClient) => _httpClient = httpClient;

    public async Task<IReadOnlyList<Comic>> SearchAsync(
        string query,
        IReadOnlyDictionary<string, IReadOnlyList<string>> filters,
        int page = 1,
        CancellationToken ct = default
    )
    {
        if (string.IsNullOrWhiteSpace(query))
            return await GetFilteredComicsAsync(filters, page, ct);

        var queryParams = BuildFilterQueryParams(filters);
        queryParams["s"] = [query.Trim()];

        string searchUrl = page <= 1
            ? $"{BaseUrl}/?{ToQueryString(queryParams)}"
            : $"{BaseUrl}/page/{page}/?{ToQueryString(queryParams)}";

        var html = await GetHTMLAsync(searchUrl, ct);
        if (html == null)
            return Array.Empty<Comic>();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        return ParseMangaCards(doc);
    }

    public async Task<IReadOnlyList<Comic>> GetTrendingAsync(
        IReadOnlyDictionary<string, IReadOnlyList<string>> filters,
        int page = 1,
        CancellationToken ct = default
    )
    {
        if (HasActiveFilters(filters))
            return await GetFilteredComicsAsync(filters, page, ct);

        string url = page <= 1
            ? $"{BaseUrl}/"
            : $"{BaseUrl}/?atualizacoes={page}";

        var html = await GetHTMLAsync(url, ct);

        if (html == null)
            return Array.Empty<Comic>();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var mangaCards = doc.DocumentNode.SelectNodes(
            "//h2[contains(normalize-space(.), 'ÚLTIMAS ATUALIZAÇÕES')]" +
            "/ancestor::*[.//article[contains(@class, 'home-manga-card')]][1]" +
            "//article[contains(@class, 'home-manga-card')]"
        );

        if (mangaCards is not { Count: > 0 })
            return Array.Empty<Comic>();

        var comics = new List<Comic>();

        foreach (var mangaCard in mangaCards)
        {
            var linkNode = mangaCard.SelectSingleNode(
                ".//a[contains(@class, 'home-manga-cover')]"
            );

            string? comicUrl = linkNode?.GetAttributeValue("href", null);

            if (string.IsNullOrWhiteSpace(comicUrl))
                continue;

            string? comicId = ExtractIdFromUrl(comicUrl);

            if (string.IsNullOrWhiteSpace(comicId))
                continue;

            var titleNode = mangaCard.SelectSingleNode(".//h3/a");

            string title = titleNode?.InnerText.Trim() ?? "Unknown Title";

            var imageNode = mangaCard.SelectSingleNode(
                ".//img[contains(@class, 'manga-cover-img')]"
            );

            string? coverUrl = imageNode?.GetAttributeValue("src", null);

            comics.Add(
                new Comic(
                    Id: comicId,
                    ComicUrl: comicUrl,
                    Title: title,
                    Author: null,
                    Description: null,
                    Tags: [],
                    Year: null,
                    CoverImageUrl: coverUrl,
                    Langs: ["pt-BR"]
                )
            );
        }
        return comics
            .DistinctBy(comic => comic.Id, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private async Task<IReadOnlyList<Comic>> GetFilteredComicsAsync(
        IReadOnlyDictionary<string, IReadOnlyList<string>> filters,
        int page,
        CancellationToken ct
    )
    {
        var queryParams = BuildFilterQueryParams(filters);

        if (page > 1)
            queryParams["paged"] = [page.ToString()];

        string searchUrl = queryParams.Count > 0
            ? $"{BaseUrl}/pesquisa/?{ToQueryString(queryParams)}"
            : $"{BaseUrl}/pesquisa/";

        var html = await GetHTMLAsync(searchUrl, ct);
        if (html == null)
            return Array.Empty<Comic>();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        return ParseMangaCards(doc);
    }

    private IReadOnlyList<Comic> ParseMangaCards(HtmlDocument doc)
    {
        var mangaCards = doc.DocumentNode.SelectNodes(
            "//div[contains(concat(' ', normalize-space(@class), ' '), ' manga-card ')]"
        );

        if (mangaCards is not { Count: > 0 })
            return Array.Empty<Comic>();

        var comics = new List<Comic>();
        foreach (var mangaCard in mangaCards)
        {
            var linkNode = mangaCard.SelectSingleNode(
                ".//a[contains(@class, 'manga-card-link')]"
            );

            string? comicUrl = linkNode?.GetAttributeValue("href", null);
            if (string.IsNullOrEmpty(comicUrl))
                continue;

            string? comicId = ExtractIdFromUrl(comicUrl);
            if (string.IsNullOrEmpty(comicId))
                continue;

            var titleNode = mangaCard.SelectSingleNode(
                ".//h3[contains(@class, 'manga-card-title')]"
            );

            var imageNode = mangaCard.SelectSingleNode(
                ".//img[contains(@class, 'manga-cover-img')]"
            );

            string? coverUrl = imageNode?.GetAttributeValue("src", null);

            string title = titleNode?.InnerText.Trim() ?? string.Empty;

            // The advanced search page (/pesquisa/) renders the page title
            // ("pesquisa") in every card title, so fall back to the cover alt text.
            if (
                string.IsNullOrWhiteSpace(title)
                || title.Equals("pesquisa", StringComparison.OrdinalIgnoreCase)
            )
                title = imageNode?.GetAttributeValue("alt", null)?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(title))
                title = GetTitleFromId(comicId);

            comics.Add(
                new Comic(
                    Id: comicId,
                    ComicUrl: comicUrl,
                    Title: title,
                    Author: null,
                    Description: null,
                    Tags: [],
                    Year: null,
                    CoverImageUrl: coverUrl,
                    Langs: ["pt-BR"]
                )
            );
        }

        return comics;
    }

    private static Dictionary<string, string[]> BuildFilterQueryParams(
        IReadOnlyDictionary<string, IReadOnlyList<string>> filters
    )
    {
        var queryParams = new Dictionary<string, string[]>();

        foreach (var kvp in filters)
        {
            var values = kvp.Value
                .Where(value => !string.IsNullOrWhiteSpace(value))
                .ToArray();

            if (values.Length > 0)
                queryParams[kvp.Key] = values;
        }

        return queryParams;
    }

    private static bool HasActiveFilters(
        IReadOnlyDictionary<string, IReadOnlyList<string>> filters
    ) =>
        filters.Values.Any(values =>
            values.Any(value => !string.IsNullOrWhiteSpace(value))
        );

    private static string GetTitleFromId(string id) =>
        string.Join(
            ' ',
            id.Split('-', StringSplitOptions.RemoveEmptyEntries)
                .Select(word => char.ToUpperInvariant(word[0]) + word[1..])
        );



    public async Task<Comic?> GetDetailsAsync(string comicId, CancellationToken ct = default)
    {
        string detailsUrl = $"{BaseUrl}/manga/{comicId}";

        var html = await GetHTMLAsync(detailsUrl, ct);
        if (html == null)
            return null;

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var titleNode = doc.DocumentNode.SelectSingleNode("//h1[contains(@class, 'manga-title')]");
        string title = titleNode?.InnerText.Trim() ?? "Unknown Title";

        var authorNodes = doc.DocumentNode.SelectNodes("//div[@class='manga-meta-grid']//div[contains(@class, 'manga-meta-item')][span[@class='meta-label' and contains(., 'Autor:')]]/span[@class='meta-value']");
        string[] authors =
            authorNodes?.Select(a => a.InnerText.Trim()).ToArray() ?? Array.Empty<string>();

        var tagNodes = doc.DocumentNode.SelectNodes("//span[contains(@class, 'manga-tag')]//a");
        string[] tags =
            tagNodes?.Select(t => t.InnerText.Trim()).ToArray() ?? Array.Empty<string>();

        var statusNode = doc.DocumentNode.SelectSingleNode("//div[@class='manga-meta-grid']//div[contains(@class, 'manga-meta-item')][span[@class='meta-label' and contains(., 'status:')]]/span[@class='meta-value']");
        ComicStatus status = GetComicStatus(statusNode?.InnerText.Trim());

        var yearNode = doc.DocumentNode.SelectSingleNode("//div[@class='manga-meta-grid']//div[contains(@class, 'manga-meta-item')][span[@class='meta-label' and contains(., 'ano:')]]/span[@class='meta-value']");
        string yearStr = yearNode?.InnerText.Trim() ?? "";
        int? year = int.TryParse(yearStr, out int y) ? y : null;

        var descriptionNode = doc.DocumentNode.SelectSingleNode(
            "//div[contains(@class, 'manga-synopsis')]//div[@class='synopsis-content']"
        );
        string? description = descriptionNode?.InnerText.Trim();

        var coverNode = doc.DocumentNode.SelectSingleNode(
            "//img[contains(@class, 'manga-cover-image')]"
        );

        string? coverUrl = coverNode?.GetAttributeValue("src", null);

        return new Comic(
            Id: comicId,
            ComicUrl: detailsUrl,
            Title: title,
            Author: authors.FirstOrDefault(),
            Description: description,
            Tags: tags,
            Year: year,
            coverUrl,
            Langs: [SourceLanguage],
            Status: status
        );
    }

    public async Task<IReadOnlyList<Chapter>> GetAllChaptersAsync(
        string comicId,
        string language,
        CancellationToken ct = default
    )
    {
        var chaptersUrl = $"{BaseUrl}/manga/{comicId}";

        var html = await GetHTMLAsync(chaptersUrl, ct);
        if (html == null)
            return Array.Empty<Chapter>();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var chapterNodes = doc.DocumentNode.SelectNodes(
            "//article[contains(@class, 'chapter-grid-item')]"
        );

        if (chapterNodes is not { Count: > 0 })
            return Array.Empty<Chapter>();

        var chapters = new List<Chapter>();

        foreach (var node in chapterNodes.Reverse())
        {
            string? chapterNumberString =
                node.GetAttributeValue("data-chapter-number", null);

            var linkNode = node.SelectSingleNode(
                ".//a[contains(@class, 'chapter-grid-number')]"
            );

            var titleNode = linkNode?.SelectSingleNode(".//strong");

            string title = titleNode?.InnerText.Trim() ?? "Unknown";

            string? chapterUrl =
                linkNode?.GetAttributeValue("href", null);

            string? chapterId =
                ExtractChapterIdFromUrl(chapterUrl);

            if (string.IsNullOrEmpty(chapterId))
                continue;

            chapters.Add(
                new Chapter(
                    Id: chapterId,
                    Title: title,
                    Number: chapterNumberString,
                    Volume: null,
                    Language: language,
                    Groups: Array.Empty<string>(),
                    LastUpdate: null,
                    Pages: null
                )
            );
        }

        return chapters;
    }

    public async Task<IReadOnlyList<ChapterPage>> GetChapterPagesAsync(
        string comicId,
        string chapterId,
        CancellationToken ct = default
    )
    {
        var pagesUrl = $"{BaseUrl}/capitulo/{chapterId}/";

        var html = await GetHTMLAsync(pagesUrl, ct);
        if (html == null)
            return Array.Empty<ChapterPage>();

        var doc = new HtmlDocument();
        doc.LoadHtml(html);

        var imgNodes = doc.DocumentNode.SelectNodes(
            "//img[contains(@class, 'chapter-image')]"
        );

        if (imgNodes is not { Count: > 0 })
            return Array.Empty<ChapterPage>();

        var pages = new List<ChapterPage>(imgNodes.Count);

        for (int i = 0; i < imgNodes.Count; i++)
        {
            var img = imgNodes[i];

            string imageUrl = img.GetAttributeValue("src", "");

            if (string.IsNullOrEmpty(imageUrl))
                continue;

            pages.Add(
                new ChapterPage(
                    Number: i + 1,
                    ImageUrl: imageUrl
                )
            );
        }

        return pages;
    }

    public async Task<byte[]?> GetImageBytesAsync(string imageUrl, CancellationToken ct = default)
    {
        if (string.IsNullOrEmpty(imageUrl))
            return null;

        using var request = new HttpRequestMessage(HttpMethod.Get, imageUrl);
        request.Headers.Add("Referer", BaseUrl);

        using var response = await _httpClient!.SendAsync(request, ct);

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
            throw new HttpRequestException(
                "MangaLivre Rate Limit Exceeded. Try again later.",
                null,
                HttpStatusCode.TooManyRequests
            );

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadAsByteArrayAsync(ct);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}