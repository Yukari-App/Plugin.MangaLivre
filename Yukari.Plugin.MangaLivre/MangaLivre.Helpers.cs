using System.Net;
using Yukari.Core.Models;

namespace Yukari.Plugin.MangaLivre;

public partial class MangaLivreSource
{
    private static IReadOnlyList<Filter>? _filters =
    [
        new Filter(
                Key: "status",
                DisplayName: "Status",
                [
                    new FilterOption("", "Todos", true),
                    new FilterOption("concluido", "Concluído", false),
                    new FilterOption("em-andamento", "Em Andamento", false),
                ],
                AllowMultiple: false
            ),

            new Filter(
                Key: "genre",
                DisplayName: "Gênero",
                [
                    new FilterOption("", "Todos", true),
                    new FilterOption("absolute-cinema", "ABSOLUTE CINEMA", false),
                    new FilterOption("acao", "Ação", false),
                    new FilterOption("action", "Action", false),
                    new FilterOption("adaptation", "Adaptação", false),
                    new FilterOption("adulto", "Adulto", false),
                    new FilterOption("adventure", "Aventura", false),
                    new FilterOption("aliens", "Alienígenas", false),
                    new FilterOption("animals", "Animais", false),
                    new FilterOption("apocalipse", "Apocalipse", false),
                    new FilterOption("arte-foda", "Arte Foda", false),
                    new FilterOption("artes-marciais", "Artes Marciais", false),
                    new FilterOption("aventura", "Aventura", false),
                    new FilterOption("award-winning", "Prêmio de Destaque", false),
                    new FilterOption("boys-love", "Boys Love", false),
                    new FilterOption("colorido", "Colorido", false),
                    new FilterOption("comedia", "Comédia", false),
                    new FilterOption("comedy", "Comédia", false),
                    new FilterOption("comic", "Comic", false),
                    new FilterOption("construcao", "Construção", false),
                    new FilterOption("coreana", "Coreana", false),
                    new FilterOption("crime", "Crime", false),
                    new FilterOption("cultivo", "Cultivo", false),
                    new FilterOption("culto", "Culto", false),
                    new FilterOption("delinquents", "Delinquents", false),
                    new FilterOption("demonios", "Demonios", false),
                    new FilterOption("demons", "Demons", false),
                    new FilterOption("doujinshi", "Doujinshi", false),
                    new FilterOption("dragoes", "Dragoes", false),
                    new FilterOption("drama", "Drama", false),
                    new FilterOption("dungeon", "Dungeon", false),
                    new FilterOption("ecchi", "Ecchi", false),
                    new FilterOption("escola", "Escola", false),
                    new FilterOption("escolar", "Escolar", false),
                    new FilterOption("esportes", "Esportes", false),
                    new FilterOption("estrategia", "Estratégia", false),
                    new FilterOption("evolucao", "Evolução", false),
                    new FilterOption("familia", "Família", false),
                    new FilterOption("fantasia", "Fantasia", false),
                    new FilterOption("fantasy", "Fantasy", false),
                    new FilterOption("ficcao-cientifica", "Ficção Científica", false),
                    new FilterOption("full-collor", "Full Color", false),
                    new FilterOption("full-color", "Full Color", false),
                    new FilterOption("genio", "Gênio", false),
                    new FilterOption("ghosts", "Ghosts", false),
                    new FilterOption("girls-love", "Girls Love", false),
                    new FilterOption("gore", "Gore", false),
                    new FilterOption("harem", "Harem", false),
                    new FilterOption("hentai", "Hentai", false),
                    new FilterOption("heterossexual", "Heterossexual", false),
                    new FilterOption("historical", "Historical", false),
                    new FilterOption("historico", "Histórico", false),
                    new FilterOption("horror", "Horror", false),
                    new FilterOption("isekai", "Isekai", false),
                    new FilterOption("jogo", "Jogo", false),
                    new FilterOption("jogos", "Jogos", false),
                    new FilterOption("josei", "Josei", false),
                    new FilterOption("kemonomimi", "Kemonomimi", false),
                    new FilterOption("litrpg", "Litrpg", false),
                    new FilterOption("loli", "Loli", false),
                    new FilterOption("long-strip", "Long Strip", false),
                    new FilterOption("luta", "Luta", false),
                    new FilterOption("mafia", "Mafia", false),
                    new FilterOption("magia", "Magia", false),
                    new FilterOption("magic", "Magic", false),
                    new FilterOption("magical-girls", "Magical Girls", false),
                    new FilterOption("maldicao", "Maldição", false),
                    new FilterOption("martial-arts", "Martial Arts", false),
                    new FilterOption("mecha", "Mecha", false),
                    new FilterOption("milf", "MILF", false),
                    new FilterOption("militar", "Militar", false),
                    new FilterOption("military", "Military", false),
                    new FilterOption("misterio", "Misterio", false),
                    new FilterOption("monster-girls", "Monster Girls", false),
                    new FilterOption("monsters", "Monsters", false),
                    new FilterOption("monstros", "Monstros", false),
                    new FilterOption("murim", "Murim", false),
                    new FilterOption("mystery", "Mystery", false),
                    new FilterOption("ninja", "Ninja", false),
                    new FilterOption("oda-genio", "Oda Genio", false),
                    new FilterOption("office-workers", "Office Workers", false),
                    new FilterOption("oneshot", "Oneshot", false),
                    new FilterOption("overpower", "Overpower", false),
                    new FilterOption("peak", "Peak", false),
                    new FilterOption("peak-arte", "Peak Arte", false),
                    new FilterOption("petel-o-leal", "Petel O Leal", false),
                    new FilterOption("philosophical", "Philosophical", false),
                    new FilterOption("police", "Police", false),
                    new FilterOption("pornhwa", "Pornhwa", false),
                    new FilterOption("post-apocalyptic", "Post-Apocalyptic", false),
                    new FilterOption("psicologico", "Psicologico", false),
                    new FilterOption("psychological", "Psychological", false),
                    new FilterOption("realidade-virtual", "Realidade Virtual", false),
                    new FilterOption("reencarnacao", "Reencarnacao", false),
                    new FilterOption("regressao", "Regressao", false),
                    new FilterOption("reincarnation", "Reincarnation", false),
                    new FilterOption("retorno", "Retorno", false),
                    new FilterOption("romance", "Romance", false),
                    new FilterOption("samurai", "Samurai", false),
                    new FilterOption("school-life", "School Life", false),
                    new FilterOption("sci-fi", "Sci-Fi", false),
                    new FilterOption("seinen", "Seinen", false),
                    new FilterOption("sexual-violence", "Sexual Violence", false),
                    new FilterOption("shoujo", "Shoujo", false),
                    new FilterOption("shounen", "Shounen", false),
                    new FilterOption("sistema", "Sistema", false),
                    new FilterOption("slice-of-life", "Slice of Life", false),
                    new FilterOption("sobrenatural", "Sobrenatural", false),
                    new FilterOption("sports", "Sports", false),
                    new FilterOption("super-poderes", "Super Poderes", false),
                    new FilterOption("superacao", "Superacao", false),
                    new FilterOption("supernatural", "Supernatural", false),
                    new FilterOption("survival", "Survival", false),
                    new FilterOption("suspense", "Suspense", false),
                    new FilterOption("tela-de-sistema", "Tela de Sistema", false),
                    new FilterOption("terror", "Terror", false),
                    new FilterOption("thriller", "Thriller", false),
                    new FilterOption("time-travel", "Time Travel", false),
                    new FilterOption("tragedia", "Tragedia", false),
                    new FilterOption("tragedy", "Tragedy", false),
                    new FilterOption("traicao", "Traicao", false),
                    new FilterOption("vampiros", "Vampiros", false),
                    new FilterOption("viagem", "Viagem", false),
                    new FilterOption("viagem-no-tempo", "Viagem no Tempo", false),
                    new FilterOption("vida-escolar", "Vida Escolar", false),
                    new FilterOption("video-games", "Video Games", false),
                    new FilterOption("vilao", "Vilao", false),
                    new FilterOption("villainess", "Villainess", false),
                    new FilterOption("vinganca", "Vinganca", false),
                    new FilterOption("web-comic", "Web Comic", false),
                    new FilterOption("wuxia", "Wuxia", false),
                    new FilterOption("xuanhuan", "Xuanhuan", false),
                    new FilterOption("yandere", "Yandere", false),
                ],
                AllowMultiple: false
            ),

            new Filter(
                Key: "rating_min",
                DisplayName: "Avaliação mínima",
                [
                    new FilterOption("", "Qualquer", true),
                    new FilterOption("1", "1 ou mais", false),
                    new FilterOption("2", "2 ou mais", false),
                    new FilterOption("3", "3 ou mais", false),
                    new FilterOption("4", "4 ou mais", false),
                    new FilterOption("5", "5 ou mais", false),
                    new FilterOption("6", "6 ou mais", false),
                    new FilterOption("7", "7 ou mais", false),
                    new FilterOption("8", "8 ou mais", false),
                    new FilterOption("9", "9 ou mais", false),
                    new FilterOption("10", "10 ou mais", false),
                ],
                AllowMultiple: false
            ),
    ];
    private static readonly IReadOnlyDictionary<string, string>
    _languages = new Dictionary<string, string> { { "pt-BR", "Português (BR)" } };

    private async Task<string?> GetHTMLAsync(string url, CancellationToken ct = default)
    {
        using var response = await _httpClient!.SendAsync(
            new HttpRequestMessage(HttpMethod.Get, url),
            ct
        );

        if (response.StatusCode == HttpStatusCode.TooManyRequests)
            throw new HttpRequestException(
                "MangaLivre Rate Limit Exceeded. Try again later.",
                null,
                HttpStatusCode.TooManyRequests
            );

        if (response.StatusCode is HttpStatusCode.BadRequest or HttpStatusCode.NotFound)
            return default;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(ct);
    }

    private ComicStatus GetComicStatus(string? status) =>
        status switch
        {
            _ => ComicStatus.Unknown,
        };

    private string? ExtractIdFromUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return null;
        var parts = url.TrimEnd('/').Split('/');
        return parts.Length > 0 ? parts[^1] : null;
    }

    private string? ExtractChapterIdFromUrl(string? url)
    {
        if (string.IsNullOrEmpty(url))
            return null;
        var parts = url.TrimEnd('/').Split('/');
        return parts.Length > 0 ? parts[^1] : null;
    }

    private static string ToQueryString(Dictionary<string, string[]> source) =>
        string.Join(
            "&",
            source.SelectMany(kvp =>
                kvp.Value.Select(v => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(v)}")
            )
        );
}